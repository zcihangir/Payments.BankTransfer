﻿using Grand.Business.Core.Extensions;
using Grand.Business.Core.Interfaces.Common.Directory;
using Grand.Business.Core.Interfaces.Common.Localization;
using Grand.Business.Core.Interfaces.Common.Stores;
using Grand.Business.Core.Interfaces.Messages;
using Grand.Business.Core.Utilities.Messages.DotLiquidDrops;
using Grand.Domain.Common;
using Grand.Domain.Customers;
using Grand.Domain.Localization;
using Grand.Domain.Messages;
using Grand.Domain.Orders;
using Grand.Domain.Stores;
using Grand.Infrastructure;
using Grand.SharedKernel.Extensions;
using MediatR;
using Payments.BankTransfer.Domain;

namespace Payments.BankTransfer.Infrastructure
{
    internal class BankTransferMessageProvider : IBankTransferMessageProvider
    {
        #region Fields

        private readonly IMessageTemplateService _messageTemplateService;
        private readonly IQueuedEmailService _queuedEmailService;
        private readonly ILanguageService _languageService;
        private readonly IEmailAccountService _emailAccountService;
        private readonly IStoreService _storeService;
        private readonly IStoreHelper _storeHelper;
        private readonly IMediator _mediator;
        private readonly EmailAccountSettings _emailAccountSettings;
        private readonly IUserFieldService _userFieldService;

        #endregion

        #region Ctor

        public BankTransferMessageProvider(IMessageTemplateService messageTemplateService,
            IQueuedEmailService queuedEmailService,
            ILanguageService languageService,
            IEmailAccountService emailAccountService,
            IStoreService storeService,
            IStoreHelper storeHelper,
            IMediator mediator,
            EmailAccountSettings emailAccountSettings,
            IUserFieldService userFieldService)
        {
            _messageTemplateService = messageTemplateService;
            _queuedEmailService = queuedEmailService;
            _languageService = languageService;
            _emailAccountService = emailAccountService;
            _storeService = storeService;
            _storeHelper = storeHelper;
            _emailAccountSettings = emailAccountSettings;
            _userFieldService = userFieldService;
            _mediator = mediator;
        }

        #endregion

        #region Utilities

        protected virtual async Task<Store> GetStore(string storeId)
        {
            return await _storeService.GetStoreById(storeId) ?? (await _storeService.GetAllStores()).FirstOrDefault();
        }

        protected virtual async Task<MessageTemplate> GetMessageTemplate(string messageTemplateName, string storeId)
        {
            var messageTemplate = await _messageTemplateService.GetMessageTemplateByName(messageTemplateName, storeId);

            //no template found
            if (messageTemplate == null)
                return null;

            //ensure it's active
            var isActive = messageTemplate.IsActive;
            return !isActive ? null : messageTemplate;
        }

        protected virtual async Task<EmailAccount> GetEmailAccountOfMessageTemplate(MessageTemplate messageTemplate, string languageId)
        {
            var emailAccounId = messageTemplate.GetTranslation(mt => mt.EmailAccountId, languageId);
            var emailAccount = (await _emailAccountService.GetEmailAccountById(emailAccounId) ?? await _emailAccountService.GetEmailAccountById(_emailAccountSettings.DefaultEmailAccountId)) ??
                               (await _emailAccountService.GetAllEmailAccounts()).FirstOrDefault();
            return emailAccount;

        }

        protected virtual async Task<Language> EnsureLanguageIsActive(string languageId, string storeId)
        {
            //load language by specified ID
            var language = await _languageService.GetLanguageById(languageId);

            if (language is not { Published: true })
            {
                //load any language from the specified store
                language = (await _languageService.GetAllLanguages(storeId: storeId)).FirstOrDefault();
            }
            if (language is not { Published: true })
            {
                //load any language
                language = (await _languageService.GetAllLanguages()).FirstOrDefault();
            }

            if (language == null)
                throw new Exception("No active language could be loaded");
            return language;
        }

        #endregion

        public async Task<int> SendQrPaymentMessage(Order order, Customer customer, string languageId)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            var store = await GetStore(order.StoreId);
            var language = await EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = await GetMessageTemplate(BankTransferPaymentDefaults.EmailPaymentQrCode, store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = await GetEmailAccountOfMessageTemplate(messageTemplate, language.Id);

            var liquidBuilder = new LiquidObjectBuilder(_mediator);

            liquidBuilder
                .AddStoreTokens(store, language, emailAccount)
                .AddOrderTokens(order, customer, store, _storeHelper.DomainHost);

            if (customer != null)
                liquidBuilder.AddCustomerTokens(customer, store, _storeHelper.DomainHost, language);

            LiquidObject liquidObject = await liquidBuilder.BuildAsync();

            string variableSymbol = await _userFieldService.GetFieldsForEntity<string>(order, InvoiceConstants.INVOICE_VARIABLE_SYMBOL_FIELD_KEY);
            liquidObject.AdditionalTokens.Add("Order_VariableSymbol", variableSymbol);

            //event notification
            await _mediator.MessageTokensAdded(messageTemplate, liquidObject);
            var toEmail = order.BillingAddress.Email;
            var toName = $"{order.BillingAddress.FirstName} {order.BillingAddress.LastName}";

            return await SendNotification(messageTemplate, emailAccount,
                languageId, liquidObject,
                toEmail, toName,
                reference: Reference.Order, objectId: order.Id);
        }

        #region Utility

        public virtual async Task<int> SendNotification(MessageTemplate messageTemplate,
            EmailAccount emailAccount, string languageId, LiquidObject liquidObject,
            string toEmailAddress, string toName,
            string attachmentFilePath = null, string attachmentFileName = null,
            IEnumerable<string> attachedDownloads = null,
            string replyToEmailAddress = null, string replyToName = null,
            string fromEmail = null, string fromName = null, string subject = null,
            Reference reference = Reference.None, string objectId = "")
        {
            if (string.IsNullOrEmpty(toEmailAddress))
                return 0;

            //retrieve translation message template data
            var bcc = messageTemplate.GetTranslation(mt => mt.BccEmailAddresses, languageId);

            if (string.IsNullOrEmpty(subject))
                subject = messageTemplate.GetTranslation(mt => mt.Subject, languageId);

            var body = messageTemplate.GetTranslation(mt => mt.Body, languageId);

            var email = new QueuedEmail();
            liquidObject.Email = new LiquidEmail(email.Id);

            var subjectReplaced = LiquidExtensions.Render(liquidObject, subject);
            var bodyReplaced = LiquidExtensions.Render(liquidObject, body);

            var attachments = new List<string>();
            if (attachedDownloads != null)
                attachments.AddRange(attachedDownloads);
            if (!string.IsNullOrEmpty(messageTemplate.AttachedDownloadId))
                attachments.Add(messageTemplate.AttachedDownloadId);

            //limit name length
            toName = CommonHelper.EnsureMaximumLength(toName, 300);
            email.PriorityId = QueuedEmailPriority.High;
            email.From = !string.IsNullOrEmpty(fromEmail) ? fromEmail : emailAccount.Email;
            email.FromName = !string.IsNullOrEmpty(fromName) ? fromName : emailAccount.DisplayName;
            email.To = toEmailAddress;
            email.ToName = toName;
            email.ReplyTo = replyToEmailAddress;
            email.ReplyToName = replyToName;
            email.CC = string.Empty;
            email.Bcc = bcc;
            email.Subject = subjectReplaced;
            email.Body = bodyReplaced;
            email.AttachmentFilePath = attachmentFilePath;
            email.AttachmentFileName = attachmentFileName;
            email.AttachedDownloads = attachments;
            email.CreatedOnUtc = DateTime.UtcNow;
            email.EmailAccountId = emailAccount.Id;
            email.DontSendBeforeDateUtc = !messageTemplate.DelayBeforeSend.HasValue ? null
                    : (DateTime?)(DateTime.UtcNow + TimeSpan.FromHours(messageTemplate.DelayPeriodId.ToHours(messageTemplate.DelayBeforeSend.Value)));
            email.Reference = reference;
            email.ObjectId = objectId;

            await _queuedEmailService.InsertQueuedEmail(email);
            return 1;
        }

        #endregion
    }
}

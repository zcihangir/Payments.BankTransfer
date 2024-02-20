using Grand.Business.Core.Interfaces.Common.Configuration;
using Grand.Business.Core.Interfaces.Common.Localization;
using Grand.Business.Core.Interfaces.Common.Stores;
using Grand.Business.Core.Utilities.Common.Security;
using Grand.Domain.Common;
using Grand.Domain.Customers;
using Grand.Infrastructure;
using Grand.Web.Common.Controllers;
using Grand.Web.Common.Filters;
using Grand.Web.Common.Security.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payments.BankTransfer.Models;

namespace Payments.BankTransfer.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    [Area("Admin")]
    [PermissionAuthorize(PermissionSystemName.PaymentMethods)]
    public class PaymentBankTransferController : BasePaymentController
    {
        private readonly IWorkContext _workContext;
        private readonly IStoreService _storeService;
        private readonly ISettingService _settingService;
        private readonly ITranslationService _translationService;


        public PaymentBankTransferController(IWorkContext workContext,
            IStoreService storeService,
            ISettingService settingService,
            ITranslationService translationService)
        {
            _workContext = workContext;
            _storeService = storeService;
            _settingService = settingService;
            _translationService = translationService;
        }


        protected virtual async Task<string> GetActiveStore(IStoreService storeService, IWorkContext workContext)
        {
            var stores = await storeService.GetAllStores();
            if (stores.Count < 2)
                return stores.FirstOrDefault().Id;

            var storeId = workContext.CurrentCustomer.GetUserFieldFromEntity<string>(SystemCustomerFieldNames.AdminAreaStoreScopeConfiguration);
            var store = await storeService.GetStoreById(storeId);

            return store != null ? store.Id : "";
        }

        public async Task<IActionResult> Configure()
        {
            //load settings for a chosen store scope
            var storeScope = await this.GetActiveStore(_storeService, _workContext);
            var bankTransferPaymentSettings = _settingService.LoadSetting<BankTransferPaymentSettings>(storeScope);

            var model = new ConfigurationModel {
                DescriptionText = bankTransferPaymentSettings.DescriptionText,
                AdditionalFee = bankTransferPaymentSettings.AdditionalFee,
                AdditionalFeePercentage = bankTransferPaymentSettings.AdditionalFeePercentage,
                ShippableProductRequired = bankTransferPaymentSettings.ShippableProductRequired,
                DisplayOrder = bankTransferPaymentSettings.DisplayOrder,
                SkipPaymentInfo = bankTransferPaymentSettings.SkipPaymentInfo,
                IBAN = bankTransferPaymentSettings.IBAN,
                SWIFT = bankTransferPaymentSettings.SWIFT,
                VariableSymbolPattern = bankTransferPaymentSettings.VariableSymbolPattern,
                QrCodeStringPattern = bankTransferPaymentSettings.QrCodeStringPattern,
            };
            model.DescriptionText = bankTransferPaymentSettings.DescriptionText;

            model.ActiveStore = storeScope;

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return await Configure();

            //load settings for a chosen store scope
            var storeScope = await this.GetActiveStore(_storeService, _workContext);
            var bankTransferPaymentSettings = _settingService.LoadSetting<BankTransferPaymentSettings>(storeScope);

            //save settings
            bankTransferPaymentSettings.DescriptionText = model.DescriptionText;
            bankTransferPaymentSettings.AdditionalFee = model.AdditionalFee;
            bankTransferPaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;
            bankTransferPaymentSettings.ShippableProductRequired = model.ShippableProductRequired;
            bankTransferPaymentSettings.DisplayOrder = model.DisplayOrder;
            bankTransferPaymentSettings.SkipPaymentInfo = model.SkipPaymentInfo;
            bankTransferPaymentSettings.IBAN = model.IBAN;
            bankTransferPaymentSettings.SWIFT = model.SWIFT;
            bankTransferPaymentSettings.VariableSymbolPattern = model.VariableSymbolPattern;
            bankTransferPaymentSettings.QrCodeStringPattern = model.QrCodeStringPattern;

            await _settingService.SaveSetting(bankTransferPaymentSettings, storeScope);

            //now clear settings cache
            await _settingService.ClearCache();

            Success(_translationService.GetResource("Admin.Plugins.Saved"));

            return await Configure();
        }

    }
}

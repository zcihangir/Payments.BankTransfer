using Grand.Business.Core.Enums.Checkout;
using Grand.Business.Core.Interfaces.Checkout.Orders;
using Grand.Business.Core.Interfaces.Checkout.Payments;
using Grand.Business.Core.Utilities.Checkout;
using Grand.Business.Core.Interfaces.Common.Directory;
using Grand.Business.Core.Interfaces.Common.Localization;
using Grand.Domain.Orders;
using Grand.Domain.Payments;
using Grand.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Payments.BankTransfer
{
    public class BankTransferPaymentProvider : IPaymentProvider
    {
        private readonly ITranslationService _translationService;
        private readonly IServiceProvider _serviceProvider;
        private readonly BankTransferPaymentSettings _bankTransferPaymentSettings;
        private readonly IOrderService _orderService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BankTransferPaymentProvider(
            ITranslationService translationService,
            IServiceProvider serviceProvider,
            BankTransferPaymentSettings bankTransferPaymentSettings,
            IOrderService orderService,
            IHttpContextAccessor httpContextAccessor)
        {
            _translationService = translationService;
            _serviceProvider = serviceProvider;
            _bankTransferPaymentSettings = bankTransferPaymentSettings;
            _orderService = orderService;
            _httpContextAccessor = httpContextAccessor;
        }

        public string ConfigurationUrl => BankTransferPaymentDefaults.ConfigurationUrl;

        public string SystemName => BankTransferPaymentDefaults.ProviderSystemName;

        public string FriendlyName => _translationService.GetResource(BankTransferPaymentDefaults.FriendlyName);

        public int Priority => _bankTransferPaymentSettings.DisplayOrder;

        public IList<string> LimitedToStores => new List<string>();

        public IList<string> LimitedToGroups => new List<string>();

        public async Task<PaymentTransaction> InitPaymentTransaction()
        {
            return await Task.FromResult<PaymentTransaction>(null);
        }

        public async Task<ProcessPaymentResult> ProcessPayment(PaymentTransaction paymentTransaction)
        {
            var result = new ProcessPaymentResult();
            result.NewPaymentTransactionStatus = TransactionStatus.Pending;
            return await Task.FromResult(result);
        }

        public Task PostProcessPayment(PaymentTransaction paymentTransaction)
        {
            return Task.CompletedTask;
        }

        public async Task PostRedirectPayment(PaymentTransaction paymentTransaction)
        {
            var order = await _orderService.GetOrderByGuid(paymentTransaction.OrderGuid);

            _httpContextAccessor.HttpContext.Response.Redirect(BankTransferPaymentDefaults.PaymentInstructionsUrl + $"/{order.Id}");
        }

        public async Task<bool> HidePaymentMethod(IList<ShoppingCartItem> cart)
        {
            if (_bankTransferPaymentSettings.ShippableProductRequired && !cart.RequiresShipping())
                return true;

            if (String.IsNullOrEmpty(_bankTransferPaymentSettings.IBAN))
                return true;

            if (String.IsNullOrEmpty(_bankTransferPaymentSettings.SWIFT))
                return true;

            if (String.IsNullOrEmpty(_bankTransferPaymentSettings.QrCodeStringPattern))
                return true;

            if (String.IsNullOrEmpty(_bankTransferPaymentSettings.VariableSymbolPattern))
                return true;

            return await Task.FromResult(false);
        }

        public async Task<double> GetAdditionalHandlingFee(IList<ShoppingCartItem> cart)
        {
            if (_bankTransferPaymentSettings.AdditionalFee <= 0)
                return _bankTransferPaymentSettings.AdditionalFee;

            double result;
            if (_bankTransferPaymentSettings.AdditionalFeePercentage)
            {
                //percentage
                var orderTotalCalculationService = _serviceProvider.GetRequiredService<IOrderCalculationService>();
                var subtotal = await orderTotalCalculationService.GetShoppingCartSubTotal(cart, true);
                result = (double)((((float)subtotal.subTotalWithDiscount) * ((float)_bankTransferPaymentSettings.AdditionalFee)) / 100f);
            }
            else
            {
                result = _bankTransferPaymentSettings.AdditionalFee;
            }

            if (result > 0)
            {
                var currencyService = _serviceProvider.GetRequiredService<ICurrencyService>();
                var workContext = _serviceProvider.GetRequiredService<IWorkContext>();
                result = await currencyService.ConvertFromPrimaryStoreCurrency(result, workContext.WorkingCurrency);
            }
            return result;
        }

        public async Task<IList<string>> ValidatePaymentForm(IDictionary<string, string> model)
        {
            var warnings = new List<string>();
            return await Task.FromResult(warnings);
        }

        public async Task<PaymentTransaction> SavePaymentInfo(IDictionary<string, string> model)
        {
            return await Task.FromResult<PaymentTransaction>(null);
        }

        public Task<string> GetControllerRouteName()
        {
            return Task.FromResult("Plugin.PaymentBankTransfer");
        }

        public async Task<CapturePaymentResult> Capture(PaymentTransaction paymentTransaction)
        {
            var result = new CapturePaymentResult();
            result.AddError("Capture method not supported");
            return await Task.FromResult(result);
        }

        public async Task<bool> CanRePostRedirectPayment(PaymentTransaction paymentTransaction)
        {
            if (paymentTransaction == null)
                throw new ArgumentNullException(nameof(paymentTransaction));

            return await Task.FromResult(true);
        }

        public async Task<RefundPaymentResult> Refund(RefundPaymentRequest refundPaymentRequest)
        {
            var result = new RefundPaymentResult();
            result.AddError("Refund method not supported");
            return await Task.FromResult(result);
        }

        public async Task<VoidPaymentResult> Void(PaymentTransaction paymentTransaction)
        {
            var result = new VoidPaymentResult();
            result.AddError("Void method not supported");
            return await Task.FromResult(result);
        }
        
        public async Task CancelPayment(PaymentTransaction paymentTransaction)
        {
            var paymentTransactionService = _serviceProvider.GetRequiredService<IPaymentTransactionService>();
            paymentTransaction.TransactionStatus = TransactionStatus.Canceled;
            await paymentTransactionService.UpdatePaymentTransaction(paymentTransaction);
        }

        public async Task<bool> SupportCapture()
        {
            return await Task.FromResult(false);
        }

        public async Task<bool> SupportPartiallyRefund()
        {
            return await Task.FromResult(false);
        }

        public async Task<bool> SupportRefund()
        {
            return await Task.FromResult(false);
        }

        public async Task<bool> SupportVoid()
        {
            return await Task.FromResult(false);
        }

        public PaymentMethodType PaymentMethodType => PaymentMethodType.Redirection;

        public async Task<bool> SkipPaymentInfo()
        {
            return await Task.FromResult(_bankTransferPaymentSettings.SkipPaymentInfo);
        }

        public async Task<string> Description()
        {
            return await Task.FromResult(_translationService.GetResource("Plugins.Payment.BankTransfer.PaymentMethodDescription"));
        }

        public string LogoURL => "/Plugins/Payments.BankTransfer/logo.jpg";

    }
}

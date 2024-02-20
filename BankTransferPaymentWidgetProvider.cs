using Grand.Business.Core.Interfaces.Cms;
using Grand.Business.Core.Interfaces.Common.Localization;

namespace Payments.BankTransfer
{
    public class BankTransferPaymentWidgetProvider : IWidgetProvider
    {
        private readonly ITranslationService _translationService;
        private readonly BankTransferPaymentSettings _bankTransferPaymentSettings;

        public BankTransferPaymentWidgetProvider(
            ITranslationService translationService, 
            BankTransferPaymentSettings bankTransferPaymentSettings)
        {
            _translationService = translationService;
            _bankTransferPaymentSettings = bankTransferPaymentSettings;
        }

        public string ConfigurationUrl => BankTransferPaymentDefaults.ConfigurationUrl;

        public string SystemName => BankTransferPaymentDefaults.ProviderSystemName;

        public string FriendlyName => _translationService.GetResource(BankTransferPaymentDefaults.FriendlyName);

        public int Priority => _bankTransferPaymentSettings.DisplayOrder;

        public IList<string> LimitedToStores => new List<string>();

        public IList<string> LimitedToGroups => new List<string>();

        public Task<string> GetPublicViewComponentName(string widgetZone)
        {
            return Task.FromResult("BankTransfer");
        }

        public async Task<IList<string>> GetWidgetZones()
        {
            return await Task.FromResult(new string[] { 
                "checkout_completed_bottom",
                "checkout_payment_info_top"
            });
        }
    }
}

using Grand.Web.Common.Models;
using Grand.Infrastructure.ModelBinding;
using Grand.Infrastructure.Models;

namespace Payments.BankTransfer.Models
{
    public class ConfigurationModel : BaseModel, ILocalizedModel<ConfigurationModel.ConfigurationLocalizedModel>
    {
        public ConfigurationModel()
        {
            Locales = new List<ConfigurationLocalizedModel>();
        }

        public string ActiveStore { get; set; }

        [GrandResourceDisplayName("Plugins.Payment.BankTransfer.IBAN")]
        public string IBAN { get; set; }

        [GrandResourceDisplayName("Plugins.Payment.BankTransfer.SWIFT")]
        public string SWIFT { get; set; }

        [GrandResourceDisplayName("Plugins.Payment.BankTransfer.VariableSymbolPattern")]
        public string VariableSymbolPattern { get; set; }

        [GrandResourceDisplayName("Plugins.Payment.BankTransfer.QrCodeStringPattern")]
        public string QrCodeStringPattern { get; set; }

        [GrandResourceDisplayName("Plugins.Payment.BankTransfer.DescriptionText")]
        public string DescriptionText { get; set; }

        [GrandResourceDisplayName("Plugins.Payment.BankTransfer.AdditionalFee")]
        public double AdditionalFee { get; set; }

        [GrandResourceDisplayName("Plugins.Payment.BankTransfer.AdditionalFeePercentage")]
        public bool AdditionalFeePercentage { get; set; }

        [GrandResourceDisplayName("Plugins.Payment.BankTransfer.ShippableProductRequired")]
        public bool ShippableProductRequired { get; set; }

        [GrandResourceDisplayName("Plugins.Payment.BankTransfer.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [GrandResourceDisplayName("Plugins.Payment.BankTransfer.SkipPaymentInfo")]
        public bool SkipPaymentInfo { get; set; }
        
        public IList<ConfigurationLocalizedModel> Locales { get; set; }

        #region Nested class

        public partial class ConfigurationLocalizedModel : ILocalizedModelLocal
        {
            public string LanguageId { get; set; }

            [GrandResourceDisplayName("Plugins.Payment.BankTransfer.DescriptionText")]
            public string DescriptionText { get; set; }
        }

        #endregion
    }
}
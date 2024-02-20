using Grand.Domain.Configuration;

namespace Payments.BankTransfer
{
    public class BankTransferPaymentSettings : ISettings
    {
        public int DisplayOrder { get; set; }

        public string IBAN { get; set; }

        public string SWIFT { get; set; }

        public string VariableSymbolPattern { get; set; }

        public string QrCodeStringPattern { get; set; }

        public string DescriptionText { get; set; }

        public bool AdditionalFeePercentage { get; set; }
      
        public double AdditionalFee { get; set; }
  
        public bool ShippableProductRequired { get; set; }

        public bool SkipPaymentInfo { get; set; }
    }
}

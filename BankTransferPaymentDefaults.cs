namespace Payments.BankTransfer
{
    public static class BankTransferPaymentDefaults
    {
        public const string ProviderSystemName = "Payments.BankTransfer";
        public const string FriendlyName = "Payments.BankTransfer.FriendlyName";
        public const string ConfigurationUrl = "/Admin/PaymentBankTransfer/Configure";
        public const string PaymentInfoUrl = "/Plugins/PaymentBankTransfer/PaymentInfo";
        public const string PaymentInstructionsUrl = "/Plugins/PaymentBankTransfer/PaymentInstructions";
        public const string PaymentCodeUrl = "/Plugins/PaymentBankTransfer/PaymentCode";
        public const string PaymentCodeByNumberUrl = "/Plugins/PaymentBankTransfer/PaymentCodeByNumber";
        public const string EmailPaymentQrCode = "Payments.BankTransfer.QrCode";
    }
}

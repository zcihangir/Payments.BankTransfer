using Grand.Domain.Orders;
using Grand.Infrastructure.Models;

namespace Payments.BankTransfer.Models
{
    public class PaymentInstructionsModel : BaseModel
    {
        public string DescriptionText { get; set; }

        public int OrderNumber { get; set; }

        public string OrderId { get; set; }

        public string QrCodePaymentString { get; set; }

        public string AccountNumber { get; set; }

        public string IBAN { get; set; }

        public string SWIFT { get; set; }

        public string VariableSymbol { get; set; }

        public double TotalAmount { get; set; }

        public string CurrencyCode { get; set; }

        public Order Order { get; set; }
    }
}
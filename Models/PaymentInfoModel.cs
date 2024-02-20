using Grand.Infrastructure.Models;

namespace Payments.BankTransfer.Models
{
    public class PaymentInfoModel : BaseModel
    {
        public string DescriptionText { get; set; }
    }
}
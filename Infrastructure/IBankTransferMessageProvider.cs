using Grand.Domain.Customers;
using Grand.Domain.Orders;

namespace Payments.BankTransfer.Infrastructure
{
    public interface IBankTransferMessageProvider
    {
        Task<int> SendQrPaymentMessage(Order order, Customer customer, string languageId);
    }
}

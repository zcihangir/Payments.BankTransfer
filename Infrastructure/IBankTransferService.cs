using Grand.Domain.Orders;

namespace Payments.BankTransfer.Infrastructure
{
    public interface IBankTransferService
    {
        Task<Order> SetNextAvailableNumberForOrder(Order order);

        Task<string> GetQrCodeString(Order order);

        Task<string> GetQrCodeStringByOrderId(string orderId);

        Task SendPaymentNotificationWithQRCode(string orderId);

        Task SendPaymentNotificationWithQRCode(Order order);

        Task<byte[]> GetQrCodeBytesAsPng(string orderId, int pixelsPerSegment = 20);

        Task<byte[]> GetQrCodeBytesAsPng(Order order, int pixelsPerSegment = 20);
    }
}

using Grand.Infrastructure.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Payments.BankTransfer
{
    public partial class EndpointProvider : IEndpointProvider
    {
        public void RegisterEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
        {
            endpointRouteBuilder.MapControllerRoute("Plugin.PaymentBankTransfer",
                 BankTransferPaymentDefaults.PaymentInstructionsUrl,
                 new { controller = "PaymentBankTransfer", action = "PaymentInfo", area = "" }
            );

            endpointRouteBuilder.MapControllerRoute("Plugin.PaymentBankTransferInstructions",
                 $"{BankTransferPaymentDefaults.PaymentInstructionsUrl}/{{orderId}}",
                 new { controller = "PaymentBankTransfer", action = "PaymentInstructions", area = "" }
            );

            endpointRouteBuilder.MapControllerRoute("Plugin.PaymentBankTransferCodeFile",
                 $"{BankTransferPaymentDefaults.PaymentCodeUrl}/{{orderId}}.png",
                 new { controller = "PaymentBankTransfer", action = "PaymentCode", area = "" }
            );

            endpointRouteBuilder.MapControllerRoute("Plugin.PaymentBankTransferCodeByNumberFile",
                 $"{BankTransferPaymentDefaults.PaymentCodeByNumberUrl}/{{orderNumber}}.png",
                 new { controller = "PaymentBankTransfer", action = "PaymentCodeByNumber", area = "" }
            );

            endpointRouteBuilder.MapControllerRoute("Plugin.PaymentBankTransferCode",
                 $"{BankTransferPaymentDefaults.PaymentCodeUrl}/{{orderId}}",
                 new { controller = "PaymentBankTransfer", action = "PaymentCode", area = "" }
            );

            endpointRouteBuilder.MapControllerRoute("Plugin.PaymentBankTransferCodeByNumber",
                 $"{BankTransferPaymentDefaults.PaymentCodeByNumberUrl}/{{orderNumber}}",
                 new { controller = "PaymentBankTransfer", action = "PaymentCodeByNumber", area = "" }
            );
        }

        public int Priority => 0;

    }
}

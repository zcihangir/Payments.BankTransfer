using Microsoft.AspNetCore.Mvc;

namespace Payments.BankTransfer.Components
{
    [ViewComponent(Name = "BankTransfer")]
    public class BankTransferComponent : ViewComponent
    {
        public IViewComponentResult Invoke(string widgetZone, object additionalData = null)
        {
            return View();
        }
    }
}

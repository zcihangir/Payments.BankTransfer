using Grand.Business.Core.Interfaces.Checkout.Orders;
using Grand.Business.Core.Interfaces.Common.Configuration;
using Grand.Business.Core.Interfaces.Common.Directory;
using Grand.Business.Core.Interfaces.Common.Stores;
using Grand.Domain.Orders;
using Grand.Infrastructure;
using Grand.Web.Common.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payments.BankTransfer.Domain;
using Payments.BankTransfer.Infrastructure;
using Payments.BankTransfer.Models;

namespace Payments.BankTransfer.Controllers
{
    public class PaymentBankTransferController : BasePaymentController
    {
        private readonly IWorkContext _workContext;
        private readonly IBankTransferService _bankTransferService;
        private readonly IOrderService _orderService;
        private readonly IUserFieldService _userFieldService;
        private readonly IStoreService _storeService;
        private readonly ISettingService _settingService;

        public PaymentBankTransferController(
            IWorkContext workContext,
            IBankTransferService bankTransferService,
            IOrderService orderService,
            IUserFieldService userFieldService,
            IStoreService storeService,
            ISettingService settingService)
        {
            _workContext = workContext;
            _bankTransferService = bankTransferService;
            _orderService = orderService;
            _userFieldService = userFieldService;
            _storeService = storeService;
            _settingService = settingService;
        }

        public IActionResult PaymentInfo()
        {
            var bankTransferPaymentSettings = _settingService.LoadSetting<BankTransferPaymentSettings>(_workContext.CurrentStore.Id);

            var model = new PaymentInfoModel {
                DescriptionText = bankTransferPaymentSettings.DescriptionText
            };

            return View(model);
        }

        public async Task<ActionResult> PaymentInstructions(string orderId)
        {
            var bankTransferPaymentSettings = _settingService.LoadSetting<BankTransferPaymentSettings>(_workContext.CurrentStore.Id);

            var order = await _orderService.GetOrderById(orderId);
            var store = await _storeService.GetStoreById(order.StoreId);

            var model = new PaymentInstructionsModel {
                DescriptionText = bankTransferPaymentSettings.DescriptionText,
                QrCodePaymentString = await _bankTransferService.GetQrCodeStringByOrderId(orderId),
                Order = order,
                IBAN = bankTransferPaymentSettings.IBAN,
                SWIFT = bankTransferPaymentSettings.SWIFT,
                OrderNumber = order.OrderNumber,
                OrderId = orderId,
                TotalAmount = order.OrderTotal,
                CurrencyCode = order.CustomerCurrencyCode,
                AccountNumber = $"{store.BankAccount.AccountNumber}/{store.BankAccount.BankCode}"
            };

            model.VariableSymbol = await _userFieldService.GetFieldsForEntity<string>(order, InvoiceConstants.INVOICE_VARIABLE_SYMBOL_FIELD_KEY);

            return View(model);
        }

        [AllowAnonymous]
        public async Task<ActionResult> PaymentCode(string orderId, int size = 20)
        {
            byte[] qrCodeBytes = await _bankTransferService.GetQrCodeBytesAsPng(orderId, pixelsPerSegment: size);

            return File(qrCodeBytes, "image/x-png");
        }

        [AllowAnonymous]
        public async Task<ActionResult> PaymentCodeByNumber(int? orderNumber, int size = 20)
        {
            Order order = await _orderService.GetOrderByNumber(orderNumber.Value);

            byte[] qrCodeBytes = await _bankTransferService.GetQrCodeBytesAsPng(order, pixelsPerSegment: size);

            return File(qrCodeBytes, "image/x-png");
        }
    }
}
using BookShop.Models;
using BookShop.Utils;
using BookShop.Utils.ConfigOptions;
using Microsoft.Extensions.Options;

namespace BookShop.Services
{
    public interface IVnPayService
    {
        string CreatePaymentUrl(HttpContext context, VnPaymentRequestModel model);
        VnPaymentResponseModel PaymentExecute(IQueryCollection collections);
    }
    public class VnPayService : IVnPayService
    {
        private readonly VnPayConfigOptions _options;

        public VnPayService(IOptions<VnPayConfigOptions> options)
        {
            _options = options.Value;
        }

        public string CreatePaymentUrl(HttpContext context, VnPaymentRequestModel model)
        {
            var tick = DateTime.Now.Ticks.ToString();

            var vnPay = new VnPayLibrary();
            vnPay.AddRequestData("vnp_Version", _options.Version!);
            vnPay.AddRequestData("vnp_Command", _options.Command!);
            vnPay.AddRequestData("vnp_TmnCode", _options.TmnCode!);
            vnPay.AddRequestData("vnp_Amount", (model.amount * 100).ToString());

            vnPay.AddRequestData("vnp_CreateDate", model.CreatedDate.ToString("yyyyMMddHHmmss"));
            vnPay.AddRequestData("vnp_CurrCode", _options.CurrCode!);
            vnPay.AddRequestData("vnp_IpAddr", UtilityHelper.GetIpAddress(context));
            vnPay.AddRequestData("vnp_Locale", _options.Locale!);

            vnPay.AddRequestData("vnp_OrderInfo", "Thanh toán cho đơn hàng: " + model.OrderId);
            vnPay.AddRequestData("vnp_OrderType", "other");
            vnPay.AddRequestData("vnp_ReturnUrl", _options.PaymentBackReturnUrl!);

            vnPay.AddRequestData("vnp_TxnRef", tick);

            var paymentUrl = vnPay.CreateRequestUrl(_options.BaseUrl!, _options.HashSecret!);

            return paymentUrl;
        }

        public VnPaymentResponseModel PaymentExecute(IQueryCollection collections)
        {
            var vnPay = new VnPayLibrary();
            foreach (var (key, value) in collections)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                {
                    vnPay.AddResponseData(key, value.ToString());
                }
            }

            var vnp_orderId = Convert.ToInt64(vnPay.GetResponseData("vnp_TxnRef"));
            var vnp_TransactionId = Convert.ToInt64(vnPay.GetResponseData("vnp_TransactionNo"));
            var vnp_SecureHash = collections.FirstOrDefault(p => p.Key == "vnp_SecureHash").Value;
            var vnp_ResponseCode = vnPay.GetResponseData("vnp_ResponseCode");
            var vnp_OrderInfo = vnPay.GetResponseData("vnp_OrderInfo");

            bool checkSignature = vnPay.ValidateSignature(vnp_SecureHash!, _options.HashSecret!);
            if (!checkSignature)
            {
                return new VnPaymentResponseModel
                {
                    Success = false
                };
            }

            return new VnPaymentResponseModel
            {
                Success = true,
                PaymentMethod = "VnPay",
                OrderDescription = vnp_OrderInfo,
                OrderId = vnp_orderId.ToString(),
                TransactionId = vnp_TransactionId.ToString(),
                Token = vnp_SecureHash,
                VnPayResponseCode = vnp_ResponseCode
            };
        }
    }
}
using BookShop.Models;
using BookShop.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BookShop.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookShop.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ICartRepository _cartRepo;
        private readonly IVnPayService _vnPayService;

        public CartController(ICartRepository cartRepo, IVnPayService vnPayService)
        {
            _cartRepo = cartRepo;
            _vnPayService = vnPayService;
        }

        public async Task<IActionResult> AddItem(int bookId, int qty = 1, int redirect = 0)
        {
            var cartCount = await _cartRepo.AddItem(bookId, qty);
            if (redirect == 0)
                return Ok(cartCount);
            return RedirectToAction("GetUserCart");
        }

        public async Task<IActionResult> RemoveItem(int bookId)
        {
            var cartCount = await _cartRepo.RemoveItem(bookId);
            return RedirectToAction("GetUserCart");
        }

        public async Task<IActionResult> GetUserCart()
        {
            var cart = await _cartRepo.GetUserCart();
            return View(cart);
        }

        public async Task<IActionResult> GetTotalItemInCart()
        {
            int cartItem = await _cartRepo.GetCartItemCount();
            return Ok(cartItem);
        }

        public IActionResult Checkout()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Checkout(CheckoutModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Handle payment processing  
            if (model.PaymentMethod == "VNPay")
            {
                string cleanedamount = model.amount!.Replace(".", ""); // Assuming amount is part of CheckoutModel  
                var vnPayModel = new VnPaymentRequestModel
                {
                    amount = Double.Parse(cleanedamount),
                    CreatedDate = DateTime.Now,
                    Description = $"{model.Name} {model.MobileNumber}",
                    FullName = model.Name,
                    OrderId = new Random().Next(1000, 100000) // Generate a random order ID  
                };
                return Redirect(_vnPayService.CreatePaymentUrl(HttpContext, vnPayModel));
            }

            // Process checkout without payment  
            bool isCheckedOut = await _cartRepo.DoCheckout(model);
            if (!isCheckedOut)
                return RedirectToAction(nameof(OrderFailure));
            return RedirectToAction(nameof(OrderSuccess));
        }

        public IActionResult PaymentSuccess()
        {
            return View();
        }

        public IActionResult PaymentFail()
        {
            return View();
        }
        public IActionResult PaymentCallBack()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);
            if (response.VnPayResponseCode == "00")
            {
                // Processed successfully  
                return RedirectToAction(nameof(PaymentSuccess));
            }

            // Get the message corresponding to VnPayResponseCode from the dictionary  
            if (vnp_TransactionStatus.TryGetValue(response.VnPayResponseCode!, out var message))
            {
                TempData["Message"] = $"Payment error: {message}";
            }
            else
            {
                TempData["Message"] = $"Unknown payment error: {response.VnPayResponseCode}";
            }

            return RedirectToAction(nameof(PaymentFail));
        }

        public IActionResult OrderSuccess()
        {
            return View();
        }

        public IActionResult OrderFailure()
        {
            return View();
        }

        // Transaction status dictionary  
        public static Dictionary<string, string> vnp_TransactionStatus = new Dictionary<string, string>()
        {
            {"00","Giao dịch thành công" },
            {"01","Giao dịch chưa hoàn tất" },
            {"02","Giao dịch bị lỗi" },
            {"04","Giao dịch đảo (Khách hàng đã bị trừ tiền tại Ngân hàng nhưng GD chưa thành công ở VNPAY)" },
            {"05","VNPAY đang xử lý giao dịch này (GD hoàn tiền)" },
            {"06","VNPAY đã gửi yêu cầu hoàn tiền sang Ngân hàng (GD hoàn tiền)" },
            {"07","Giao dịch bị nghi ngờ gian lận" },
            {"09","GD Hoàn trả bị từ chối" }
        };
    }
}
using System.Linq; // Đảm bảo thêm import này nếu chưa có  
using BookShop.Models.DTOs;
using System.Collections.Generic;

namespace BookShop.Models.DTOs
{
    public class CheckoutViewModel
    {
        public CheckoutModel CheckoutDetails { get; set; }
        public ShoppingCart CartDetails { get; set; }

        public decimal TotalAmount => CartDetails?.CartDetails
            .Sum(item => (decimal)item.Book.Price * item.Quantity) ?? 0;
    }
}
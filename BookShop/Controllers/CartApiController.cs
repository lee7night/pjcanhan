using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
namespace BookShop.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]  // Yêu cầu xác thực người dùng  
    public class CartApiController : ControllerBase
    {
        private readonly ICartRepository _cartRepo;

        public CartApiController(ICartRepository cartRepo)
        {
            _cartRepo = cartRepo;
        }

        [HttpPost("add-item")]
        public async Task<IActionResult> AddItem([FromQuery] int bookId, [FromQuery] int qty = 1)
        {
            var cartCount = await _cartRepo.AddItem(bookId, qty);
            return Ok(cartCount); // Trả về số lượng mặt hàng trong giỏ  
        }

        [HttpDelete("remove-item/{bookId}")]
        public async Task<IActionResult> RemoveItem(int bookId)
        {
            await _cartRepo.RemoveItem(bookId);
            return NoContent(); // Trả về HTTP 204 No Content  
        }

        [HttpGet("user-cart")]
        public async Task<IActionResult> GetUserCart()
        {
            var cart = await _cartRepo.GetUserCart();
            return Ok(cart); // Trả về thông tin giỏ hàng  
        }

        [HttpGet("cart-item-count")]
        public async Task<IActionResult> GetTotalItemInCart()
        {
            int cartItem = await _cartRepo.GetCartItemCount();
            return Ok(cartItem); // Trả về số lượng mặt hàng trong giỏ  
        }

        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout([FromBody] CheckoutModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState); // Trả về HTTP 400 Bad Request  

            bool isCheckedOut = await _cartRepo.DoCheckout(model);
            if (!isCheckedOut)
                return StatusCode(StatusCodes.Status500InternalServerError, "Checkout failed."); // Trả về HTTP 500 nếu thất bại  

            return Ok(new { Success = true }); // Trả về kết quả thành công  
        }
    }
}
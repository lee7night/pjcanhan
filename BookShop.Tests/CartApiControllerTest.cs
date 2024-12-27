using Moq;  
using Xunit;  
using Microsoft.AspNetCore.Mvc;  
using BookShop.Controllers;  
using BookShop.Repositories;
using System.Threading.Tasks;  
using BookShop.Models; 
using Microsoft.AspNetCore.Http;
using System.Collections.Generic; 

namespace BookShop.Tests  
{  
    public class CartApiControllerTests  
    {  
        private readonly Mock<ICartRepository> _mockCartRepo;  
        private readonly CartApiController _controller;  

        public CartApiControllerTests()  
        {  
            _mockCartRepo = new Mock<ICartRepository>();  
            _controller = new CartApiController(_mockCartRepo.Object);  
        }  

        [Fact]  
        public async Task AddItem_ShouldReturnOkResult_WhenItemAdded()  
        {  
            // Arrange  
            int bookId = 1;  
            int qty = 1;  
            _mockCartRepo.Setup(repo => repo.AddItem(bookId, qty)).ReturnsAsync(5);  

            // Act  
            var result = await _controller.AddItem(bookId, qty);  

            // Assert  
            var okResult = Assert.IsType<OkObjectResult>(result);  
            Assert.Equal(5, okResult.Value);  
        }  

        [Fact]  
        public async Task RemoveItem_ShouldReturnNoContent_WhenItemRemoved()  
        {  
            // Arrange  
            int bookId = 1;  

            // Act  
            var result = await _controller.RemoveItem(bookId);  

            // Assert  
            Assert.IsType<NoContentResult>(result);  
        }  

        [Fact]  
    public async Task GetUserCart_ShouldReturnOkResult_WhenCartIsAvailable()  
    {  
        // Arrange  
        var cart = new ShoppingCart(); // Tạo đối tượng ShoppingCart mẫu  
        _mockCartRepo.Setup(repo => repo.GetUserCart()).ReturnsAsync(cart);  

        // Act  
        var result = await _controller.GetUserCart();  

        // Assert  
        var okResult = Assert.IsType<OkObjectResult>(result);  
        Assert.IsType<ShoppingCart>(okResult.Value);  
    }  



        [Fact]  
        public async Task Checkout_ShouldReturnBadRequest_WhenModelStateIsInvalid()  
        {  
            // Arrange  
            _controller.ModelState.AddModelError("key", "error message");  

            // Act  
            var result = await _controller.Checkout(new CheckoutModel());  

            // Assert  
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);  
            Assert.IsType<SerializableError>(badRequestResult.Value);  
        }  

        [Fact]  
        public async Task Checkout_ShouldReturnOkResult_WhenCheckoutIsSuccessful()  
        {  
            // Arrange  
            var model = new CheckoutModel(); // Khai báo CheckoutModel hợp lệ  
            _mockCartRepo.Setup(repo => repo.DoCheckout(model)).ReturnsAsync(true);  

            // Act  
            var result = await _controller.Checkout(model);  

            // Assert  
            var okResult = Assert.IsType<OkObjectResult>(result);  
            Assert.True((bool)okResult.Value.GetType().GetProperty("Success").GetValue(okResult.Value));  
        }  

        [Fact]  
        public async Task Checkout_ShouldReturnInternalServerError_WhenCheckoutFails()  
        {  
            // Arrange  
            var model = new CheckoutModel(); // Khai báo CheckoutModel hợp lệ  
            _mockCartRepo.Setup(repo => repo.DoCheckout(model)).ReturnsAsync(false);  

            // Act  
            var result = await _controller.Checkout(model);  

            // Assert  
            var statusCodeResult = Assert.IsType<ObjectResult>(result);  
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);  
            Assert.Equal("Checkout failed.", statusCodeResult.Value);  
        }  
    }  
}
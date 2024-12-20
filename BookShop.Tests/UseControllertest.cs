using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Threading.Tasks;
using Xunit;
using BookShoppingCartMvcUI.Controllers;
using BookShoppingCartMvcUI.Repositories; // Assuming IUserOrderRepository is in this namespace

namespace BookShoppingCartMvcUI.Tests
{
    public class UserOrderControllerTests
    {
        private readonly Mock<IUserOrderRepository> _mockRepo;
        private readonly UserOrderController _controller;

        public UserOrderControllerTests()
        {
            // Create mock repository
            _mockRepo = new Mock<IUserOrderRepository>();

            // Instantiate the controller with the mocked repository
            _controller = new UserOrderController(_mockRepo.Object);
        }

        [Fact]
        public async Task UserOrders_Returns_ViewResult_With_Orders()
        {
            // Arrange: Mock the repository method to return a sample order list
            var mockOrders = new List<Order>  // Assuming Order is your model type
            {
                new Order { Id = 1, CreateDate = DateTime.Now },
                new Order { Id = 2, CreateDate = DateTime.Now.AddDays(-1) }
            };

            _mockRepo.Setup(repo => repo.UserOrders(It.IsAny<bool>())).ReturnsAsync(mockOrders);
            // Act: Call the UserOrders action method
            var result = await _controller.UserOrders();

            // Assert: Verify that the result is a ViewResult and contains the expected orders
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<Order>>(viewResult.Model);
            Assert.Equal(2, model.Count);
        }
    }
}

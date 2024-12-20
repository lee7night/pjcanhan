using BookShoppingCartMvcUI.Controllers;
using BookShoppingCartMvcUI.Models;
using BookShoppingCartMvcUI.Repositories;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Xunit;
using Microsoft.AspNetCore.Http;

namespace BookShoppingCartMvcUI.Tests
{
    public class AdminOperationsControllerTests
    {
        private readonly Mock<IUserOrderRepository> _mockRepo;
        private readonly AdminOperationsController _controller;

        public AdminOperationsControllerTests()
        {
            // Khởi tạo Mock cho IUserOrderRepository
            _mockRepo = new Mock<IUserOrderRepository>();

            // Khởi tạo controller với Mock repository
            _controller = new AdminOperationsController(_mockRepo.Object);

            // Khởi tạo TempData (cần cho việc sử dụng trong các hành động với TempData)
            _controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
        }

        // Kiểm tra phương thức UpdateOrderStatus (POST) khi thành công
        [Fact]
        public async Task UpdateOrderStatus_Post_Should_Update_Status_And_Redirect_With_Success_Message()
        {
            // Arrange: Mô phỏng phương thức ChangeOrderStatus và GetOrderStatuses
            var data = new UpdateOrderStatusModel
            {
                OrderId = 1,
                OrderStatusId = 2
            };
            _mockRepo.Setup(repo => repo.ChangeOrderStatus(data)).Returns(Task.CompletedTask);

            var mockOrderStatuses = new List<OrderStatus>
            {
                new OrderStatus { Id = 1, StatusName = "Pending" },
                new OrderStatus { Id = 2, StatusName = "Shipped" }
            };
            _mockRepo.Setup(repo => repo.GetOrderStatuses()).ReturnsAsync(mockOrderStatuses);

            // Act: Gọi phương thức hành động UpdateOrderStatus (POST)
            var result = await _controller.UpdateOrderStatus(data);

            // Assert: Kiểm tra kết quả trả về là RedirectToActionResult với thông báo thành công
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("UpdateOrderStatus", redirectResult.ActionName);
            Assert.Equal("Updated successfully", _controller.TempData["msg"]);
        }

        // Kiểm tra phương thức UpdateOrderStatus (POST) khi gặp lỗi
        [Fact]
        public async Task UpdateOrderStatus_Post_Should_Redirect_With_Error_Message_When_Fails()
        {
            // Arrange: Mô phỏng phương thức ChangeOrderStatus ném ngoại lệ
            var data = new UpdateOrderStatusModel
            {
                OrderId = 1,
                OrderStatusId = 2
            };
            _mockRepo.Setup(repo => repo.ChangeOrderStatus(data)).ThrowsAsync(new Exception("Error"));

            var mockOrderStatuses = new List<OrderStatus>
            {
                new OrderStatus { Id = 1, StatusName = "Pending" },
                new OrderStatus { Id = 2, StatusName = "Shipped" }
            };
            _mockRepo.Setup(repo => repo.GetOrderStatuses()).ReturnsAsync(mockOrderStatuses);

            // Act: Gọi phương thức hành động UpdateOrderStatus (POST)
            var result = await _controller.UpdateOrderStatus(data);

            // Assert: Kiểm tra kết quả trả về là RedirectToActionResult với thông báo lỗi
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("UpdateOrderStatus", redirectResult.ActionName);
            Assert.Equal("Something went wrong", _controller.TempData["msg"]);
        }
    }
}

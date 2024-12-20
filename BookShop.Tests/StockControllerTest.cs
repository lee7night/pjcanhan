using AutoMapper;
using BookShop.Controllers;
using BookShop.Models;
using BookShop.Repositories;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

public class StockControllerTests
{
    private readonly Mock<IStockRepository> _mockStockRepo;
    private readonly Mock<IMapper> _mockMapper;
    private readonly StockController _controller;

    public StockControllerTests()
    {
        _mockStockRepo = new Mock<IStockRepository>();
        _mockMapper = new Mock<IMapper>();
        _controller = new StockController(_mockStockRepo.Object, _mockMapper.Object);
    }

    [Fact]
        public async Task Index_Should_Return_View_With_Stocks()
        {
            // Arrange: Mô phỏng phương thức GetStocks trả về một danh sách StockDisplayModel
            var mockStocks = new List<StockDisplayModel>
            {
                new StockDisplayModel { BookId = 1, BookName = "Book 1", Quantity = 10 },
                new StockDisplayModel { BookId = 2, BookName = "Book 2", Quantity = 5 }
            };

            _mockStockRepo.Setup(repo => repo.GetStocks(It.IsAny<string>())).ReturnsAsync(mockStocks);

            // Act: Gọi phương thức hành động Index
            var result = await _controller.Index();

            // Assert: Kiểm tra kết quả trả về là ViewResult với danh sách stocks
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<StockDisplayModel>>(viewResult.Model);
            Assert.Equal(mockStocks.Count, model.Count());
        }
    [Fact]
    public async Task ManangeStock_Get_ReturnsViewResult_WithStock()
    {
        // Arrange
        var bookId = 1;
        var existingStock = new Stock { BookId = bookId, Quantity = 10 };
        var stockDTO = new StockDTO { BookId = bookId, Quantity = 10 };
        _mockStockRepo.Setup(repo => repo.GetStockByBookId(bookId)).ReturnsAsync(existingStock);
        _mockMapper.Setup(m => m.Map<StockDTO>(existingStock)).Returns(stockDTO);

        // Act
        var result = await _controller.ManangeStock(bookId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<StockDTO>(viewResult.Model);
        Assert.Equal(bookId, model.BookId);
        Assert.Equal(10, model.Quantity);
    }

    [Fact]
    public async Task ManangeStock_Post_ReturnsViewResult_WhenModelStateIsInvalid()
    {
        // Arrange
        var stockDTO = new StockDTO { BookId = 1, Quantity = 10 };
        _controller.ModelState.AddModelError("Quantity", "Required");

        // Act
        var result = await _controller.ManangeStock(stockDTO);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(stockDTO, viewResult.Model);
    }


}
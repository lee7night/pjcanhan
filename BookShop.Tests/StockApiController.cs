using AutoMapper;  
using Microsoft.AspNetCore.Mvc;  
using Moq;  
using Xunit;  
using System.Collections.Generic;  
using System.Threading.Tasks;  
using BookShop.Controllers;  
using BookShop.Repositories;  
using BookShop.Models;
using BookShop.Models.DTOs;
using Microsoft.AspNetCore.Http;  

// Tạo lớp test cho StockApiController  
public class StockApiControllerTests  
{  
    private readonly Mock<IStockRepository> _mockStockRepo;  
    private readonly IMapper _mapper;  
    private readonly StockApiController _controller;  

    public StockApiControllerTests()  
    {  
        _mockStockRepo = new Mock<IStockRepository>();  
        
        // Cấu hình AutoMapper cho việc ánh xạ giữa Stock và StockDTO  
        var config = new MapperConfiguration(cfg =>  
        {  
            cfg.CreateMap<Stock, StockDTO>();  
        });  
        _mapper = config.CreateMapper();  
        
        _controller = new StockApiController(_mockStockRepo.Object, _mapper);  
    }  

    [Fact]  
    public async Task GetStockByBookId_ShouldReturnOkResult_WhenStockExists()  
    {  
        // Arrange  
        int bookId = 1;  
        var mockStock = new Stock { Id = 1, BookId = bookId, Quantity = 10 };  
        _mockStockRepo.Setup(repo => repo.GetStockByBookId(bookId)).ReturnsAsync(mockStock);  

        // Act  
        var result = await _controller.GetStockByBookId(bookId);  

        // Assert  
        var okResult = Assert.IsType<OkObjectResult>(result);  
        var stockDto = Assert.IsType<StockDTO>(okResult.Value);  
        Assert.Equal(bookId, stockDto.BookId);  
    }  

    [Fact]  
    public async Task GetStockByBookId_ShouldReturnNotFound_WhenStockDoesNotExist()  
    {  
        // Arrange  
        int bookId = 1;  
        _mockStockRepo.Setup(repo => repo.GetStockByBookId(bookId)).ReturnsAsync((Stock)null);  

        // Act  
        var result = await _controller.GetStockByBookId(bookId);  

        // Assert  
        Assert.IsType<NotFoundResult>(result);  
    }  

    [Fact]  
    public async Task ManageStock_ShouldReturnNoContent_WhenStockIsSuccessfull()  
    {  
        // Arrange  
        var stockDto = new StockDTO { BookId = 1, Quantity = 10 };  
        
        // Act  
        var result = await _controller.ManageStock(stockDto);  

        // Assert  
        Assert.IsType<NoContentResult>(result);  
        _mockStockRepo.Verify(repo => repo.ManageStock(It.IsAny<StockDTO>()), Times.Once);  
    }  

    [Fact]  
    public async Task ManageStock_ShouldReturnBadRequest_WhenModelStateIsInvalid()  
    {  
        // Arrange  
        _controller.ModelState.AddModelError("key", "error"); // Giả lập lỗi Validation  

        // Act  
        var result = await _controller.ManageStock(new StockDTO());  

        // Assert  
        Assert.IsType<BadRequestObjectResult>(result);  
    }  

    [Fact]  
    public async Task ManageStock_ShouldReturn500_WhenExceptionOccurs()  
    {  
        // Arrange  
        _mockStockRepo.Setup(repo => repo.ManageStock(It.IsAny<StockDTO>()))  
            .ThrowsAsync(new System.Exception("Some error"));  

        // Act  
        var result = await _controller.ManageStock(new StockDTO { BookId = 1, Quantity = 10 });  

        // Assert  
        var statusCodeResult = Assert.IsType<ObjectResult>(result);  
        Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);  
        Assert.Equal("Something went wrong while managing stock.", statusCodeResult.Value);  
    }  
}
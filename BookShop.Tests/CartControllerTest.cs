using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

public class CartControllerTests
{
    private readonly Mock<ICartRepository> _mockCartRepo;
    private readonly CartController _controller;

    public CartControllerTests()
    {
        _mockCartRepo = new Mock<ICartRepository>();
        _controller = new CartController(_mockCartRepo.Object);
    }

    [Fact]
    public async Task AddItem_ShouldReturnOk_WhenRedirectIsZero()
    {
        // Arrange
        int bookId = 1;
        int qty = 1;
        int redirect = 0;
        int expectedCartCount = 5;

_mockCartRepo.Setup(repo => repo.AddItem(It.IsAny<int>(), It.IsAny<int>()))
             .ReturnsAsync(expectedCartCount);


        // Act
        var result = await _controller.AddItem(bookId, qty, redirect);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(expectedCartCount, okResult.Value);
    }

    [Fact]
    public async Task AddItem_ShouldRedirectToGetUserCart_WhenRedirectIsNotZero()
    {
        // Arrange
        int bookId = 1;
        int qty = 1;
        int redirect = 1;

        _mockCartRepo.Setup(repo => repo.AddItem(bookId, qty))
                     .ReturnsAsync(5);

        // Act
        var result = await _controller.AddItem(bookId, qty, redirect);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("GetUserCart", redirectResult.ActionName);
    }

    [Fact]
    public async Task RemoveItem_ShouldRedirectToGetUserCart()
    {
        // Arrange
        int bookId = 1;
        _mockCartRepo.Setup(repo => repo.RemoveItem(bookId))
                     .ReturnsAsync(3);

        // Act
        var result = await _controller.RemoveItem(bookId);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("GetUserCart", redirectResult.ActionName);
    }

    

[Fact]
public async Task GetTotalItemInCart_ShouldReturnOkWithCartItemCount()
{
    // Arrange
    int expectedCartItemCount = 4;

    // Setup mock repository to return expected count
    _mockCartRepo.Setup(repo => repo.GetCartItemCount(It.IsAny<string>()))
                 .ReturnsAsync(expectedCartItemCount);

    // Act
    var result = await _controller.GetTotalItemInCart();

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result);
    Assert.Equal(expectedCartItemCount, okResult.Value);
}



    [Fact]
    public async Task Checkout_Post_ShouldRedirectToOrderSuccess_WhenCheckoutIsSuccessful()
    {
        // Arrange
        var model = new CheckoutModel();
        _mockCartRepo.Setup(repo => repo.DoCheckout(model))
                     .ReturnsAsync(true);

        // Act
        var result = await _controller.Checkout(model);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("OrderSuccess", redirectResult.ActionName);
    }

    [Fact]
    public async Task Checkout_Post_ShouldReturnView_WhenModelStateIsInvalid()
    {
        // Arrange
        var model = new CheckoutModel();
        _controller.ModelState.AddModelError("Error", "Invalid data");

        // Act
        var result = await _controller.Checkout(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(model, viewResult.Model);
    }
}

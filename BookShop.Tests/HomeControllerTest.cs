using BookShop.Controllers;
using BookShop.Models;
using BookShop.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Http;


namespace BookShop.Tests
{
    public class HomeControllerTests
    {
        private readonly Mock<ILogger<HomeController>> _mockLogger;
        private readonly Mock<IHomeRepository> _mockHomeRepository;
        private readonly HomeController _controller;
        
        public HomeControllerTests()
        {
            _mockLogger = new Mock<ILogger<HomeController>>();
            _mockHomeRepository = new Mock<IHomeRepository>();
            _controller = new HomeController(_mockLogger.Object, _mockHomeRepository.Object);
        }

        // Test for Index action
        [Fact]
        public async Task Index_ReturnsViewResult_WithBooksAndGenres()
        {
            // Arrange
            var mockBooks = new List<Book>
            {
                new Book { BookName = "Book 1", GenreId = 1 },
                new Book { BookName = "Book 2", GenreId = 2 }
            };
            var mockGenres = new List<Genre>
            {
                new Genre { GenreName = "Genre 1", Id = 1 },
                new Genre { GenreName = "Genre 2", Id = 2 }
            };
            _mockHomeRepository.Setup(repo => repo.GetBooks(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(mockBooks);
            _mockHomeRepository.Setup(repo => repo.Genres())
                .ReturnsAsync(mockGenres);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<BookDisplayModel>(viewResult.Model);
            Assert.Equal(2, model.Books.Count());
            Assert.Equal(2, model.Genres.Count());
        }

        // Test for Privacy action
        [Fact]
        public void Privacy_ReturnsViewResult()
        {
            // Act
            var result = _controller.Privacy();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
        }

        // Test for Error action
        [Fact]
        public void Error_ReturnsViewResult_WithErrorViewModel()
        {
            // Arrange
            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(ctx => ctx.TraceIdentifier).Returns("some-trace-id");

            var controllerContext = new ControllerContext()
            {
                HttpContext = mockHttpContext.Object
            };

            var controller = new HomeController(_mockLogger.Object, _mockHomeRepository.Object)
            {
                ControllerContext = controllerContext
            };

            // Act
            var result = controller.Error();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<ErrorViewModel>(viewResult.Model);
            Assert.False(string.IsNullOrEmpty(model.RequestId)); // Ensure RequestId is not null or empty
        }

    }
}

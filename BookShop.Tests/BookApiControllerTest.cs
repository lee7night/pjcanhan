using BookShop.Controllers;
using BookShop.Shared;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;

namespace BookShop.Tests
{
    public class BookApiControllerTests
    {
        private readonly Mock<IBookRepository> _mockBookRepo;
        private readonly Mock<IGenreRepository> _mockGenreRepo;
        private readonly Mock<IFileService> _mockFileService;
        private readonly Mock<IMapper> _mockMapper;
        private readonly BookApiController _controller;

        public BookApiControllerTests()
        {
            _mockBookRepo = new Mock<IBookRepository>();
            _mockGenreRepo = new Mock<IGenreRepository>();
            _mockFileService = new Mock<IFileService>();
            _mockMapper = new Mock<IMapper>();

            _controller = new BookApiController(
                _mockBookRepo.Object,
                _mockGenreRepo.Object,
                _mockFileService.Object,
                _mockMapper.Object);
        }



        [Fact]
        public async Task GetBook_ReturnsNotFound_WhenBookDoesNotExist()
        {
            // Arrange  
            _mockBookRepo.Setup(repo => repo.GetBookById(1)).ReturnsAsync((Book?)null);

            // Act  
            var result = await _controller.GetBook(1);

            // Assert  
            Assert.IsType<NotFoundResult>(result.Result);
        }



        [Fact]
        public async Task AddBook_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange  
            _controller.ModelState.AddModelError("Title", "Title is required");

            // Act  
            var result = await _controller.AddBook(new BookDTO());

            // Assert  
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.IsType<SerializableError>(badRequestResult.Value);
        }

        [Fact]
        public async Task AddBook_ReturnsCreated_WhenBookIsAdded()
        {
            // Arrange  
            var bookDto = new BookDTO { BookName = "Test Book" };
            var book = new Book { Id = 1, BookName = "Test Book" };
            _mockMapper.Setup(mapper => mapper.Map<Book>(bookDto)).Returns(book);
            _mockBookRepo.Setup(repo => repo.AddBook(It.IsAny<Book>())).Returns(Task.CompletedTask);
            _mockBookRepo.Setup(repo => repo.GetBookById(1)).ReturnsAsync(book); // Ensure the added book can be retrieved  

            // Act  
            var result = await _controller.AddBook(bookDto);

            // Assert  
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(_controller.GetBook), createdAtActionResult.ActionName);
        }

        [Fact]
        public async Task DeleteBook_ReturnsNotFound_WhenBookDoesNotExist()
        {
            // Arrange  
            _mockBookRepo.Setup(repo => repo.GetBookById(1)).ReturnsAsync((Book)null);

            // Act  
            var result = await _controller.DeleteBook(1);

            // Assert  
            Assert.IsType<NotFoundResult>(result);
        }

        // Additional Tests could be added here for other scenarios like updating a book etc.  
    }
}
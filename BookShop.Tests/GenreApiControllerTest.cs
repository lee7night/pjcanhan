using AutoMapper;  
using Microsoft.AspNetCore.Mvc;  
using Moq;  
using BookShop.Controllers;  
using BookShop.Repositories; // Thay thế bằng namespace đúng của IGenreRepository  
using BookShop.Models; // Thay thế bằng namespace đúng của GenreDTO và Genre  
using System;  
using System.Collections.Generic;  
using System.Threading.Tasks;  
using Xunit;  

namespace BookShop.Tests  
{  
    public class GenreApiControllerTests  
    {  
        private readonly Mock<IGenreRepository> _mockGenreRepo;  
        private readonly IMapper _mapper;  
        private readonly GenreApiController _controller;  

        public GenreApiControllerTests()  
        {  
            _mockGenreRepo = new Mock<IGenreRepository>();  

            var config = new MapperConfiguration(cfg =>  
            {  
                cfg.CreateMap<Genre, GenreDTO>();  
                cfg.CreateMap<GenreDTO, Genre>();  
            });  

            _mapper = config.CreateMapper();  
            _controller = new GenreApiController(_mockGenreRepo.Object, _mapper);  
        }  

        [Fact]  
        public async Task GetGenres_ShouldReturnOkResult_WhenGenresExist()  
        {  
            // Arrange  
            var genres = new List<Genre> {  
                new Genre { Id = 1, GenreName = "Fiction" },  
                new Genre { Id = 2, GenreName = "Non-Fiction" }  
            };  

            _mockGenreRepo.Setup(repo => repo.GetGenres()).ReturnsAsync(genres);  

            // Act  
            var result = await _controller.GetGenres();  

            // Assert  
            var okResult = Assert.IsType<OkObjectResult>(result);  
            var genreDTOs = Assert.IsAssignableFrom<List<GenreDTO>>(okResult.Value);  
            Assert.Equal(2, genreDTOs.Count);  
        }  

        [Fact]  
        public async Task GetGenreById_ShouldReturnOkResult_WhenGenreExists()  
        {  
            // Arrange  
            int genreId = 1;  
            var genre = new Genre { Id = genreId, GenreName = "Fiction" };  
            _mockGenreRepo.Setup(repo => repo.GetGenreById(genreId)).ReturnsAsync(genre);  

            // Act  
            var result = await _controller.GetGenreById(genreId);  

            // Assert  
            var okResult = Assert.IsType<OkObjectResult>(result);  
            var genreDTO = Assert.IsType<GenreDTO>(okResult.Value);  
            Assert.Equal(genreId, genreDTO.Id);  
        }  

        [Fact]  
        public async Task GetGenreById_ShouldReturnNotFound_WhenGenreDoesNotExist()  
        {  
            // Arrange  
            int genreId = 1;  
            _mockGenreRepo.Setup(repo => repo.GetGenreById(genreId)).ReturnsAsync((Genre)null);  

            // Act  
            var result = await _controller.GetGenreById(genreId);  

            // Assert  
            Assert.IsType<NotFoundResult>(result);  
        }  

       [Fact]  
public async Task AddGenre_ShouldReturnCreatedResult_WhenGenreIsAdded()  
{  
    // Arrange  
    var genreDTO = new GenreDTO { GenreName = "Science Fiction" };  
    var genreToAdd = new Genre { Id = 1, GenreName = "Science Fiction" };  

    // Set up the repository to return the genre as if it is newly added  
    _mockGenreRepo.Setup(repo => repo.AddGenre(It.IsAny<Genre>()))  
                  .Callback<Genre>(genre => genre.Id = 1); // Đặt ID của thể loại vào 1  

    // Act  
    var result = await _controller.AddGenre(genreDTO);  

    // Assert  
    var createdResult = Assert.IsType<CreatedAtActionResult>(result);  
    Assert.Equal(1, createdResult.RouteValues["id"]); // Kiểm tra ID  
}
        [Fact]  
        public async Task AddGenre_ShouldReturnBadRequest_WhenModelIsInvalid()  
        {  
            // Arrange  
            _controller.ModelState.AddModelError("Name", "Required");  

            // Act  
            var result = await _controller.AddGenre(new GenreDTO());  

            // Assert  
            Assert.IsType<BadRequestObjectResult>(result);  
        }  

        [Fact]  
        public async Task UpdateGenre_ShouldReturnNoContent_WhenUpdateIsSuccessful()  
        {  
            // Arrange  
            int genreId = 1;  
            var genreToUpdate = new GenreDTO { GenreName = "Updated Genre" };  
            var existingGenre = new Genre { Id = genreId, GenreName = "Fiction" };  

            _mockGenreRepo.Setup(repo => repo.GetGenreById(genreId)).ReturnsAsync(existingGenre);  
            _mockGenreRepo.Setup(repo => repo.UpdateGenre(existingGenre)).Returns(Task.CompletedTask);  

            // Act  
            var result = await _controller.UpdateGenre(genreId, genreToUpdate);  

            // Assert  
            Assert.IsType<NoContentResult>(result);  
        }  

        [Fact]  
        public async Task UpdateGenre_ShouldReturnNotFound_WhenGenreDoesNotExist()  
        {  
            // Arrange  
            int genreId = 1;  
            var genreToUpdate = new GenreDTO { GenreName = "Updated Genre" };  
            _mockGenreRepo.Setup(repo => repo.GetGenreById(genreId)).ReturnsAsync((Genre)null);  

            // Act  
            var result = await _controller.UpdateGenre(genreId, genreToUpdate);  

            // Assert  
            Assert.IsType<NotFoundResult>(result);  
        }  

        [Fact]  
        public async Task DeleteGenre_ShouldReturnNoContent_WhenDeleteIsSuccessful()  
        {  
            // Arrange  
            int genreId = 1;  
            var existingGenre = new Genre { Id = genreId, GenreName = "Fiction" };  

            _mockGenreRepo.Setup(repo => repo.GetGenreById(genreId)).ReturnsAsync(existingGenre);  
            _mockGenreRepo.Setup(repo => repo.DeleteGenre(existingGenre)).Returns(Task.CompletedTask);  

            // Act  
            var result = await _controller.DeleteGenre(genreId);  

            // Assert  
            Assert.IsType<NoContentResult>(result);  
        }  

        [Fact]  
        public async Task DeleteGenre_ShouldReturnNotFound_WhenGenreDoesNotExist()  
        {  
            // Arrange  
            int genreId = 1;  
            _mockGenreRepo.Setup(repo => repo.GetGenreById(genreId)).ReturnsAsync((Genre)null);  

            // Act  
            var result = await _controller.DeleteGenre(genreId);  

            // Assert  
            Assert.IsType<NotFoundResult>(result);  
        }  
    }  
}
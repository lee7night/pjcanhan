using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

public class GenreControllerTests
{
    private readonly Mock<IGenreRepository> _mockGenreRepo;
    private readonly Mock<IMapper> _mockMapper;
    private readonly GenreController _controller;

    public GenreControllerTests()
    {
        _mockGenreRepo = new Mock<IGenreRepository>();
        _mockMapper = new Mock<IMapper>();
        _controller = new GenreController(_mockGenreRepo.Object, _mockMapper.Object);
    }

    [Fact]
    public async Task Index_ShouldReturnViewWithGenres()
    {
        // Arrange
        var genres = new List<Genre> { new Genre { Id = 1, GenreName = "Fiction" } };
        _mockGenreRepo.Setup(repo => repo.GetGenres()).ReturnsAsync(genres);

        // Act
        var result = await _controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        viewResult.Model.Should().BeEquivalentTo(genres);
    }

    [Fact]
    public void AddGenre_Get_ShouldReturnView()
    {
        // Act
        var result = _controller.AddGenre();

        // Assert
        Assert.IsType<ViewResult>(result);
    }


    [Fact]
    public async Task AddGenre_Post_ShouldReturnViewWithModelOnFailure()
    {
        // Arrange
        _controller.ModelState.AddModelError("Name", "Required");
        var genreDto = new GenreDTO { GenreName = "" };

        // Act
        var result = await _controller.AddGenre(genreDto);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        viewResult.Model.Should().Be(genreDto);
    }

    [Fact]
    public async Task UpdateGenre_Get_ShouldReturnViewWithGenre()
    {
        // Arrange
        var genre = new Genre { Id = 1, GenreName = "Fiction" };
        var genreDto = new GenreDTO { GenreName = "Fiction" };
        _mockGenreRepo.Setup(repo => repo.GetGenreById(1)).ReturnsAsync(genre);
        _mockMapper.Setup(m => m.Map<GenreDTO>(genre)).Returns(genreDto);

        // Act
        var result = await _controller.UpdateGenre(1);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        viewResult.Model.Should().Be(genreDto);
    }

    [Fact]
    public async Task DeleteGenre_ShouldRedirectToIndexOnSuccess()
    {
        // Arrange
        var genre = new Genre { Id = 1, GenreName = "Fiction" };
        _mockGenreRepo.Setup(repo => repo.GetGenreById(1)).ReturnsAsync(genre);
        _mockGenreRepo.Setup(repo => repo.DeleteGenre(genre)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteGenre(1);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(_controller.Index), redirectResult.ActionName);
    }

    [Fact]
    public async Task DeleteGenre_ShouldThrowInvalidOperationExceptionWhenGenreNotFound()
    {
        // Arrange
        _mockGenreRepo.Setup(repo => repo.GetGenreById(1)).ReturnsAsync((Genre)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.DeleteGenre(1));
    }
}

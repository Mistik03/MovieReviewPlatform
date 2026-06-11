using FluentAssertions;
using Moq;
using MovieReview.Api.Domain.Entities;
using MovieReview.Api.Domain.Exceptions;
using MovieReview.Api.Domain.ReadModels;
using MovieReview.Api.DTOs.Genres;
using MovieReview.Api.Repositories.Interfaces;
using MovieReview.Api.Services.Implementations;
using MovieReview.Tests.TestHelpers;

namespace MovieReview.Tests.Services;

public class GenreServiceTests
{
    private readonly Mock<IGenreRepository> _genres = new();
    private readonly GenreService _sut;

    public GenreServiceTests()
    {
        _sut = new GenreService(_genres.Object);
    }

    [Fact]
    public async Task GetAll_MapsTitleCounts()
    {
        _genres.Setup(r => r.GetAllWithCountsAsync(It.IsAny<CancellationToken>()))
               .ReturnsAsync(new List<GenreWithCount> { new(TestData.Genre(), 7) });

        var result = await _sut.GetAllAsync();

        result.Single().TitleCount.Should().Be(7);
        result.Single().Name.Should().Be("Action");
    }

    [Fact]
    public async Task GetById_WhenMissing_ThrowsNotFound()
    {
        var act = () => _sut.GetByIdAsync(99);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Create_WithDuplicateName_ThrowsConflict()
    {
        _genres.Setup(r => r.GetByNameAsync("Action", It.IsAny<CancellationToken>()))
               .ReturnsAsync(TestData.Genre());

        var act = () => _sut.CreateAsync(new GenreCreateDto { Name = "Action" });

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Create_TrimsName_AndReturnsDto()
    {
        _genres.Setup(r => r.AddAsync(It.IsAny<Genre>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync((Genre g, CancellationToken _) => { g.Id = 5; return g; });

        var result = await _sut.CreateAsync(new GenreCreateDto { Name = "  Sci-Fi  " });

        result.Name.Should().Be("Sci-Fi");
        result.Id.Should().Be(5);
        result.TitleCount.Should().Be(0);
    }

    [Fact]
    public async Task Delete_WhenGenreInUse_ThrowsConflict()
    {
        _genres.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(TestData.Genre());
        _genres.Setup(r => r.CountTitlesAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(3);

        var act = () => _sut.DeleteAsync(1);

        await act.Should().ThrowAsync<ConflictException>().WithMessage("*3 title(s)*");
        _genres.Verify(r => r.DeleteAsync(It.IsAny<Genre>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Delete_WhenUnused_Deletes()
    {
        var genre = TestData.Genre();
        _genres.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(genre);
        _genres.Setup(r => r.CountTitlesAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(0);

        await _sut.DeleteAsync(1);

        _genres.Verify(r => r.DeleteAsync(genre, It.IsAny<CancellationToken>()), Times.Once);
    }
}

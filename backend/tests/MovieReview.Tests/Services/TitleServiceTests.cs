using FluentAssertions;
using Moq;
using MovieReview.Api.Domain.Enums;
using MovieReview.Api.Domain.Exceptions;
using MovieReview.Api.Domain.Queries;
using MovieReview.Api.Domain.ReadModels;
using MovieReview.Api.DTOs.Titles;
using MovieReview.Api.Repositories.Interfaces;
using MovieReview.Api.Services.Implementations;
using MovieReview.Tests.TestHelpers;

namespace MovieReview.Tests.Services;

public class TitleServiceTests
{
    private readonly Mock<ITitleRepository> _titles = new();
    private readonly Mock<IGenreRepository> _genres = new();
    private readonly TitleService _sut;

    public TitleServiceTests()
    {
        _sut = new TitleService(_titles.Object, _genres.Object);
    }

    [Fact]
    public async Task GetPaged_WithInvalidSort_ThrowsBadRequest()
    {
        var act = () => _sut.GetPagedAsync(new TitleQueryParams { Sort = "bogus" });

        await act.Should().ThrowAsync<BadRequestException>().WithMessage("*sort*");
    }

    [Fact]
    public async Task GetPaged_WithPageBelowOne_ThrowsBadRequest()
    {
        var act = () => _sut.GetPagedAsync(new TitleQueryParams { Page = 0 });

        await act.Should().ThrowAsync<BadRequestException>().WithMessage("*Page*");
    }

    [Fact]
    public async Task GetPaged_MapsStatsAndComputesTotalPages()
    {
        var rows = new List<TitleWithStats> { new(TestData.Title(), 8.25, 3, 4) };
        _titles.Setup(r => r.GetPagedWithStatsAsync(It.IsAny<TitleQueryParams>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync((rows, 41));

        var result = await _sut.GetPagedAsync(new TitleQueryParams { Page = 1, PageSize = 20 });

        result.TotalCount.Should().Be(41);
        result.TotalPages.Should().Be(3);
        result.Items.Single().AverageRating.Should().Be(8.3); // rounded to one decimal
        result.Items.Single().ReviewCount.Should().Be(3);
    }

    [Fact]
    public async Task GetById_WhenMissing_ThrowsNotFound()
    {
        var act = () => _sut.GetByIdAsync(99);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Create_WithDuplicateNameYearType_ThrowsConflict()
    {
        _titles.Setup(r => r.ExistsByNameYearTypeAsync("Inception", 2010, MediaType.Movie, null, It.IsAny<CancellationToken>()))
               .ReturnsAsync(true);

        var act = () => _sut.CreateAsync(new TitleCreateDto
        {
            Name = "Inception",
            ReleaseYear = 2010,
            MediaType = MediaType.Movie
        });

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Create_WithUnknownGenreId_ThrowsBadRequest()
    {
        var act = () => _sut.CreateAsync(new TitleCreateDto
        {
            Name = "Inception",
            ReleaseYear = 2010,
            MediaType = MediaType.Movie,
            GenreIds = [123]
        });

        await act.Should().ThrowAsync<BadRequestException>().WithMessage("*Genre*123*");
    }

    [Fact]
    public async Task Delete_WhenMissing_ThrowsNotFound()
    {
        var act = () => _sut.DeleteAsync(99);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Delete_WithReviewsOrRatings_ThrowsConflict()
    {
        _titles.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(TestData.Title());
        _titles.Setup(r => r.HasReviewsOrRatingsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var act = () => _sut.DeleteAsync(1);

        await act.Should().ThrowAsync<ConflictException>().WithMessage("*reviews or ratings*");
        _titles.Verify(r => r.DeleteAsync(It.IsAny<MovieReview.Api.Domain.Entities.Title>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Delete_WithoutDependents_DeletesTitle()
    {
        var title = TestData.Title();
        _titles.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(title);
        _titles.Setup(r => r.HasReviewsOrRatingsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        await _sut.DeleteAsync(1);

        _titles.Verify(r => r.DeleteAsync(title, It.IsAny<CancellationToken>()), Times.Once);
    }
}

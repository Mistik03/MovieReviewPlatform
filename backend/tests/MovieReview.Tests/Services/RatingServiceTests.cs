using FluentAssertions;
using Moq;
using MovieReview.Api.Domain.Entities;
using MovieReview.Api.Domain.Exceptions;
using MovieReview.Api.DTOs.Ratings;
using MovieReview.Api.Repositories.Interfaces;
using MovieReview.Api.Services.Implementations;
using MovieReview.Tests.TestHelpers;

namespace MovieReview.Tests.Services;

public class RatingServiceTests
{
    private readonly Mock<IRatingRepository> _ratings = new();
    private readonly Mock<ITitleRepository> _titles = new();
    private readonly Mock<IUserRepository> _users = new();
    private readonly RatingService _sut;

    public RatingServiceTests()
    {
        _sut = new RatingService(_ratings.Object, _titles.Object, _users.Object);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(11)]
    [InlineData(-3)]
    public async Task Create_WithScoreOutsideRange_ThrowsBadRequest(int score)
    {
        var act = () => _sut.CreateAsync(1, new RatingCreateDto { TitleId = 1, Score = score });

        await act.Should().ThrowAsync<BadRequestException>().WithMessage("*between 1 and 10*");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    public async Task Create_WithBoundaryScores_Succeeds(int score)
    {
        _titles.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(TestData.Title());
        _ratings.Setup(r => r.ExistsForUserAndTitleAsync(1, 1, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _users.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(TestData.User());
        _ratings.Setup(r => r.AddAsync(It.IsAny<Rating>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Rating r, CancellationToken _) => r);

        var result = await _sut.CreateAsync(1, new RatingCreateDto { TitleId = 1, Score = score });

        result.Score.Should().Be(score);
    }

    [Fact]
    public async Task Create_WhenTitleMissing_ThrowsNotFound()
    {
        var act = () => _sut.CreateAsync(1, new RatingCreateDto { TitleId = 99, Score = 7 });

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Create_WhenAlreadyRated_ThrowsConflict()
    {
        _titles.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(TestData.Title());
        _ratings.Setup(r => r.ExistsForUserAndTitleAsync(1, 1, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var act = () => _sut.CreateAsync(1, new RatingCreateDto { TitleId = 1, Score = 7 });

        await act.Should().ThrowAsync<ConflictException>().WithMessage("*already rated*");
    }

    [Fact]
    public async Task Update_WithScoreOutsideRange_ThrowsBadRequest()
    {
        var act = () => _sut.UpdateAsync(1, 1, false, new RatingUpdateDto { Score = 12 });

        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Update_ByDifferentUser_ThrowsForbidden()
    {
        _ratings.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.Rating(userId: 1));

        var act = () => _sut.UpdateAsync(1, callerId: 2, callerIsAdmin: false, new RatingUpdateDto { Score = 5 });

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Update_ByOwner_ChangesScoreAndSetsUpdatedAt()
    {
        _ratings.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.Rating(userId: 1, score: 6));

        var result = await _sut.UpdateAsync(1, callerId: 1, callerIsAdmin: false, new RatingUpdateDto { Score = 9 });

        result.Score.Should().Be(9);
        result.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Delete_ByDifferentUser_ThrowsForbidden()
    {
        _ratings.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.Rating(userId: 1));

        var act = () => _sut.DeleteAsync(1, callerId: 2, callerIsAdmin: false);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Delete_ByAdmin_Deletes()
    {
        var rating = TestData.Rating(userId: 1);
        _ratings.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(rating);

        await _sut.DeleteAsync(1, callerId: 99, callerIsAdmin: true);

        _ratings.Verify(r => r.DeleteAsync(rating, It.IsAny<CancellationToken>()), Times.Once);
    }
}

using FluentAssertions;
using Moq;
using MovieReview.Api.Domain.Entities;
using MovieReview.Api.Domain.Exceptions;
using MovieReview.Api.DTOs.Reviews;
using MovieReview.Api.Repositories.Interfaces;
using MovieReview.Api.Services.Implementations;
using MovieReview.Tests.TestHelpers;

namespace MovieReview.Tests.Services;

public class ReviewServiceTests
{
    private readonly Mock<IReviewRepository> _reviews = new();
    private readonly Mock<ITitleRepository> _titles = new();
    private readonly Mock<IUserRepository> _users = new();
    private readonly ReviewService _sut;

    public ReviewServiceTests()
    {
        _sut = new ReviewService(_reviews.Object, _titles.Object, _users.Object);
    }

    private void TitleExists(int id = 1) =>
        _titles.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(TestData.Title(id));

    [Fact]
    public async Task GetByTitle_WhenTitleMissing_ThrowsNotFound()
    {
        var act = () => _sut.GetByTitleAsync(99);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Create_WhenTitleMissing_ThrowsNotFound()
    {
        var act = () => _sut.CreateAsync(1, new ReviewCreateDto { TitleId = 99, Content = "Great movie overall." });

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Create_WhenAlreadyReviewed_ThrowsConflict()
    {
        TitleExists();
        _reviews.Setup(r => r.ExistsForUserAndTitleAsync(1, 1, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var act = () => _sut.CreateAsync(1, new ReviewCreateDto { TitleId = 1, Content = "Great movie overall." });

        await act.Should().ThrowAsync<ConflictException>().WithMessage("*already reviewed*");
    }

    [Fact]
    public async Task Create_TrimsContent_AndMapsAuthor()
    {
        TitleExists();
        _reviews.Setup(r => r.ExistsForUserAndTitleAsync(1, 1, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _users.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(TestData.User());
        _reviews.Setup(r => r.AddAsync(It.IsAny<Review>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Review r, CancellationToken _) => { r.Id = 10; return r; });

        var result = await _sut.CreateAsync(1, new ReviewCreateDto { TitleId = 1, Content = "  Great movie overall.  " });

        result.Content.Should().Be("Great movie overall.");
        result.Username.Should().Be("arb");
        result.TitleName.Should().Be("Inception");
    }

    [Fact]
    public async Task Update_ByDifferentUser_ThrowsForbidden()
    {
        _reviews.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.Review(userId: 1));

        var act = () => _sut.UpdateAsync(1, callerId: 2, callerIsAdmin: false,
            new ReviewUpdateDto { Content = "Edited content here." });

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Update_ByAdmin_IsAllowed()
    {
        _reviews.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.Review(userId: 1));

        var result = await _sut.UpdateAsync(1, callerId: 99, callerIsAdmin: true,
            new ReviewUpdateDto { Content = "Edited by an admin." });

        result.Content.Should().Be("Edited by an admin.");
        result.UpdatedAt.Should().NotBeNull();
        _reviews.Verify(r => r.UpdateAsync(It.IsAny<Review>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_ByOwner_Deletes()
    {
        var review = TestData.Review(userId: 1);
        _reviews.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(review);

        await _sut.DeleteAsync(1, callerId: 1, callerIsAdmin: false);

        _reviews.Verify(r => r.DeleteAsync(review, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_ByDifferentUser_ThrowsForbidden()
    {
        _reviews.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.Review(userId: 1));

        var act = () => _sut.DeleteAsync(1, callerId: 2, callerIsAdmin: false);

        await act.Should().ThrowAsync<ForbiddenException>();
        _reviews.Verify(r => r.DeleteAsync(It.IsAny<Review>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}

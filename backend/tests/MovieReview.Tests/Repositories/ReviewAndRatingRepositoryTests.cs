using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MovieReview.Api.Domain.Entities;
using MovieReview.Api.Domain.Enums;
using MovieReview.Api.Repositories.Implementations;

namespace MovieReview.Tests.Repositories;

public class ReviewAndRatingRepositoryTests : SqliteRepositoryTestBase
{
    private readonly ReviewRepository _reviews;
    private readonly RatingRepository _ratings;
    private readonly int _userId;
    private readonly int _titleId;

    public ReviewAndRatingRepositoryTests()
    {
        _reviews = new ReviewRepository(Context);
        _ratings = new RatingRepository(Context);

        var user = new User { Username = "arb", Email = "arb@example.com", PasswordHash = "h", Role = "User" };
        var title = new Title { Name = "Inception", ReleaseYear = 2010, MediaType = MediaType.Movie };
        Context.AddRange(user, title);
        Context.SaveChanges();
        _userId = user.Id;
        _titleId = title.Id;
    }

    [Fact]
    public async Task AddReview_ThenExists_ReturnsTrue()
    {
        await _reviews.AddAsync(new Review { Content = "A modern classic.", UserId = _userId, TitleId = _titleId });

        (await _reviews.ExistsForUserAndTitleAsync(_userId, _titleId)).Should().BeTrue();
        (await _reviews.ExistsForUserAndTitleAsync(_userId + 1, _titleId)).Should().BeFalse();
    }

    [Fact]
    public async Task DuplicateReviewPerUserAndTitle_ViolatesUniqueIndex()
    {
        await _reviews.AddAsync(new Review { Content = "First review.", UserId = _userId, TitleId = _titleId });

        var act = () => _reviews.AddAsync(new Review { Content = "Second review.", UserId = _userId, TitleId = _titleId });

        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task DuplicateRatingPerUserAndTitle_ViolatesUniqueIndex()
    {
        await _ratings.AddAsync(new Rating { Score = 8, UserId = _userId, TitleId = _titleId });

        var act = () => _ratings.AddAsync(new Rating { Score = 9, UserId = _userId, TitleId = _titleId });

        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task RatingScoreOutsideRange_ViolatesCheckConstraint()
    {
        var act = () => _ratings.AddAsync(new Rating { Score = 11, UserId = _userId, TitleId = _titleId });

        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task GetByTitleId_LoadsAuthorAndOrdersByNewest()
    {
        var otherUser = new User { Username = "mira", Email = "mira@example.com", PasswordHash = "h", Role = "User" };
        Context.Add(otherUser);
        Context.SaveChanges();

        await _reviews.AddAsync(new Review
        {
            Content = "Older review.", UserId = _userId, TitleId = _titleId,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        });
        await _reviews.AddAsync(new Review
        {
            Content = "Newer review.", UserId = otherUser.Id, TitleId = _titleId,
            CreatedAt = DateTime.UtcNow
        });

        var list = await _reviews.GetByTitleIdAsync(_titleId);

        list.Should().HaveCount(2);
        list[0].Content.Should().Be("Newer review.");
        list[0].User.Username.Should().Be("mira");
        list[1].User.Username.Should().Be("arb");
    }

    [Fact]
    public async Task DeleteReview_RemovesRow()
    {
        var review = await _reviews.AddAsync(new Review { Content = "To be removed.", UserId = _userId, TitleId = _titleId });

        await _reviews.DeleteAsync(review);

        (await _reviews.GetByIdAsync(review.Id)).Should().BeNull();
    }
}

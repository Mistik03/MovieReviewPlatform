using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MovieReview.Api.Domain.Entities;
using MovieReview.Api.Domain.Enums;
using MovieReview.Api.Repositories.Implementations;

namespace MovieReview.Tests.Repositories;

public class GenreAndUserRepositoryTests : SqliteRepositoryTestBase
{
    private readonly GenreRepository _genres;
    private readonly UserRepository _users;

    public GenreAndUserRepositoryTests()
    {
        _genres = new GenreRepository(Context);
        _users = new UserRepository(Context);
    }

    [Fact]
    public async Task GetAllWithCounts_CountsAssignedTitles()
    {
        var action = new Genre { Name = "Action" };
        var drama = new Genre { Name = "Drama" };
        Context.AddRange(
            new Title { Name = "A", ReleaseYear = 2000, MediaType = MediaType.Movie, TitleGenres = { new TitleGenre { Genre = action } } },
            new Title { Name = "B", ReleaseYear = 2001, MediaType = MediaType.Movie, TitleGenres = { new TitleGenre { Genre = action } } },
            drama);
        Context.SaveChanges();

        var rows = await _genres.GetAllWithCountsAsync();

        rows.Should().HaveCount(2);
        rows.Single(r => r.Genre.Name == "Action").TitleCount.Should().Be(2);
        rows.Single(r => r.Genre.Name == "Drama").TitleCount.Should().Be(0);
    }

    [Fact]
    public async Task GetByName_IsCaseInsensitive()
    {
        Context.Add(new Genre { Name = "Science Fiction" });
        Context.SaveChanges();

        (await _genres.GetByNameAsync("science fiction")).Should().NotBeNull();
        (await _genres.GetByNameAsync("SCIENCE FICTION")).Should().NotBeNull();
        (await _genres.GetByNameAsync("Horror")).Should().BeNull();
    }

    [Fact]
    public async Task GetByTmdbIds_ReturnsOnlyMatches()
    {
        Context.AddRange(
            new Genre { Name = "Action", TmdbId = 28 },
            new Genre { Name = "Drama", TmdbId = 18 },
            new Genre { Name = "Manual" });
        Context.SaveChanges();

        var found = await _genres.GetByTmdbIdsAsync([28, 999]);

        found.Should().ContainSingle(g => g.Name == "Action");
    }

    [Fact]
    public async Task DuplicateGenreName_ViolatesUniqueIndex()
    {
        await _genres.AddAsync(new Genre { Name = "Action" });

        var act = () => _genres.AddAsync(new Genre { Name = "Action" });

        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task DuplicateUsernameOrEmail_ViolatesUniqueIndexes()
    {
        await _users.AddAsync(new User { Username = "arb", Email = "arb@example.com", PasswordHash = "h", Role = "User" });

        var duplicateUsername = () => _users.AddAsync(new User { Username = "arb", Email = "other@example.com", PasswordHash = "h", Role = "User" });
        var duplicateEmail = () => _users.AddAsync(new User { Username = "other", Email = "arb@example.com", PasswordHash = "h", Role = "User" });

        await duplicateUsername.Should().ThrowAsync<DbUpdateException>();
        Context.ChangeTracker.Clear();
        await duplicateEmail.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task GetProfile_LoadsReviewsAndRatingsWithTitles()
    {
        var user = new User { Username = "arb", Email = "arb@example.com", PasswordHash = "h", Role = "User" };
        var title = new Title { Name = "Inception", ReleaseYear = 2010, MediaType = MediaType.Movie };
        Context.AddRange(user, title);
        Context.SaveChanges();
        Context.AddRange(
            new Review { Content = "Stunning visuals throughout.", UserId = user.Id, TitleId = title.Id },
            new Rating { Score = 9, UserId = user.Id, TitleId = title.Id });
        Context.SaveChanges();

        var profile = await _users.GetProfileAsync(user.Id);

        profile.Should().NotBeNull();
        profile!.Reviews.Single().Title.Name.Should().Be("Inception");
        profile.Ratings.Single().Title.Name.Should().Be("Inception");
    }
}

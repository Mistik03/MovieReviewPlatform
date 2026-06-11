using FluentAssertions;
using MovieReview.Api.Domain.Entities;
using MovieReview.Api.Domain.Enums;
using MovieReview.Api.Domain.Queries;
using MovieReview.Api.Repositories.Implementations;

namespace MovieReview.Tests.Repositories;

public class TitleRepositoryTests : SqliteRepositoryTestBase
{
    private readonly TitleRepository _sut;

    public TitleRepositoryTests()
    {
        _sut = new TitleRepository(Context);
        Seed();
    }

    private void Seed()
    {
        var action = new Genre { Name = "Action" };
        var drama = new Genre { Name = "Drama" };
        var user = new User { Username = "arb", Email = "arb@example.com", PasswordHash = "h", Role = "User" };
        var user2 = new User { Username = "mira", Email = "mira@example.com", PasswordHash = "h", Role = "User" };

        var inception = new Title
        {
            Name = "Inception", ReleaseYear = 2010, MediaType = MediaType.Movie, TmdbId = 27205,
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            TitleGenres = { new TitleGenre { Genre = action } }
        };
        var godfather = new Title
        {
            Name = "The Godfather", ReleaseYear = 1972, MediaType = MediaType.Movie,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            TitleGenres = { new TitleGenre { Genre = drama } }
        };
        var strangerThings = new Title
        {
            Name = "Stranger Things", ReleaseYear = 2016, MediaType = MediaType.TvShow, TmdbId = 66732,
            CreatedAt = DateTime.UtcNow,
            TitleGenres = { new TitleGenre { Genre = drama } }
        };

        inception.Ratings.Add(new Rating { Score = 9, User = user });
        inception.Ratings.Add(new Rating { Score = 8, User = user2 });
        inception.Reviews.Add(new Review { Content = "Brilliant layered storytelling.", User = user });
        godfather.Ratings.Add(new Rating { Score = 10, User = user });

        Context.AddRange(inception, godfather, strangerThings);
        Context.SaveChanges();
    }

    [Fact]
    public async Task GetPaged_ComputesAverageAndCounts()
    {
        var (items, total) = await _sut.GetPagedWithStatsAsync(new TitleQueryParams { Sort = "name" });

        total.Should().Be(3);
        var inception = items.Single(i => i.Title.Name == "Inception");
        inception.AverageRating.Should().Be(8.5);
        inception.RatingCount.Should().Be(2);
        inception.ReviewCount.Should().Be(1);
    }

    [Fact]
    public async Task GetPaged_FiltersByMediaType()
    {
        var (items, total) = await _sut.GetPagedWithStatsAsync(new TitleQueryParams { MediaType = MediaType.TvShow });

        total.Should().Be(1);
        items.Single().Title.Name.Should().Be("Stranger Things");
    }

    [Fact]
    public async Task GetPaged_FiltersByGenre()
    {
        var dramaId = Context.Genres.Single(g => g.Name == "Drama").Id;

        var (items, total) = await _sut.GetPagedWithStatsAsync(new TitleQueryParams { GenreId = dramaId, Sort = "name" });

        total.Should().Be(2);
        items.Select(i => i.Title.Name).Should().BeEquivalentTo("The Godfather", "Stranger Things");
    }

    [Fact]
    public async Task GetPaged_SearchIsCaseInsensitive()
    {
        var (items, total) = await _sut.GetPagedWithStatsAsync(new TitleQueryParams { Search = "godFATHER" });

        total.Should().Be(1);
        items.Single().Title.Name.Should().Be("The Godfather");
    }

    [Fact]
    public async Task GetPaged_SortsByRating()
    {
        var (items, _) = await _sut.GetPagedWithStatsAsync(new TitleQueryParams { Sort = "rating" });

        items[0].Title.Name.Should().Be("The Godfather"); // avg 10
        items[1].Title.Name.Should().Be("Inception");     // avg 8.5
        items[2].Title.Name.Should().Be("Stranger Things"); // unrated
    }

    [Fact]
    public async Task GetPaged_PaginatesCorrectly()
    {
        var (page2, total) = await _sut.GetPagedWithStatsAsync(new TitleQueryParams
        {
            Sort = "name", Page = 2, PageSize = 2
        });

        total.Should().Be(3);
        page2.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetByTmdbId_DistinguishesMediaTypes()
    {
        (await _sut.GetByTmdbIdAsync(27205, MediaType.Movie)).Should().NotBeNull();
        (await _sut.GetByTmdbIdAsync(27205, MediaType.TvShow)).Should().BeNull();
    }

    [Fact]
    public async Task ExistsByNameYearType_RespectsExcludeId()
    {
        var inceptionId = Context.Titles.Single(t => t.Name == "Inception").Id;

        (await _sut.ExistsByNameYearTypeAsync("Inception", 2010, MediaType.Movie)).Should().BeTrue();
        (await _sut.ExistsByNameYearTypeAsync("Inception", 2010, MediaType.Movie, excludeId: inceptionId)).Should().BeFalse();
    }

    [Fact]
    public async Task HasReviewsOrRatings_DetectsDependents()
    {
        var inceptionId = Context.Titles.Single(t => t.Name == "Inception").Id;
        var strangerThingsId = Context.Titles.Single(t => t.Name == "Stranger Things").Id;

        (await _sut.HasReviewsOrRatingsAsync(inceptionId)).Should().BeTrue();
        (await _sut.HasReviewsOrRatingsAsync(strangerThingsId)).Should().BeFalse();
    }

    [Fact]
    public async Task GetByIdWithDetails_LoadsGenresAndOrderedCast()
    {
        var person1 = new Person { Name = "Leonardo DiCaprio" };
        var person2 = new Person { Name = "Joseph Gordon-Levitt" };
        var inception = Context.Titles.Single(t => t.Name == "Inception");
        inception.CastMembers.Add(new CastMember { Person = person2, CharacterName = "Arthur", CastOrder = 1 });
        inception.CastMembers.Add(new CastMember { Person = person1, CharacterName = "Dom Cobb", CastOrder = 0 });
        Context.SaveChanges();
        // Query fresh entities so the ordered include is observable instead of tracked insertion order.
        Context.ChangeTracker.Clear();

        var loaded = await _sut.GetByIdWithDetailsAsync(inception.Id);

        loaded.Should().NotBeNull();
        loaded!.TitleGenres.Single().Genre.Name.Should().Be("Action");
        loaded.CastMembers.First().CharacterName.Should().Be("Dom Cobb");
    }
}

using MovieReview.Api.Domain.Entities;
using MovieReview.Api.Domain.Enums;

namespace MovieReview.Tests.TestHelpers;

/// <summary>Factory methods for entities used across test classes.</summary>
public static class TestData
{
    public static User User(int id = 1, string username = "arb", string role = Roles.User) => new()
    {
        Id = id,
        Username = username,
        Email = $"{username}@example.com",
        PasswordHash = "hash",
        Role = role
    };

    public static Title Title(int id = 1, string name = "Inception", int year = 2010,
        MediaType mediaType = MediaType.Movie, int? tmdbId = null) => new()
    {
        Id = id,
        Name = name,
        ReleaseYear = year,
        MediaType = mediaType,
        TmdbId = tmdbId,
        Description = "A mind-bending thriller.",
        Director = "Christopher Nolan"
    };

    public static Genre Genre(int id = 1, string name = "Action", int? tmdbId = null) => new()
    {
        Id = id,
        Name = name,
        TmdbId = tmdbId
    };

    public static Review Review(int id = 1, int userId = 1, int titleId = 1, string content = "A fantastic movie, watch it.") => new()
    {
        Id = id,
        UserId = userId,
        TitleId = titleId,
        Content = content,
        User = User(userId),
        Title = Title(titleId)
    };

    public static Rating Rating(int id = 1, int userId = 1, int titleId = 1, int score = 8) => new()
    {
        Id = id,
        UserId = userId,
        TitleId = titleId,
        Score = score,
        User = User(userId),
        Title = Title(titleId)
    };
}

using FluentAssertions;
using MovieReview.Api.Domain.Entities;
using MovieReview.Api.Domain.Enums;
using MovieReview.Api.Domain.Exceptions;
using MovieReview.Api.DTOs.Titles;
using MovieReview.Api.Repositories.Implementations;
using MovieReview.Api.Services.Implementations;
using MovieReview.Tests.Repositories;

namespace MovieReview.Tests.Services;

/// <summary>
/// TitleService wired to real repositories over SQLite — catches bugs that
/// repository mocks cannot, like check-vs-store normalization mismatches.
/// </summary>
public class TitleServiceIntegrationTests : SqliteRepositoryTestBase
{
    private TitleService CreateService() =>
        new(new TitleRepository(Context), new GenreRepository(Context));

    [Fact]
    public async Task Create_WithTrailingWhitespaceDuplicate_Throws409NotDbError()
    {
        Context.Add(new Title { Name = "Dune", ReleaseYear = 2021, MediaType = MediaType.Movie });
        Context.SaveChanges();

        var act = () => CreateService().CreateAsync(new TitleCreateDto
        {
            Name = "Dune ", // same title after Trim() — must hit the service rule, not the DB index
            ReleaseYear = 2021,
            MediaType = MediaType.Movie,
        });

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Update_KeepingExistingGenreWhileAddingAnother_Succeeds()
    {
        var action = new Genre { Name = "Action" };
        var drama = new Genre { Name = "Drama" };
        var title = new Title
        {
            Name = "Inception",
            ReleaseYear = 2010,
            MediaType = MediaType.Movie,
            TitleGenres = { new TitleGenre { Genre = action } }
        };
        Context.AddRange(title, drama);
        Context.SaveChanges();

        var result = await CreateService().UpdateAsync(title.Id, new TitleUpdateDto
        {
            Name = "Inception",
            ReleaseYear = 2010,
            GenreIds = [action.Id, drama.Id], // keeps Action, adds Drama
        });

        result.Genres.Select(g => g.Name).Should().BeEquivalentTo("Action", "Drama");
    }

    [Fact]
    public async Task Update_RenamingToItself_DoesNotConflict()
    {
        var title = new Title { Name = "Heat", ReleaseYear = 1995, MediaType = MediaType.Movie };
        Context.Add(title);
        Context.SaveChanges();

        var act = () => CreateService().UpdateAsync(title.Id, new TitleUpdateDto
        {
            Name = "Heat",
            ReleaseYear = 1995,
        });

        await act.Should().NotThrowAsync();
    }
}

using FluentAssertions;
using Moq;
using MovieReview.Api.Domain.Entities;
using MovieReview.Api.Domain.Enums;
using MovieReview.Api.Domain.Exceptions;
using MovieReview.Api.DTOs.Import;
using MovieReview.Api.DTOs.Titles;
using MovieReview.Api.External.Tmdb;
using MovieReview.Api.Repositories.Interfaces;
using MovieReview.Api.Services.Implementations;
using MovieReview.Api.Services.Interfaces;
using MovieReview.Tests.TestHelpers;

namespace MovieReview.Tests.Services;

public class TmdbImportServiceTests
{
    private readonly Mock<ITmdbClient> _tmdb = new();
    private readonly Mock<ITitleRepository> _titles = new();
    private readonly Mock<IGenreRepository> _genres = new();
    private readonly Mock<IPersonRepository> _people = new();
    private readonly Mock<ITitleService> _titleService = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly TmdbImportService _sut;

    public TmdbImportServiceTests()
    {
        // The unit-of-work mock simply executes the supplied action.
        _unitOfWork
            .Setup(u => u.ExecuteInTransactionAsync(It.IsAny<Func<Task<int>>>(), It.IsAny<CancellationToken>()))
            .Returns<Func<Task<int>>, CancellationToken>((action, _) => action());

        _tmdb.Setup(c => c.BuildImageUrl(It.IsAny<string?>(), It.IsAny<string>()))
             .Returns<string?, string>((path, size) =>
                 string.IsNullOrWhiteSpace(path) ? null : $"https://image.tmdb.org/t/p/{size}{path}");

        _sut = new TmdbImportService(
            _tmdb.Object, _titles.Object, _genres.Object, _people.Object,
            _titleService.Object, _unitOfWork.Object);
    }

    private static TmdbMovieDetails Movie() => new()
    {
        Id = 27205,
        Title = "Inception",
        Overview = "A thief who steals corporate secrets...",
        ReleaseDate = "2010-07-15",
        Runtime = 148,
        PosterPath = "/poster.jpg",
        BackdropPath = "/backdrop.jpg",
        Genres = [new TmdbGenre { Id = 28, Name = "Action" }, new TmdbGenre { Id = 878, Name = "Science Fiction" }],
        Credits = new TmdbCredits
        {
            Cast =
            [
                new TmdbCastMember { Id = 6193, Name = "Leonardo DiCaprio", Character = "Dom Cobb", Order = 0 },
                new TmdbCastMember { Id = 24045, Name = "Joseph Gordon-Levitt", Character = "Arthur", Order = 1 }
            ],
            Crew = [new TmdbCrewMember { Name = "Christopher Nolan", Job = "Director" }]
        }
    };

    [Fact]
    public async Task Search_WithEmptyQuery_ThrowsBadRequest()
    {
        var act = () => _sut.SearchAsync("  ");

        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Search_FiltersOutPeople_AndFlagsImportedEntries()
    {
        _tmdb.Setup(c => c.SearchMultiAsync("inception", It.IsAny<CancellationToken>()))
             .ReturnsAsync(new TmdbSearchResponse
             {
                 Results =
                 [
                     new TmdbSearchResult { Id = 27205, MediaTypeRaw = "movie", MovieTitle = "Inception", ReleaseDate = "2010-07-15" },
                     new TmdbSearchResult { Id = 1, MediaTypeRaw = "person", TvName = "Some Actor" },
                     new TmdbSearchResult { Id = 66732, MediaTypeRaw = "tv", TvName = "Stranger Things", FirstAirDate = "2016-07-15" }
                 ]
             });
        _titles.Setup(r => r.GetByTmdbIdAsync(27205, MediaType.Movie, It.IsAny<CancellationToken>()))
               .ReturnsAsync(TestData.Title(id: 42, tmdbId: 27205));

        var results = await _sut.SearchAsync("inception");

        results.Should().HaveCount(2); // person filtered out
        var movie = results.Single(r => r.MediaType == MediaType.Movie);
        movie.AlreadyImported.Should().BeTrue();
        movie.TitleId.Should().Be(42);
        movie.ReleaseYear.Should().Be(2010);
        results.Single(r => r.MediaType == MediaType.TvShow).AlreadyImported.Should().BeFalse();
    }

    [Fact]
    public async Task Import_WhenAlreadyImported_ThrowsConflict()
    {
        _titles.Setup(r => r.GetByTmdbIdAsync(27205, MediaType.Movie, It.IsAny<CancellationToken>()))
               .ReturnsAsync(TestData.Title(tmdbId: 27205));

        var act = () => _sut.ImportAsync(new TmdbImportRequestDto { TmdbId = 27205, MediaType = MediaType.Movie });

        await act.Should().ThrowAsync<ConflictException>().WithMessage("*already been imported*");
    }

    [Fact]
    public async Task Import_WhenTmdbHasNoSuchMovie_ThrowsNotFound()
    {
        _tmdb.Setup(c => c.GetMovieWithCreditsAsync(999, It.IsAny<CancellationToken>()))
             .ReturnsAsync((TmdbMovieDetails?)null);

        var act = () => _sut.ImportAsync(new TmdbImportRequestDto { TmdbId = 999, MediaType = MediaType.Movie });

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Import_Movie_CreatesTitleWithGenresCastAndDirector()
    {
        Title? saved = null;
        _tmdb.Setup(c => c.GetMovieWithCreditsAsync(27205, It.IsAny<CancellationToken>())).ReturnsAsync(Movie());
        _genres.Setup(r => r.GetByTmdbIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync([TestData.Genre(id: 1, name: "Action", tmdbId: 28)]);
        _genres.Setup(r => r.AddAsync(It.IsAny<Genre>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync((Genre g, CancellationToken _) => { g.Id = 2; return g; });
        _people.Setup(r => r.GetByTmdbIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync([]);
        _titles.Setup(r => r.AddAsync(It.IsAny<Title>(), It.IsAny<CancellationToken>()))
               .Callback<Title, CancellationToken>((t, _) => { t.Id = 7; saved = t; })
               .ReturnsAsync((Title t, CancellationToken _) => t);
        _titleService.Setup(s => s.GetByIdAsync(7, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new TitleDetailDto { Id = 7, Name = "Inception" });

        var result = await _sut.ImportAsync(new TmdbImportRequestDto { TmdbId = 27205, MediaType = MediaType.Movie });

        result.Id.Should().Be(7);
        saved.Should().NotBeNull();
        saved!.TmdbId.Should().Be(27205);
        saved.Director.Should().Be("Christopher Nolan");
        saved.ReleaseYear.Should().Be(2010);
        saved.RuntimeMinutes.Should().Be(148);
        saved.PosterUrl.Should().Contain("w500");
        saved.BackdropUrl.Should().Contain("w1280");
        saved.TitleGenres.Should().HaveCount(2);
        saved.CastMembers.Should().HaveCount(2);
        saved.CastMembers.Should().Contain(cm => cm.CharacterName == "Dom Cobb");

        // The existing "Action" genre (TMDB 28) is reused; only "Science Fiction" is created.
        _genres.Verify(r => r.AddAsync(It.Is<Genre>(g => g.Name == "Science Fiction"), It.IsAny<CancellationToken>()), Times.Once);
        _genres.Verify(r => r.AddAsync(It.Is<Genre>(g => g.Name == "Action"), It.IsAny<CancellationToken>()), Times.Never);
        // Both actors are new and inserted in one batch.
        _people.Verify(r => r.AddRangeAsync(It.Is<IEnumerable<Person>>(p => p.Count() == 2), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Import_ReusesGenreWithSameName_FromManualCreation()
    {
        var movie = Movie();
        movie.Genres = [new TmdbGenre { Id = 28, Name = "Action" }];
        _tmdb.Setup(c => c.GetMovieWithCreditsAsync(27205, It.IsAny<CancellationToken>())).ReturnsAsync(movie);
        _genres.Setup(r => r.GetByTmdbIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync([]);
        var manualGenre = TestData.Genre(id: 9, name: "Action", tmdbId: null);
        _genres.Setup(r => r.GetByNameAsync("Action", It.IsAny<CancellationToken>())).ReturnsAsync(manualGenre);
        _people.Setup(r => r.GetByTmdbIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync([]);
        _titles.Setup(r => r.AddAsync(It.IsAny<Title>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync((Title t, CancellationToken _) => t);
        _titleService.Setup(s => s.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new TitleDetailDto());

        await _sut.ImportAsync(new TmdbImportRequestDto { TmdbId = 27205, MediaType = MediaType.Movie });

        manualGenre.TmdbId.Should().Be(28); // adopted the TMDB id
        _genres.Verify(r => r.AddAsync(It.IsAny<Genre>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}

using MovieReview.Api.Domain.Entities;
using MovieReview.Api.Domain.Enums;
using MovieReview.Api.Domain.Exceptions;
using MovieReview.Api.DTOs.Import;
using MovieReview.Api.DTOs.Titles;
using MovieReview.Api.External.Tmdb;
using MovieReview.Api.Repositories.Interfaces;
using MovieReview.Api.Services.Interfaces;

namespace MovieReview.Api.Services.Implementations;

/// <summary>
/// Orchestrates calls to the external TMDB service and several repositories:
/// fetch details + credits, dedupe genres and people against the local catalog,
/// then create the title with its cast atomically.
/// </summary>
public class TmdbImportService : ITmdbImportService
{
    /// <summary>Number of top-billed actors imported per title.</summary>
    private const int MaxCast = 12;

    private const string PosterSize = "w500";
    private const string BackdropSize = "w1280";
    private const string ProfileSize = "w342";

    private readonly ITmdbClient _tmdb;
    private readonly ITitleRepository _titles;
    private readonly IGenreRepository _genres;
    private readonly IPersonRepository _people;
    private readonly ITitleService _titleService;
    private readonly IUnitOfWork _unitOfWork;

    public TmdbImportService(
        ITmdbClient tmdb,
        ITitleRepository titles,
        IGenreRepository genres,
        IPersonRepository people,
        ITitleService titleService,
        IUnitOfWork unitOfWork)
    {
        _tmdb = tmdb;
        _titles = titles;
        _genres = genres;
        _people = people;
        _titleService = titleService;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<TmdbSearchItemDto>> SearchAsync(string query, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            throw new BadRequestException("Search query must not be empty.");

        var response = await _tmdb.SearchMultiAsync(query.Trim(), ct);
        var items = new List<TmdbSearchItemDto>();

        foreach (var result in response.Results)
        {
            var mediaType = result.MediaTypeRaw switch
            {
                "movie" => (MediaType?)MediaType.Movie,
                "tv" => MediaType.TvShow,
                _ => null
            };
            if (mediaType is null) continue;

            var existing = await _titles.GetByTmdbIdAsync(result.Id, mediaType.Value, ct);
            var dateString = mediaType == MediaType.Movie ? result.ReleaseDate : result.FirstAirDate;

            items.Add(new TmdbSearchItemDto
            {
                TmdbId = result.Id,
                MediaType = mediaType.Value,
                Name = (mediaType == MediaType.Movie ? result.MovieTitle : result.TvName) ?? "(untitled)",
                ReleaseYear = ParseYear(dateString),
                Overview = result.Overview,
                PosterUrl = _tmdb.BuildImageUrl(result.PosterPath, PosterSize),
                VoteAverage = Math.Round(result.VoteAverage, 1),
                AlreadyImported = existing is not null,
                TitleId = existing?.Id
            });
        }

        return items;
    }

    public async Task<TitleDetailDto> ImportAsync(TmdbImportRequestDto request, CancellationToken ct = default)
    {
        // Business rule: each TMDB entry can be imported only once.
        if (await _titles.GetByTmdbIdAsync(request.TmdbId, request.MediaType, ct) is not null)
            throw new ConflictException("This title has already been imported.");

        var data = request.MediaType == MediaType.Movie
            ? await FetchMovieAsync(request.TmdbId, ct)
            : await FetchTvShowAsync(request.TmdbId, ct);

        var titleId = await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var genreLinks = await UpsertGenresAsync(data.Genres, ct);
            var castLinks = await UpsertCastAsync(data.Cast, ct);

            var title = new Title
            {
                TmdbId = request.TmdbId,
                MediaType = request.MediaType,
                Name = data.Name,
                Description = data.Overview,
                ReleaseYear = data.ReleaseYear,
                Director = data.Director,
                RuntimeMinutes = data.RuntimeMinutes,
                PosterUrl = data.PosterUrl,
                BackdropUrl = data.BackdropUrl,
                TitleGenres = genreLinks,
                CastMembers = castLinks
            };

            await _titles.AddAsync(title, ct);
            return title.Id;
        }, ct);

        return await _titleService.GetByIdAsync(titleId, ct);
    }

    // ---------- TMDB fetch ----------

    private sealed record ImportData(
        string Name, string? Overview, int ReleaseYear, string? Director, int? RuntimeMinutes,
        string? PosterUrl, string? BackdropUrl, List<TmdbGenre> Genres, List<TmdbCastMember> Cast);

    private async Task<ImportData> FetchMovieAsync(int tmdbId, CancellationToken ct)
    {
        var movie = await _tmdb.GetMovieWithCreditsAsync(tmdbId, ct)
            ?? throw new NotFoundException($"TMDB has no movie with id {tmdbId}.");

        return new ImportData(
            movie.Title,
            movie.Overview,
            ParseYear(movie.ReleaseDate) ?? 0,
            movie.Credits.Crew.FirstOrDefault(c => c.Job == "Director")?.Name,
            movie.Runtime is > 0 ? movie.Runtime : null,
            _tmdb.BuildImageUrl(movie.PosterPath, PosterSize),
            _tmdb.BuildImageUrl(movie.BackdropPath, BackdropSize),
            movie.Genres,
            TopCast(movie.Credits));
    }

    private async Task<ImportData> FetchTvShowAsync(int tmdbId, CancellationToken ct)
    {
        var show = await _tmdb.GetTvShowWithCreditsAsync(tmdbId, ct)
            ?? throw new NotFoundException($"TMDB has no TV show with id {tmdbId}.");

        return new ImportData(
            show.Name,
            show.Overview,
            ParseYear(show.FirstAirDate) ?? 0,
            show.CreatedBy.Count > 0 ? string.Join(", ", show.CreatedBy.Select(c => c.Name)) : null,
            show.EpisodeRunTime.Count > 0 ? show.EpisodeRunTime[0] : null,
            _tmdb.BuildImageUrl(show.PosterPath, PosterSize),
            _tmdb.BuildImageUrl(show.BackdropPath, BackdropSize),
            show.Genres,
            TopCast(show.Credits));
    }

    private static List<TmdbCastMember> TopCast(TmdbCredits credits) =>
        credits.Cast.OrderBy(c => c.Order).Take(MaxCast).ToList();

    private static int? ParseYear(string? date) =>
        !string.IsNullOrWhiteSpace(date) && date.Length >= 4 && int.TryParse(date[..4], out var year)
            ? year
            : null;

    // ---------- Local upserts ----------

    private async Task<List<TitleGenre>> UpsertGenresAsync(List<TmdbGenre> tmdbGenres, CancellationToken ct)
    {
        var links = new List<TitleGenre>();
        if (tmdbGenres.Count == 0) return links;

        var existingByTmdbId = (await _genres.GetByTmdbIdsAsync(tmdbGenres.Select(g => g.Id), ct))
            .ToDictionary(g => g.TmdbId!.Value);

        foreach (var tmdbGenre in tmdbGenres)
        {
            if (existingByTmdbId.TryGetValue(tmdbGenre.Id, out var genre))
            {
                links.Add(new TitleGenre { GenreId = genre.Id });
                continue;
            }

            // A genre with the same name may already exist from manual creation —
            // adopt it (and remember its TMDB id) instead of violating the unique name index.
            var byName = await _genres.GetByNameAsync(tmdbGenre.Name, ct);
            if (byName is not null)
            {
                byName.TmdbId ??= tmdbGenre.Id;
                links.Add(new TitleGenre { GenreId = byName.Id });
                continue;
            }

            var created = await _genres.AddAsync(new Genre { Name = tmdbGenre.Name, TmdbId = tmdbGenre.Id }, ct);
            links.Add(new TitleGenre { GenreId = created.Id });
        }

        return links;
    }

    private async Task<List<CastMember>> UpsertCastAsync(List<TmdbCastMember> cast, CancellationToken ct)
    {
        var links = new List<CastMember>();
        if (cast.Count == 0) return links;

        var existing = (await _people.GetByTmdbIdsAsync(cast.Select(c => c.Id), ct))
            .ToDictionary(p => p.TmdbId!.Value);

        var missing = cast
            .Where(c => !existing.ContainsKey(c.Id))
            .DistinctBy(c => c.Id)
            .Select(c => new Person
            {
                TmdbId = c.Id,
                Name = c.Name,
                ProfileImageUrl = _tmdb.BuildImageUrl(c.ProfilePath, ProfileSize)
            })
            .ToList();

        if (missing.Count > 0)
        {
            await _people.AddRangeAsync(missing, ct);
            foreach (var person in missing)
                existing[person.TmdbId!.Value] = person;
        }

        foreach (var member in cast.DistinctBy(c => c.Id))
        {
            links.Add(new CastMember
            {
                PersonId = existing[member.Id].Id,
                CharacterName = member.Character,
                CastOrder = member.Order
            });
        }

        return links;
    }
}

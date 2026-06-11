using MovieReview.Api.DTOs.Import;
using MovieReview.Api.DTOs.Titles;

namespace MovieReview.Api.Services.Interfaces;

public interface ITmdbImportService
{
    /// <summary>Searches TMDB for movies and TV shows, flagging entries already in the local catalog.</summary>
    Task<IReadOnlyList<TmdbSearchItemDto>> SearchAsync(string query, CancellationToken ct = default);

    /// <summary>
    /// Imports a movie or TV show from TMDB: fetches details and credits, upserts genres
    /// and people, and creates the title with its cast — all in one transaction.
    /// </summary>
    Task<TitleDetailDto> ImportAsync(TmdbImportRequestDto request, CancellationToken ct = default);
}

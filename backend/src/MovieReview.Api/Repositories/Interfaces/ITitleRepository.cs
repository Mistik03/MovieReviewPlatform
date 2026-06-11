using MovieReview.Api.Domain.Entities;
using MovieReview.Api.Domain.Enums;
using MovieReview.Api.Domain.Queries;
using MovieReview.Api.Domain.ReadModels;

namespace MovieReview.Api.Repositories.Interfaces;

public interface ITitleRepository
{
    Task<Title?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>Loads a title with genres and the ordered cast for the detail page.</summary>
    Task<Title?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default);

    Task<TitleWithStats?> GetStatsByIdAsync(int id, CancellationToken ct = default);

    Task<(IReadOnlyList<TitleWithStats> Items, int TotalCount)> GetPagedWithStatsAsync(
        TitleQueryParams query, CancellationToken ct = default);

    Task<Title?> GetByTmdbIdAsync(int tmdbId, MediaType mediaType, CancellationToken ct = default);

    Task<bool> ExistsByNameYearTypeAsync(
        string name, int releaseYear, MediaType mediaType, int? excludeId = null, CancellationToken ct = default);

    Task<bool> HasReviewsOrRatingsAsync(int titleId, CancellationToken ct = default);

    Task<Title> AddAsync(Title title, CancellationToken ct = default);
    Task UpdateAsync(Title title, CancellationToken ct = default);
    Task DeleteAsync(Title title, CancellationToken ct = default);
}

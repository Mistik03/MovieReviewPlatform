using MovieReview.Api.Domain.Entities;
using MovieReview.Api.Domain.ReadModels;

namespace MovieReview.Api.Repositories.Interfaces;

public interface IGenreRepository
{
    Task<IReadOnlyList<GenreWithCount>> GetAllWithCountsAsync(CancellationToken ct = default);
    Task<Genre?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Genre?> GetByNameAsync(string name, CancellationToken ct = default);

    /// <summary>Fetches all genres matching the given TMDB ids — used by the import to dedupe.</summary>
    Task<IReadOnlyList<Genre>> GetByTmdbIdsAsync(IEnumerable<int> tmdbIds, CancellationToken ct = default);

    Task<int> CountTitlesAsync(int genreId, CancellationToken ct = default);
    Task<Genre> AddAsync(Genre genre, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<Genre> genres, CancellationToken ct = default);
    Task DeleteAsync(Genre genre, CancellationToken ct = default);
}

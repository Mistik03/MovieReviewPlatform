using MovieReview.Api.Domain.Entities;

namespace MovieReview.Api.Repositories.Interfaces;

public interface IPersonRepository
{
    Task<Person?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>Fetches all people matching the given TMDB ids — used by the import to dedupe.</summary>
    Task<IReadOnlyList<Person>> GetByTmdbIdsAsync(IEnumerable<int> tmdbIds, CancellationToken ct = default);

    Task AddRangeAsync(IEnumerable<Person> people, CancellationToken ct = default);
}

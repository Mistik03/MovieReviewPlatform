using MovieReview.Api.Domain.Entities;

namespace MovieReview.Api.Repositories.Interfaces;

public interface IRatingRepository
{
    /// <summary>Loads a rating with its author and title.</summary>
    Task<Rating?> GetByIdAsync(int id, CancellationToken ct = default);

    Task<IReadOnlyList<Rating>> GetByTitleIdAsync(int titleId, CancellationToken ct = default);
    Task<IReadOnlyList<Rating>> GetByUserIdAsync(int userId, CancellationToken ct = default);
    Task<bool> ExistsForUserAndTitleAsync(int userId, int titleId, CancellationToken ct = default);

    Task<Rating> AddAsync(Rating rating, CancellationToken ct = default);
    Task UpdateAsync(Rating rating, CancellationToken ct = default);
    Task DeleteAsync(Rating rating, CancellationToken ct = default);
}

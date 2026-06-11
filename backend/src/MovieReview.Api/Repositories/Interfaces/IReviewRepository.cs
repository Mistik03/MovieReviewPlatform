using MovieReview.Api.Domain.Entities;

namespace MovieReview.Api.Repositories.Interfaces;

public interface IReviewRepository
{
    /// <summary>Loads a review with its author and title.</summary>
    Task<Review?> GetByIdAsync(int id, CancellationToken ct = default);

    Task<IReadOnlyList<Review>> GetByTitleIdAsync(int titleId, CancellationToken ct = default);
    Task<IReadOnlyList<Review>> GetByUserIdAsync(int userId, CancellationToken ct = default);
    Task<bool> ExistsForUserAndTitleAsync(int userId, int titleId, CancellationToken ct = default);

    Task<Review> AddAsync(Review review, CancellationToken ct = default);
    Task UpdateAsync(Review review, CancellationToken ct = default);
    Task DeleteAsync(Review review, CancellationToken ct = default);
}

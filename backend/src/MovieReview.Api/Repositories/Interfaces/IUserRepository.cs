using MovieReview.Api.Domain.Entities;

namespace MovieReview.Api.Repositories.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);

    /// <summary>Loads a user together with their reviews and ratings (titles included) for the profile page.</summary>
    Task<User?> GetProfileAsync(int id, CancellationToken ct = default);

    Task<User> AddAsync(User user, CancellationToken ct = default);
}

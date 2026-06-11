using MovieReview.Api.DTOs.Users;

namespace MovieReview.Api.Services.Interfaces;

public interface IUserService
{
    Task<UserProfileDto> GetProfileAsync(int userId, CancellationToken ct = default);
}

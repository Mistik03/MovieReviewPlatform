using MovieReview.Api.Domain.Exceptions;
using MovieReview.Api.DTOs.Users;
using MovieReview.Api.Repositories.Interfaces;
using MovieReview.Api.Services.Interfaces;
using MovieReview.Api.Services.Mapping;

namespace MovieReview.Api.Services.Implementations;

public class UserService : IUserService
{
    private readonly IUserRepository _users;

    public UserService(IUserRepository users)
    {
        _users = users;
    }

    public async Task<UserProfileDto> GetProfileAsync(int userId, CancellationToken ct = default)
    {
        var user = await _users.GetProfileAsync(userId, ct)
            ?? throw new NotFoundException($"User with id {userId} was not found.");

        return new UserProfileDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role,
            CreatedAt = user.CreatedAt,
            ReviewCount = user.Reviews.Count,
            RatingCount = user.Ratings.Count,
            Reviews = user.Reviews
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => DtoMapper.ToResponse(r, user.Username))
                .ToList(),
            Ratings = user.Ratings
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => DtoMapper.ToResponse(r, user.Username))
                .ToList()
        };
    }
}

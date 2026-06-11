using MovieReview.Api.Domain.Entities;

namespace MovieReview.Api.Auth;

public interface IJwtTokenService
{
    (string Token, DateTime ExpiresAtUtc) CreateToken(User user);
}

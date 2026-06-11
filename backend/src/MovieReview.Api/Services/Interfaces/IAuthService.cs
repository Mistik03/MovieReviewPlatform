using MovieReview.Api.DTOs.Auth;

namespace MovieReview.Api.Services.Interfaces;

public interface IAuthService
{
    /// <summary>Creates a new account with the default User role and signs the caller in.</summary>
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken ct = default);

    Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken ct = default);
}

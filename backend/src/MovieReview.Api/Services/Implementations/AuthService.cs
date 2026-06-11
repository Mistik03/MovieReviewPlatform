using MovieReview.Api.Auth;
using MovieReview.Api.Domain.Entities;
using MovieReview.Api.Domain.Enums;
using MovieReview.Api.Domain.Exceptions;
using MovieReview.Api.DTOs.Auth;
using MovieReview.Api.Repositories.Interfaces;
using MovieReview.Api.Services.Interfaces;

namespace MovieReview.Api.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IJwtTokenService _tokens;

    public AuthService(IUserRepository users, IJwtTokenService tokens)
    {
        _users = users;
        _tokens = tokens;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken ct = default)
    {
        if (await _users.GetByUsernameAsync(request.Username, ct) is not null)
            throw new ConflictException($"Username '{request.Username}' is already taken.");

        if (await _users.GetByEmailAsync(request.Email, ct) is not null)
            throw new ConflictException($"An account with email '{request.Email}' already exists.");

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = Roles.User
        };

        await _users.AddAsync(user, ct);
        return BuildResponse(user);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken ct = default)
    {
        var user = request.UsernameOrEmail.Contains('@')
            ? await _users.GetByEmailAsync(request.UsernameOrEmail, ct)
            : await _users.GetByUsernameAsync(request.UsernameOrEmail, ct);

        // Same message for unknown user and wrong password so the endpoint
        // cannot be used to probe which accounts exist.
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedException("Invalid username/email or password.");

        return BuildResponse(user);
    }

    private AuthResponseDto BuildResponse(User user)
    {
        var (token, expiresAtUtc) = _tokens.CreateToken(user);
        return new AuthResponseDto
        {
            Token = token,
            ExpiresAtUtc = expiresAtUtc,
            UserId = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role
        };
    }
}

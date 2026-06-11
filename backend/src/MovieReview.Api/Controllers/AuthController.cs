using Microsoft.AspNetCore.Mvc;
using MovieReview.Api.DTOs.Auth;
using MovieReview.Api.Services.Interfaces;

namespace MovieReview.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth)
    {
        _auth = auth;
    }

    /// <summary>Creates a new account (default role: User) and returns a JWT.</summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterRequestDto request, CancellationToken ct)
    {
        var response = await _auth.RegisterAsync(request, ct);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    /// <summary>Validates credentials and returns a JWT.</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginRequestDto request, CancellationToken ct) =>
        Ok(await _auth.LoginAsync(request, ct));
}

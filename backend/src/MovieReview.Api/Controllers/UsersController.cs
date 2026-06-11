using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieReview.Api.Auth;
using MovieReview.Api.DTOs.Users;
using MovieReview.Api.Services.Interfaces;

namespace MovieReview.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _users;

    public UsersController(IUserService users)
    {
        _users = users;
    }

    /// <summary>Gets the authenticated user's profile with their reviews and ratings.</summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<UserProfileDto>> Me(CancellationToken ct) =>
        Ok(await _users.GetProfileAsync(User.GetUserId(), ct));
}

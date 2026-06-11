using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieReview.Api.Auth;
using MovieReview.Api.DTOs.Ratings;
using MovieReview.Api.Services.Interfaces;

namespace MovieReview.Api.Controllers;

[ApiController]
[Route("api/ratings")]
public class RatingsController : ControllerBase
{
    private readonly IRatingService _ratings;

    public RatingsController(IRatingService ratings)
    {
        _ratings = ratings;
    }

    /// <summary>Gets all ratings for a title.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<RatingResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<RatingResponseDto>>> GetByTitle(
        [FromQuery] int titleId, CancellationToken ct) =>
        Ok(await _ratings.GetByTitleAsync(titleId, ct));

    /// <summary>Gets the authenticated user's ratings.</summary>
    [HttpGet("mine")]
    [Authorize]
    [ProducesResponseType(typeof(IReadOnlyList<RatingResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<RatingResponseDto>>> GetMine(CancellationToken ct) =>
        Ok(await _ratings.GetByUserAsync(User.GetUserId(), ct));

    /// <summary>Posts a rating (1-10) — one per user per title.</summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(RatingResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RatingResponseDto>> Create(RatingCreateDto dto, CancellationToken ct)
    {
        var created = await _ratings.CreateAsync(User.GetUserId(), dto, ct);
        return StatusCode(StatusCodes.Status201Created, created);
    }

    /// <summary>Updates the caller's own rating (admins may update any).</summary>
    [HttpPut("{id:int}")]
    [Authorize]
    [ProducesResponseType(typeof(RatingResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RatingResponseDto>> Update(int id, RatingUpdateDto dto, CancellationToken ct) =>
        Ok(await _ratings.UpdateAsync(id, User.GetUserId(), User.IsAdmin(), dto, ct));

    /// <summary>Deletes the caller's own rating (admins may delete any).</summary>
    [HttpDelete("{id:int}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _ratings.DeleteAsync(id, User.GetUserId(), User.IsAdmin(), ct);
        return NoContent();
    }
}

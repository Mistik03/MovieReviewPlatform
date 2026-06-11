using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieReview.Api.Auth;
using MovieReview.Api.DTOs.Reviews;
using MovieReview.Api.Services.Interfaces;

namespace MovieReview.Api.Controllers;

[ApiController]
[Route("api/reviews")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviews;

    public ReviewsController(IReviewService reviews)
    {
        _reviews = reviews;
    }

    /// <summary>Gets all reviews for a title.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ReviewResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<ReviewResponseDto>>> GetByTitle(
        [FromQuery] int titleId, CancellationToken ct) =>
        Ok(await _reviews.GetByTitleAsync(titleId, ct));

    /// <summary>Gets the authenticated user's reviews.</summary>
    [HttpGet("mine")]
    [Authorize]
    [ProducesResponseType(typeof(IReadOnlyList<ReviewResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ReviewResponseDto>>> GetMine(CancellationToken ct) =>
        Ok(await _reviews.GetByUserAsync(User.GetUserId(), ct));

    /// <summary>Gets a review by id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ReviewResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReviewResponseDto>> GetById(int id, CancellationToken ct) =>
        Ok(await _reviews.GetByIdAsync(id, ct));

    /// <summary>Creates a review — one per user per title.</summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ReviewResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ReviewResponseDto>> Create(ReviewCreateDto dto, CancellationToken ct)
    {
        var created = await _reviews.CreateAsync(User.GetUserId(), dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Edits the caller's own review (admins may edit any).</summary>
    [HttpPut("{id:int}")]
    [Authorize]
    [ProducesResponseType(typeof(ReviewResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReviewResponseDto>> Update(int id, ReviewUpdateDto dto, CancellationToken ct) =>
        Ok(await _reviews.UpdateAsync(id, User.GetUserId(), User.IsAdmin(), dto, ct));

    /// <summary>Deletes the caller's own review (admins may delete any).</summary>
    [HttpDelete("{id:int}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _reviews.DeleteAsync(id, User.GetUserId(), User.IsAdmin(), ct);
        return NoContent();
    }
}

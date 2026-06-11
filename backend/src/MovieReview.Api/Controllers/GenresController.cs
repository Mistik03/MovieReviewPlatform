using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieReview.Api.Domain.Enums;
using MovieReview.Api.DTOs.Genres;
using MovieReview.Api.Services.Interfaces;

namespace MovieReview.Api.Controllers;

[ApiController]
[Route("api/genres")]
public class GenresController : ControllerBase
{
    private readonly IGenreService _genres;

    public GenresController(IGenreService genres)
    {
        _genres = genres;
    }

    /// <summary>Lists all genres with the number of titles in each.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<GenreResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<GenreResponseDto>>> GetAll(CancellationToken ct) =>
        Ok(await _genres.GetAllAsync(ct));

    /// <summary>Gets a genre by id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(GenreResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GenreResponseDto>> GetById(int id, CancellationToken ct) =>
        Ok(await _genres.GetByIdAsync(id, ct));

    /// <summary>Creates a genre. Admin only.</summary>
    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(GenreResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<GenreResponseDto>> Create(GenreCreateDto dto, CancellationToken ct)
    {
        var created = await _genres.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Deletes a genre that has no titles assigned. Admin only.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _genres.DeleteAsync(id, ct);
        return NoContent();
    }
}

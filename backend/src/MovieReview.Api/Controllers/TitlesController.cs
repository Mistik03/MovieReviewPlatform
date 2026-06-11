using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieReview.Api.Domain.Enums;
using MovieReview.Api.Domain.Queries;
using MovieReview.Api.DTOs.Common;
using MovieReview.Api.DTOs.Titles;
using MovieReview.Api.Services.Interfaces;

namespace MovieReview.Api.Controllers;

[ApiController]
[Route("api/titles")]
public class TitlesController : ControllerBase
{
    private readonly ITitleService _titles;

    public TitlesController(ITitleService titles)
    {
        _titles = titles;
    }

    /// <summary>Browses the catalog. Filter by mediaType, genreId and search; sort by newest, name, year or rating.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<TitleResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResultDto<TitleResponseDto>>> GetAll([FromQuery] TitleQueryParams query, CancellationToken ct) =>
        Ok(await _titles.GetPagedAsync(query, ct));

    /// <summary>Gets a title with genres, cast and computed rating statistics.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TitleDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TitleDetailDto>> GetById(int id, CancellationToken ct) =>
        Ok(await _titles.GetByIdAsync(id, ct));

    /// <summary>Creates a title manually. Admin only.</summary>
    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(TitleDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TitleDetailDto>> Create(TitleCreateDto dto, CancellationToken ct)
    {
        var created = await _titles.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Updates a title. Admin only.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(TitleDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TitleDetailDto>> Update(int id, TitleUpdateDto dto, CancellationToken ct) =>
        Ok(await _titles.UpdateAsync(id, dto, ct));

    /// <summary>Deletes a title that has no reviews or ratings. Admin only.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _titles.DeleteAsync(id, ct);
        return NoContent();
    }
}

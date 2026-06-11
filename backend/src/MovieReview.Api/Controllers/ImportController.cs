using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieReview.Api.Domain.Enums;
using MovieReview.Api.DTOs.Import;
using MovieReview.Api.DTOs.Titles;
using MovieReview.Api.Services.Interfaces;

namespace MovieReview.Api.Controllers;

[ApiController]
[Route("api/import/tmdb")]
[Authorize(Roles = Roles.Admin)]
public class ImportController : ControllerBase
{
    private readonly ITmdbImportService _import;

    public ImportController(ITmdbImportService import)
    {
        _import = import;
    }

    /// <summary>Searches TMDB for movies and TV shows to import. Admin only.</summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(IReadOnlyList<TmdbSearchItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TmdbSearchItemDto>>> Search(
        [FromQuery] string query, CancellationToken ct) =>
        Ok(await _import.SearchAsync(query, ct));

    /// <summary>Imports a TMDB movie or TV show with genres and cast. Admin only.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(TitleDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TitleDetailDto>> Import(TmdbImportRequestDto request, CancellationToken ct)
    {
        var created = await _import.ImportAsync(request, ct);
        return CreatedAtAction("GetById", "Titles", new { id = created.Id }, created);
    }
}

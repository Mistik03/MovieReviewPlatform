using MovieReview.Api.Domain.Entities;
using MovieReview.Api.Domain.Exceptions;
using MovieReview.Api.DTOs.Genres;
using MovieReview.Api.Repositories.Interfaces;
using MovieReview.Api.Services.Interfaces;
using MovieReview.Api.Services.Mapping;

namespace MovieReview.Api.Services.Implementations;

public class GenreService : IGenreService
{
    private readonly IGenreRepository _genres;

    public GenreService(IGenreRepository genres)
    {
        _genres = genres;
    }

    public async Task<IReadOnlyList<GenreResponseDto>> GetAllAsync(CancellationToken ct = default)
    {
        var rows = await _genres.GetAllWithCountsAsync(ct);
        return rows.Select(DtoMapper.ToResponse).ToList();
    }

    public async Task<GenreResponseDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var genre = await _genres.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Genre with id {id} was not found.");

        var titleCount = await _genres.CountTitlesAsync(id, ct);
        return DtoMapper.ToResponse(genre, titleCount);
    }

    public async Task<GenreResponseDto> CreateAsync(GenreCreateDto dto, CancellationToken ct = default)
    {
        var name = dto.Name.Trim();

        // Business rule: genre names are unique (case-insensitive).
        if (await _genres.GetByNameAsync(name, ct) is not null)
            throw new ConflictException($"Genre '{name}' already exists.");

        var genre = await _genres.AddAsync(new Genre { Name = name }, ct);
        return DtoMapper.ToResponse(genre, 0);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var genre = await _genres.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Genre with id {id} was not found.");

        // Business rule: a genre that still has titles assigned cannot be removed.
        var titleCount = await _genres.CountTitlesAsync(id, ct);
        if (titleCount > 0)
            throw new ConflictException($"Genre '{genre.Name}' is assigned to {titleCount} title(s) and cannot be deleted.");

        await _genres.DeleteAsync(genre, ct);
    }
}

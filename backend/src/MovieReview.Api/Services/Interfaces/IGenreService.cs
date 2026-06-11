using MovieReview.Api.DTOs.Genres;

namespace MovieReview.Api.Services.Interfaces;

public interface IGenreService
{
    Task<IReadOnlyList<GenreResponseDto>> GetAllAsync(CancellationToken ct = default);
    Task<GenreResponseDto> GetByIdAsync(int id, CancellationToken ct = default);
    Task<GenreResponseDto> CreateAsync(GenreCreateDto dto, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}

using MovieReview.Api.Domain.Queries;
using MovieReview.Api.DTOs.Common;
using MovieReview.Api.DTOs.Titles;

namespace MovieReview.Api.Services.Interfaces;

public interface ITitleService
{
    Task<PagedResultDto<TitleResponseDto>> GetPagedAsync(TitleQueryParams query, CancellationToken ct = default);
    Task<TitleDetailDto> GetByIdAsync(int id, CancellationToken ct = default);
    Task<TitleDetailDto> CreateAsync(TitleCreateDto dto, CancellationToken ct = default);
    Task<TitleDetailDto> UpdateAsync(int id, TitleUpdateDto dto, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}

using MovieReview.Api.DTOs.Ratings;

namespace MovieReview.Api.Services.Interfaces;

public interface IRatingService
{
    Task<IReadOnlyList<RatingResponseDto>> GetByTitleAsync(int titleId, CancellationToken ct = default);
    Task<IReadOnlyList<RatingResponseDto>> GetByUserAsync(int userId, CancellationToken ct = default);
    Task<RatingResponseDto> CreateAsync(int userId, RatingCreateDto dto, CancellationToken ct = default);
    Task<RatingResponseDto> UpdateAsync(int id, int callerId, bool callerIsAdmin, RatingUpdateDto dto, CancellationToken ct = default);
    Task DeleteAsync(int id, int callerId, bool callerIsAdmin, CancellationToken ct = default);
}

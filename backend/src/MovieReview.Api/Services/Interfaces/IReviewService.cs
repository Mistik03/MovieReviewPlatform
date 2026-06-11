using MovieReview.Api.DTOs.Reviews;

namespace MovieReview.Api.Services.Interfaces;

public interface IReviewService
{
    Task<IReadOnlyList<ReviewResponseDto>> GetByTitleAsync(int titleId, CancellationToken ct = default);
    Task<ReviewResponseDto> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<ReviewResponseDto>> GetByUserAsync(int userId, CancellationToken ct = default);
    Task<ReviewResponseDto> CreateAsync(int userId, ReviewCreateDto dto, CancellationToken ct = default);
    Task<ReviewResponseDto> UpdateAsync(int id, int callerId, bool callerIsAdmin, ReviewUpdateDto dto, CancellationToken ct = default);
    Task DeleteAsync(int id, int callerId, bool callerIsAdmin, CancellationToken ct = default);
}

using MovieReview.Api.Domain.Entities;
using MovieReview.Api.Domain.Exceptions;
using MovieReview.Api.DTOs.Reviews;
using MovieReview.Api.Repositories.Interfaces;
using MovieReview.Api.Services.Interfaces;
using MovieReview.Api.Services.Mapping;

namespace MovieReview.Api.Services.Implementations;

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviews;
    private readonly ITitleRepository _titles;
    private readonly IUserRepository _users;

    public ReviewService(IReviewRepository reviews, ITitleRepository titles, IUserRepository users)
    {
        _reviews = reviews;
        _titles = titles;
        _users = users;
    }

    public async Task<IReadOnlyList<ReviewResponseDto>> GetByTitleAsync(int titleId, CancellationToken ct = default)
    {
        _ = await _titles.GetByIdAsync(titleId, ct)
            ?? throw new NotFoundException($"Title with id {titleId} was not found.");

        var reviews = await _reviews.GetByTitleIdAsync(titleId, ct);
        return reviews.Select(r => DtoMapper.ToResponse(r)).ToList();
    }

    public async Task<ReviewResponseDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var review = await _reviews.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Review with id {id} was not found.");
        return DtoMapper.ToResponse(review);
    }

    public async Task<IReadOnlyList<ReviewResponseDto>> GetByUserAsync(int userId, CancellationToken ct = default)
    {
        var reviews = await _reviews.GetByUserIdAsync(userId, ct);
        return reviews.Select(r => DtoMapper.ToResponse(r)).ToList();
    }

    public async Task<ReviewResponseDto> CreateAsync(int userId, ReviewCreateDto dto, CancellationToken ct = default)
    {
        var title = await _titles.GetByIdAsync(dto.TitleId, ct)
            ?? throw new NotFoundException($"Title with id {dto.TitleId} was not found.");

        // Business rule: one review per user per title.
        if (await _reviews.ExistsForUserAndTitleAsync(userId, dto.TitleId, ct))
            throw new ConflictException("You have already reviewed this title.");

        var user = await _users.GetByIdAsync(userId, ct)
            ?? throw new NotFoundException($"User with id {userId} was not found.");

        var review = new Review
        {
            Content = dto.Content.Trim(),
            UserId = userId,
            TitleId = dto.TitleId
        };

        await _reviews.AddAsync(review, ct);
        review.User = user;
        review.Title = title;
        return DtoMapper.ToResponse(review);
    }

    public async Task<ReviewResponseDto> UpdateAsync(
        int id, int callerId, bool callerIsAdmin, ReviewUpdateDto dto, CancellationToken ct = default)
    {
        var review = await _reviews.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Review with id {id} was not found.");

        // Business rule: users may only edit their own reviews; admins are exempt.
        if (review.UserId != callerId && !callerIsAdmin)
            throw new ForbiddenException("You can only edit your own reviews.");

        review.Content = dto.Content.Trim();
        review.UpdatedAt = DateTime.UtcNow;
        await _reviews.UpdateAsync(review, ct);
        return DtoMapper.ToResponse(review);
    }

    public async Task DeleteAsync(int id, int callerId, bool callerIsAdmin, CancellationToken ct = default)
    {
        var review = await _reviews.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Review with id {id} was not found.");

        // Business rule: users may only delete their own reviews; admins may delete any.
        if (review.UserId != callerId && !callerIsAdmin)
            throw new ForbiddenException("You can only delete your own reviews.");

        await _reviews.DeleteAsync(review, ct);
    }
}

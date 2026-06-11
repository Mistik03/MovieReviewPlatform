using MovieReview.Api.Domain.Entities;
using MovieReview.Api.Domain.Exceptions;
using MovieReview.Api.DTOs.Ratings;
using MovieReview.Api.Repositories.Interfaces;
using MovieReview.Api.Services.Interfaces;
using MovieReview.Api.Services.Mapping;

namespace MovieReview.Api.Services.Implementations;

public class RatingService : IRatingService
{
    private readonly IRatingRepository _ratings;
    private readonly ITitleRepository _titles;
    private readonly IUserRepository _users;

    public RatingService(IRatingRepository ratings, ITitleRepository titles, IUserRepository users)
    {
        _ratings = ratings;
        _titles = titles;
        _users = users;
    }

    public async Task<IReadOnlyList<RatingResponseDto>> GetByTitleAsync(int titleId, CancellationToken ct = default)
    {
        _ = await _titles.GetByIdAsync(titleId, ct)
            ?? throw new NotFoundException($"Title with id {titleId} was not found.");

        var ratings = await _ratings.GetByTitleIdAsync(titleId, ct);
        return ratings.Select(r => DtoMapper.ToResponse(r)).ToList();
    }

    public async Task<IReadOnlyList<RatingResponseDto>> GetByUserAsync(int userId, CancellationToken ct = default)
    {
        var ratings = await _ratings.GetByUserIdAsync(userId, ct);
        return ratings.Select(r => DtoMapper.ToResponse(r)).ToList();
    }

    public async Task<RatingResponseDto> CreateAsync(int userId, RatingCreateDto dto, CancellationToken ct = default)
    {
        ValidateScore(dto.Score);

        var title = await _titles.GetByIdAsync(dto.TitleId, ct)
            ?? throw new NotFoundException($"Title with id {dto.TitleId} was not found.");

        // Business rule: one rating per user per title.
        if (await _ratings.ExistsForUserAndTitleAsync(userId, dto.TitleId, ct))
            throw new ConflictException("You have already rated this title.");

        var user = await _users.GetByIdAsync(userId, ct)
            ?? throw new NotFoundException($"User with id {userId} was not found.");

        var rating = new Rating
        {
            Score = dto.Score,
            UserId = userId,
            TitleId = dto.TitleId
        };

        await _ratings.AddAsync(rating, ct);
        rating.User = user;
        rating.Title = title;
        return DtoMapper.ToResponse(rating);
    }

    public async Task<RatingResponseDto> UpdateAsync(
        int id, int callerId, bool callerIsAdmin, RatingUpdateDto dto, CancellationToken ct = default)
    {
        ValidateScore(dto.Score);

        var rating = await _ratings.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Rating with id {id} was not found.");

        // Business rule: users may only edit their own ratings; admins are exempt.
        if (rating.UserId != callerId && !callerIsAdmin)
            throw new ForbiddenException("You can only edit your own ratings.");

        rating.Score = dto.Score;
        rating.UpdatedAt = DateTime.UtcNow;
        await _ratings.UpdateAsync(rating, ct);
        return DtoMapper.ToResponse(rating);
    }

    public async Task DeleteAsync(int id, int callerId, bool callerIsAdmin, CancellationToken ct = default)
    {
        var rating = await _ratings.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Rating with id {id} was not found.");

        // Business rule: users may only delete their own ratings; admins may delete any.
        if (rating.UserId != callerId && !callerIsAdmin)
            throw new ForbiddenException("You can only delete your own ratings.");

        await _ratings.DeleteAsync(rating, ct);
    }

    /// <summary>Business rule: score must be between 1 and 10 — enforced here, independent of model validation.</summary>
    private static void ValidateScore(int score)
    {
        if (score is < 1 or > 10)
            throw new BadRequestException("Rating score must be between 1 and 10.");
    }
}

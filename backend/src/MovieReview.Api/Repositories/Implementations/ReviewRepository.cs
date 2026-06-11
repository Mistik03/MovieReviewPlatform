using Microsoft.EntityFrameworkCore;
using MovieReview.Api.Data;
using MovieReview.Api.Domain.Entities;
using MovieReview.Api.Repositories.Interfaces;

namespace MovieReview.Api.Repositories.Implementations;

public class ReviewRepository : IReviewRepository
{
    private readonly AppDbContext _context;

    public ReviewRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Review?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _context.Reviews
            .Include(r => r.User)
            .Include(r => r.Title)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<IReadOnlyList<Review>> GetByTitleIdAsync(int titleId, CancellationToken ct = default) =>
        await _context.Reviews
            .Include(r => r.User)
            .Include(r => r.Title)
            .Where(r => r.TitleId == titleId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Review>> GetByUserIdAsync(int userId, CancellationToken ct = default) =>
        await _context.Reviews
            .Include(r => r.User)
            .Include(r => r.Title)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);

    public Task<bool> ExistsForUserAndTitleAsync(int userId, int titleId, CancellationToken ct = default) =>
        _context.Reviews.AnyAsync(r => r.UserId == userId && r.TitleId == titleId, ct);

    public async Task<Review> AddAsync(Review review, CancellationToken ct = default)
    {
        _context.Reviews.Add(review);
        await _context.SaveChangesAsync(ct);
        return review;
    }

    public Task UpdateAsync(Review review, CancellationToken ct = default) =>
        _context.SaveChangesAsync(ct);

    public async Task DeleteAsync(Review review, CancellationToken ct = default)
    {
        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync(ct);
    }
}

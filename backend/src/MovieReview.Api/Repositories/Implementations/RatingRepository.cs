using Microsoft.EntityFrameworkCore;
using MovieReview.Api.Data;
using MovieReview.Api.Domain.Entities;
using MovieReview.Api.Repositories.Interfaces;

namespace MovieReview.Api.Repositories.Implementations;

public class RatingRepository : IRatingRepository
{
    private readonly AppDbContext _context;

    public RatingRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Rating?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _context.Ratings
            .Include(r => r.User)
            .Include(r => r.Title)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<IReadOnlyList<Rating>> GetByTitleIdAsync(int titleId, CancellationToken ct = default) =>
        await _context.Ratings
            .Include(r => r.User)
            .Include(r => r.Title)
            .Where(r => r.TitleId == titleId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Rating>> GetByUserIdAsync(int userId, CancellationToken ct = default) =>
        await _context.Ratings
            .Include(r => r.User)
            .Include(r => r.Title)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);

    public Task<bool> ExistsForUserAndTitleAsync(int userId, int titleId, CancellationToken ct = default) =>
        _context.Ratings.AnyAsync(r => r.UserId == userId && r.TitleId == titleId, ct);

    public async Task<Rating> AddAsync(Rating rating, CancellationToken ct = default)
    {
        _context.Ratings.Add(rating);
        await _context.SaveChangesAsync(ct);
        return rating;
    }

    public Task UpdateAsync(Rating rating, CancellationToken ct = default) =>
        _context.SaveChangesAsync(ct);

    public async Task DeleteAsync(Rating rating, CancellationToken ct = default)
    {
        _context.Ratings.Remove(rating);
        await _context.SaveChangesAsync(ct);
    }
}

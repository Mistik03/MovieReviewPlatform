using Microsoft.EntityFrameworkCore;
using MovieReview.Api.Data;
using MovieReview.Api.Domain.Entities;
using MovieReview.Api.Domain.Enums;
using MovieReview.Api.Domain.Queries;
using MovieReview.Api.Domain.ReadModels;
using MovieReview.Api.Repositories.Interfaces;

namespace MovieReview.Api.Repositories.Implementations;

public class TitleRepository : ITitleRepository
{
    private readonly AppDbContext _context;

    public TitleRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Title?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _context.Titles
            .Include(t => t.TitleGenres).ThenInclude(tg => tg.Genre)
            .FirstOrDefaultAsync(t => t.Id == id, ct);

    public Task<Title?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default) =>
        _context.Titles
            .Include(t => t.TitleGenres).ThenInclude(tg => tg.Genre)
            .Include(t => t.CastMembers.OrderBy(cm => cm.CastOrder)).ThenInclude(cm => cm.Person)
            .AsSplitQuery()
            .FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task<TitleWithStats?> GetStatsByIdAsync(int id, CancellationToken ct = default)
    {
        var result = await _context.Titles
            .Where(t => t.Id == id)
            .Select(t => new
            {
                Title = t,
                AverageRating = t.Ratings.Select(r => (double?)r.Score).Average(),
                ReviewCount = t.Reviews.Count,
                RatingCount = t.Ratings.Count
            })
            .FirstOrDefaultAsync(ct);

        return result is null
            ? null
            : new TitleWithStats(result.Title, result.AverageRating, result.ReviewCount, result.RatingCount);
    }

    public async Task<(IReadOnlyList<TitleWithStats> Items, int TotalCount)> GetPagedWithStatsAsync(
        TitleQueryParams query, CancellationToken ct = default)
    {
        var titles = _context.Titles
            .Include(t => t.TitleGenres).ThenInclude(tg => tg.Genre)
            .AsQueryable();

        if (query.MediaType.HasValue)
            titles = titles.Where(t => t.MediaType == query.MediaType.Value);

        if (query.GenreId.HasValue)
            titles = titles.Where(t => t.TitleGenres.Any(tg => tg.GenreId == query.GenreId.Value));

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            // Case-insensitive LIKE that translates on both PostgreSQL and SQLite (used in tests).
            var pattern = $"%{query.Search.Trim().ToLower()}%";
            titles = titles.Where(t => EF.Functions.Like(t.Name.ToLower(), pattern));
        }

        var totalCount = await titles.CountAsync(ct);

        var projected = titles.Select(t => new
        {
            Title = t,
            AverageRating = t.Ratings.Select(r => (double?)r.Score).Average(),
            ReviewCount = t.Reviews.Count,
            RatingCount = t.Ratings.Count
        });

        projected = query.Sort switch
        {
            "name" => projected.OrderBy(x => x.Title.Name),
            "year" => projected.OrderByDescending(x => x.Title.ReleaseYear).ThenBy(x => x.Title.Name),
            "rating" => projected.OrderByDescending(x => x.AverageRating ?? 0)
                                 .ThenByDescending(x => x.RatingCount),
            _ => projected.OrderByDescending(x => x.Title.CreatedAt)
        };

        var page = await projected
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(ct);

        var items = page
            .Select(x => new TitleWithStats(x.Title, x.AverageRating, x.ReviewCount, x.RatingCount))
            .ToList();

        return (items, totalCount);
    }

    public Task<Title?> GetByTmdbIdAsync(int tmdbId, MediaType mediaType, CancellationToken ct = default) =>
        _context.Titles.FirstOrDefaultAsync(t => t.TmdbId == tmdbId && t.MediaType == mediaType, ct);

    public Task<bool> ExistsByNameYearTypeAsync(
        string name, int releaseYear, MediaType mediaType, int? excludeId = null, CancellationToken ct = default) =>
        _context.Titles.AnyAsync(t =>
            t.Name == name &&
            t.ReleaseYear == releaseYear &&
            t.MediaType == mediaType &&
            (excludeId == null || t.Id != excludeId), ct);

    public Task<bool> HasReviewsOrRatingsAsync(int titleId, CancellationToken ct = default) =>
        _context.Titles
            .Where(t => t.Id == titleId)
            .AnyAsync(t => t.Reviews.Any() || t.Ratings.Any(), ct);

    public async Task<Title> AddAsync(Title title, CancellationToken ct = default)
    {
        _context.Titles.Add(title);
        await _context.SaveChangesAsync(ct);
        return title;
    }

    public Task UpdateAsync(Title title, CancellationToken ct = default) =>
        _context.SaveChangesAsync(ct);

    public async Task DeleteAsync(Title title, CancellationToken ct = default)
    {
        _context.Titles.Remove(title);
        await _context.SaveChangesAsync(ct);
    }
}

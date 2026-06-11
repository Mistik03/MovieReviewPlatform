using Microsoft.EntityFrameworkCore;
using MovieReview.Api.Data;
using MovieReview.Api.Domain.Entities;
using MovieReview.Api.Domain.ReadModels;
using MovieReview.Api.Repositories.Interfaces;

namespace MovieReview.Api.Repositories.Implementations;

public class GenreRepository : IGenreRepository
{
    private readonly AppDbContext _context;

    public GenreRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<GenreWithCount>> GetAllWithCountsAsync(CancellationToken ct = default)
    {
        var rows = await _context.Genres
            .OrderBy(g => g.Name)
            .Select(g => new { Genre = g, TitleCount = g.TitleGenres.Count })
            .ToListAsync(ct);

        return rows.Select(r => new GenreWithCount(r.Genre, r.TitleCount)).ToList();
    }

    public Task<Genre?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _context.Genres.FirstOrDefaultAsync(g => g.Id == id, ct);

    public Task<Genre?> GetByNameAsync(string name, CancellationToken ct = default) =>
        _context.Genres.FirstOrDefaultAsync(g => g.Name.ToLower() == name.ToLower(), ct);

    public async Task<IReadOnlyList<Genre>> GetByTmdbIdsAsync(IEnumerable<int> tmdbIds, CancellationToken ct = default)
    {
        var ids = tmdbIds.ToList();
        return await _context.Genres
            .Where(g => g.TmdbId != null && ids.Contains(g.TmdbId.Value))
            .ToListAsync(ct);
    }

    public Task<int> CountTitlesAsync(int genreId, CancellationToken ct = default) =>
        _context.TitleGenres.CountAsync(tg => tg.GenreId == genreId, ct);

    public async Task<Genre> AddAsync(Genre genre, CancellationToken ct = default)
    {
        _context.Genres.Add(genre);
        await _context.SaveChangesAsync(ct);
        return genre;
    }

    public async Task AddRangeAsync(IEnumerable<Genre> genres, CancellationToken ct = default)
    {
        _context.Genres.AddRange(genres);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Genre genre, CancellationToken ct = default)
    {
        _context.Genres.Remove(genre);
        await _context.SaveChangesAsync(ct);
    }
}

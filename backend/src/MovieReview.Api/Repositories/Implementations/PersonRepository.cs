using Microsoft.EntityFrameworkCore;
using MovieReview.Api.Data;
using MovieReview.Api.Domain.Entities;
using MovieReview.Api.Repositories.Interfaces;

namespace MovieReview.Api.Repositories.Implementations;

public class PersonRepository : IPersonRepository
{
    private readonly AppDbContext _context;

    public PersonRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Person?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _context.People.FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<IReadOnlyList<Person>> GetByTmdbIdsAsync(IEnumerable<int> tmdbIds, CancellationToken ct = default)
    {
        var ids = tmdbIds.ToList();
        return await _context.People
            .Where(p => p.TmdbId != null && ids.Contains(p.TmdbId.Value))
            .ToListAsync(ct);
    }

    public async Task AddRangeAsync(IEnumerable<Person> people, CancellationToken ct = default)
    {
        _context.People.AddRange(people);
        await _context.SaveChangesAsync(ct);
    }
}

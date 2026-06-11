using Microsoft.EntityFrameworkCore;
using MovieReview.Api.Data;
using MovieReview.Api.Domain.Entities;
using MovieReview.Api.Repositories.Interfaces;

namespace MovieReview.Api.Repositories.Implementations;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<User?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _context.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default) =>
        _context.Users.FirstOrDefaultAsync(u => u.Username == username, ct);

    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        _context.Users.FirstOrDefaultAsync(u => u.Email == email, ct);

    public Task<User?> GetProfileAsync(int id, CancellationToken ct = default) =>
        _context.Users
            .Include(u => u.Reviews).ThenInclude(r => r.Title)
            .Include(u => u.Ratings).ThenInclude(r => r.Title)
            .AsSplitQuery()
            .FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<User> AddAsync(User user, CancellationToken ct = default)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync(ct);
        return user;
    }
}

using Microsoft.EntityFrameworkCore;
using MovieReview.Api.Domain.Entities;

namespace MovieReview.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Title> Titles => Set<Title>();
    public DbSet<Genre> Genres => Set<Genre>();
    public DbSet<TitleGenre> TitleGenres => Set<TitleGenre>();
    public DbSet<Person> People => Set<Person>();
    public DbSet<CastMember> CastMembers => Set<CastMember>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Rating> Ratings => Set<Rating>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(u => u.Username).HasMaxLength(50).IsRequired();
            entity.Property(u => u.Email).HasMaxLength(254).IsRequired();
            entity.Property(u => u.PasswordHash).IsRequired();
            entity.Property(u => u.Role).HasMaxLength(20).IsRequired();
            entity.HasIndex(u => u.Username).IsUnique();
            entity.HasIndex(u => u.Email).IsUnique();
        });

        modelBuilder.Entity<Title>(entity =>
        {
            entity.Property(t => t.Name).HasMaxLength(300).IsRequired();
            entity.Property(t => t.Description).HasMaxLength(4000);
            entity.Property(t => t.Director).HasMaxLength(200);
            entity.Property(t => t.PosterUrl).HasMaxLength(500);
            entity.Property(t => t.BackdropUrl).HasMaxLength(500);
            // Stored as text so the database stays readable (Movie / TvShow).
            entity.Property(t => t.MediaType).HasConversion<string>().HasMaxLength(10);
            entity.HasIndex(t => new { t.TmdbId, t.MediaType }).IsUnique()
                  .HasFilter("\"TmdbId\" IS NOT NULL");
            entity.HasIndex(t => new { t.Name, t.ReleaseYear, t.MediaType }).IsUnique();
        });

        modelBuilder.Entity<Genre>(entity =>
        {
            entity.Property(g => g.Name).HasMaxLength(100).IsRequired();
            entity.HasIndex(g => g.Name).IsUnique();
        });

        modelBuilder.Entity<TitleGenre>(entity =>
        {
            entity.HasKey(tg => new { tg.TitleId, tg.GenreId });
            entity.HasOne(tg => tg.Title)
                  .WithMany(t => t.TitleGenres)
                  .HasForeignKey(tg => tg.TitleId)
                  .OnDelete(DeleteBehavior.Cascade);
            // Genre deletion is blocked by the service while titles reference it;
            // Restrict makes the database enforce the same invariant.
            entity.HasOne(tg => tg.Genre)
                  .WithMany(g => g.TitleGenres)
                  .HasForeignKey(tg => tg.GenreId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Person>(entity =>
        {
            entity.Property(p => p.Name).HasMaxLength(200).IsRequired();
            entity.Property(p => p.ProfileImageUrl).HasMaxLength(500);
            entity.HasIndex(p => p.TmdbId).IsUnique().HasFilter("\"TmdbId\" IS NOT NULL");
        });

        modelBuilder.Entity<CastMember>(entity =>
        {
            entity.HasKey(cm => new { cm.TitleId, cm.PersonId });
            entity.Property(cm => cm.CharacterName).HasMaxLength(300);
            entity.HasOne(cm => cm.Title)
                  .WithMany(t => t.CastMembers)
                  .HasForeignKey(cm => cm.TitleId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(cm => cm.Person)
                  .WithMany(p => p.CastMembers)
                  .HasForeignKey(cm => cm.PersonId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.Property(r => r.Content).HasMaxLength(5000).IsRequired();
            // One review per user per title — also checked in the service layer (409).
            entity.HasIndex(r => new { r.UserId, r.TitleId }).IsUnique();
            entity.HasOne(r => r.User)
                  .WithMany(u => u.Reviews)
                  .HasForeignKey(r => r.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            // Title deletion is blocked by the service while reviews exist.
            entity.HasOne(r => r.Title)
                  .WithMany(t => t.Reviews)
                  .HasForeignKey(r => r.TitleId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Rating>(entity =>
        {
            // One rating per user per title — also checked in the service layer (409).
            entity.HasIndex(r => new { r.UserId, r.TitleId }).IsUnique();
            entity.ToTable(t => t.HasCheckConstraint("CK_Rating_Score", "\"Score\" BETWEEN 1 AND 10"));
            entity.HasOne(r => r.User)
                  .WithMany(u => u.Ratings)
                  .HasForeignKey(r => r.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(r => r.Title)
                  .WithMany(t => t.Ratings)
                  .HasForeignKey(r => r.TitleId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}

using MovieReview.Api.Domain.Enums;

namespace MovieReview.Api.Domain.Entities;

/// <summary>A catalog entry — either a movie or a TV show.</summary>
public class Title
{
    public int Id { get; set; }

    /// <summary>TMDB identifier; null for manually created titles.</summary>
    public int? TmdbId { get; set; }

    public MediaType MediaType { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public int ReleaseYear { get; set; }

    /// <summary>Director for movies, creator for TV shows.</summary>
    public string? Director { get; set; }

    /// <summary>Runtime in minutes (per-episode for TV shows).</summary>
    public int? RuntimeMinutes { get; set; }

    public string? PosterUrl { get; set; }
    public string? BackdropUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<TitleGenre> TitleGenres { get; set; } = new List<TitleGenre>();
    public ICollection<CastMember> CastMembers { get; set; } = new List<CastMember>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
}

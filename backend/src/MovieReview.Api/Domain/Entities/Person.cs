namespace MovieReview.Api.Domain.Entities;

/// <summary>An actor appearing in the cast of one or more titles.</summary>
public class Person
{
    public int Id { get; set; }

    /// <summary>TMDB person identifier; null for manually created people.</summary>
    public int? TmdbId { get; set; }

    public string Name { get; set; } = null!;
    public string? ProfileImageUrl { get; set; }

    public ICollection<CastMember> CastMembers { get; set; } = new List<CastMember>();
}

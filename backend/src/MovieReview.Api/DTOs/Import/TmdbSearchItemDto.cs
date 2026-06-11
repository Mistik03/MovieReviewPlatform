using MovieReview.Api.Domain.Enums;

namespace MovieReview.Api.DTOs.Import;

/// <summary>A TMDB search hit shown in the admin import screen.</summary>
public class TmdbSearchItemDto
{
    public int TmdbId { get; set; }
    public MediaType MediaType { get; set; }
    public string Name { get; set; } = null!;
    public int? ReleaseYear { get; set; }
    public string? Overview { get; set; }
    public string? PosterUrl { get; set; }
    public double VoteAverage { get; set; }

    /// <summary>True when this TMDB entry already exists in the local catalog.</summary>
    public bool AlreadyImported { get; set; }

    /// <summary>Local catalog id when AlreadyImported is true.</summary>
    public int? TitleId { get; set; }
}

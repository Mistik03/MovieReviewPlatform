using MovieReview.Api.Domain.Enums;
using MovieReview.Api.DTOs.Genres;

namespace MovieReview.Api.DTOs.Titles;

/// <summary>Catalog list item. AverageRating and the counts are computed — never stored.</summary>
public class TitleResponseDto
{
    public int Id { get; set; }
    public MediaType MediaType { get; set; }
    public string Name { get; set; } = null!;
    public int ReleaseYear { get; set; }
    public string? PosterUrl { get; set; }
    public string? BackdropUrl { get; set; }
    public double? AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public int RatingCount { get; set; }
    public List<GenreResponseDto> Genres { get; set; } = [];
}

using System.ComponentModel.DataAnnotations;
using MovieReview.Api.Domain.Enums;

namespace MovieReview.Api.DTOs.Titles;

public class TitleCreateDto
{
    [Required, MaxLength(300)]
    public string Name { get; set; } = null!;

    [Required]
    public MediaType MediaType { get; set; }

    [MaxLength(4000)]
    public string? Description { get; set; }

    [Range(1870, 2100)]
    public int ReleaseYear { get; set; }

    [MaxLength(200)]
    public string? Director { get; set; }

    [Range(1, 1000)]
    public int? RuntimeMinutes { get; set; }

    [Url, MaxLength(500)]
    public string? PosterUrl { get; set; }

    [Url, MaxLength(500)]
    public string? BackdropUrl { get; set; }

    public List<int> GenreIds { get; set; } = [];
}

using System.ComponentModel.DataAnnotations;

namespace MovieReview.Api.DTOs.Genres;

public class GenreCreateDto
{
    [Required, MinLength(2), MaxLength(100)]
    public string Name { get; set; } = null!;
}

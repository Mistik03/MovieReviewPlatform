using System.ComponentModel.DataAnnotations;

namespace MovieReview.Api.DTOs.Reviews;

public class ReviewCreateDto
{
    [Required]
    public int TitleId { get; set; }

    [Required, MinLength(10), MaxLength(5000)]
    public string Content { get; set; } = null!;
}

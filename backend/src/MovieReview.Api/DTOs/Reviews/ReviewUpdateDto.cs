using System.ComponentModel.DataAnnotations;

namespace MovieReview.Api.DTOs.Reviews;

public class ReviewUpdateDto
{
    [Required, MinLength(10), MaxLength(5000)]
    public string Content { get; set; } = null!;
}

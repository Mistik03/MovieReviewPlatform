using System.ComponentModel.DataAnnotations;

namespace MovieReview.Api.DTOs.Ratings;

public class RatingCreateDto
{
    [Required]
    public int TitleId { get; set; }

    [Required, Range(1, 10)]
    public int Score { get; set; }
}

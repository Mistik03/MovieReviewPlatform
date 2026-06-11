using System.ComponentModel.DataAnnotations;

namespace MovieReview.Api.DTOs.Ratings;

public class RatingUpdateDto
{
    [Required, Range(1, 10)]
    public int Score { get; set; }
}

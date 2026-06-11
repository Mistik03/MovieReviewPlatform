namespace MovieReview.Api.DTOs.Ratings;

public class RatingResponseDto
{
    public int Id { get; set; }
    public int Score { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; } = null!;
    public int TitleId { get; set; }
    public string TitleName { get; set; } = null!;
    public string? TitlePosterUrl { get; set; }
}

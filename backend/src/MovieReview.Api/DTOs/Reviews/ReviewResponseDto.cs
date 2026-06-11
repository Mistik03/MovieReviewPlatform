namespace MovieReview.Api.DTOs.Reviews;

public class ReviewResponseDto
{
    public int Id { get; set; }
    public string Content { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; } = null!;
    public int TitleId { get; set; }
    public string TitleName { get; set; } = null!;
    public string? TitlePosterUrl { get; set; }
}

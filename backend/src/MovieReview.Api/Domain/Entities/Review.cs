namespace MovieReview.Api.Domain.Entities;

public class Review
{
    public int Id { get; set; }
    public string Content { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int TitleId { get; set; }
    public Title Title { get; set; } = null!;
}

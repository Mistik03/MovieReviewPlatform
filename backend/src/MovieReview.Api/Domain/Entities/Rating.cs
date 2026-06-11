namespace MovieReview.Api.Domain.Entities;

public class Rating
{
    public int Id { get; set; }

    /// <summary>Score from 1 to 10 (inclusive) — range enforced in the service layer and by a DB check constraint.</summary>
    public int Score { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int TitleId { get; set; }
    public Title Title { get; set; } = null!;
}

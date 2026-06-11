using MovieReview.Api.DTOs.Ratings;
using MovieReview.Api.DTOs.Reviews;

namespace MovieReview.Api.DTOs.Users;

/// <summary>The authenticated user's profile. PasswordHash is never exposed by any DTO.</summary>
public class UserProfileDto
{
    public int Id { get; set; }
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Role { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public int ReviewCount { get; set; }
    public int RatingCount { get; set; }
    public List<ReviewResponseDto> Reviews { get; set; } = [];
    public List<RatingResponseDto> Ratings { get; set; } = [];
}

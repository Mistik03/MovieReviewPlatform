namespace MovieReview.Api.DTOs.Auth;

public class AuthResponseDto
{
    public string Token { get; set; } = null!;
    public DateTime ExpiresAtUtc { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Role { get; set; } = null!;
}

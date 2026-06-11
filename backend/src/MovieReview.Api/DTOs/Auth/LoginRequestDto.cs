using System.ComponentModel.DataAnnotations;

namespace MovieReview.Api.DTOs.Auth;

public class LoginRequestDto
{
    /// <summary>Username or email address.</summary>
    [Required, MaxLength(254)]
    public string UsernameOrEmail { get; set; } = null!;

    [Required, MaxLength(128)]
    public string Password { get; set; } = null!;
}

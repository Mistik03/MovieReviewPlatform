using System.ComponentModel.DataAnnotations;

namespace MovieReview.Api.DTOs.Auth;

public class RegisterRequestDto
{
    [Required, MinLength(3), MaxLength(50)]
    [RegularExpression("^[a-zA-Z0-9_]+$", ErrorMessage = "Username may only contain letters, digits and underscores.")]
    public string Username { get; set; } = null!;

    [Required, EmailAddress, MaxLength(254)]
    public string Email { get; set; } = null!;

    [Required, MinLength(8), MaxLength(128)]
    public string Password { get; set; } = null!;
}

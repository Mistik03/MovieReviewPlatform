using System.Security.Claims;
using MovieReview.Api.Domain.Enums;
using MovieReview.Api.Domain.Exceptions;

namespace MovieReview.Api.Auth;

public static class ClaimsPrincipalExtensions
{
    /// <summary>Reads the authenticated user's id from the token (sub claim).</summary>
    public static int GetUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? principal.FindFirstValue("sub");

        if (!int.TryParse(value, out var id))
            throw new UnauthorizedException("The token does not contain a valid user id.");

        return id;
    }

    public static bool IsAdmin(this ClaimsPrincipal principal) =>
        principal.IsInRole(Roles.Admin);
}

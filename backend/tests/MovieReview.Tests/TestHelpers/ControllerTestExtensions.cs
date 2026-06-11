using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MovieReview.Tests.TestHelpers;

public static class ControllerTestExtensions
{
    /// <summary>Attaches an authenticated principal to the controller, mirroring what the JWT middleware produces.</summary>
    public static T WithUser<T>(this T controller, int userId, string role) where T : ControllerBase
    {
        var identity = new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Role, role)
        ], authenticationType: "TestAuth");

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };
        return controller;
    }
}

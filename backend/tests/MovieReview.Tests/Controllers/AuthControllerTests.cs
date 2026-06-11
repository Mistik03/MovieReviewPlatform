using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MovieReview.Api.Controllers;
using MovieReview.Api.DTOs.Auth;
using MovieReview.Api.Services.Interfaces;

namespace MovieReview.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _auth = new();
    private readonly AuthController _sut;

    public AuthControllerTests()
    {
        _sut = new AuthController(_auth.Object);
    }

    [Fact]
    public async Task Register_Returns201_WithAuthResponse()
    {
        var request = new RegisterRequestDto { Username = "arb", Email = "arb@example.com", Password = "Password1!" };
        var response = new AuthResponseDto { Token = "jwt", Username = "arb", Email = "arb@example.com", Role = "User" };
        _auth.Setup(s => s.RegisterAsync(request, It.IsAny<CancellationToken>())).ReturnsAsync(response);

        var result = await _sut.Register(request, CancellationToken.None);

        var objectResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status201Created);
        objectResult.Value.Should().Be(response);
    }

    [Fact]
    public async Task Login_Returns200_WithAuthResponse()
    {
        var request = new LoginRequestDto { UsernameOrEmail = "arb", Password = "Password1!" };
        var response = new AuthResponseDto { Token = "jwt", Username = "arb", Email = "arb@example.com", Role = "User" };
        _auth.Setup(s => s.LoginAsync(request, It.IsAny<CancellationToken>())).ReturnsAsync(response);

        var result = await _sut.Login(request, CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(response);
    }
}

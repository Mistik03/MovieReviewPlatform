using FluentAssertions;
using Moq;
using MovieReview.Api.Auth;
using MovieReview.Api.Domain.Entities;
using MovieReview.Api.Domain.Enums;
using MovieReview.Api.Domain.Exceptions;
using MovieReview.Api.DTOs.Auth;
using MovieReview.Api.Repositories.Interfaces;
using MovieReview.Api.Services.Implementations;
using MovieReview.Tests.TestHelpers;

namespace MovieReview.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IJwtTokenService> _tokens = new();
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _tokens.Setup(t => t.CreateToken(It.IsAny<User>()))
               .Returns(("jwt-token", DateTime.UtcNow.AddHours(2)));
        _sut = new AuthService(_users.Object, _tokens.Object);
    }

    private static RegisterRequestDto RegisterRequest() => new()
    {
        Username = "arb",
        Email = "arb@example.com",
        Password = "SuperSecret1!"
    };

    [Fact]
    public async Task Register_WithTakenUsername_ThrowsConflict()
    {
        _users.Setup(r => r.GetByUsernameAsync("arb", It.IsAny<CancellationToken>()))
              .ReturnsAsync(TestData.User());

        var act = () => _sut.RegisterAsync(RegisterRequest());

        await act.Should().ThrowAsync<ConflictException>().WithMessage("*Username*");
    }

    [Fact]
    public async Task Register_WithTakenEmail_ThrowsConflict()
    {
        _users.Setup(r => r.GetByEmailAsync("arb@example.com", It.IsAny<CancellationToken>()))
              .ReturnsAsync(TestData.User());

        var act = () => _sut.RegisterAsync(RegisterRequest());

        await act.Should().ThrowAsync<ConflictException>().WithMessage("*email*");
    }

    [Fact]
    public async Task Register_HashesPassword_AndAssignsUserRole()
    {
        User? saved = null;
        _users.Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
              .Callback<User, CancellationToken>((u, _) => saved = u)
              .ReturnsAsync((User u, CancellationToken _) => u);

        var response = await _sut.RegisterAsync(RegisterRequest());

        saved.Should().NotBeNull();
        saved!.Role.Should().Be(Roles.User);
        saved.PasswordHash.Should().NotBe("SuperSecret1!");
        BCrypt.Net.BCrypt.Verify("SuperSecret1!", saved.PasswordHash).Should().BeTrue();
        response.Token.Should().Be("jwt-token");
    }

    [Fact]
    public async Task Login_WithUnknownUser_ThrowsUnauthorized()
    {
        var act = () => _sut.LoginAsync(new LoginRequestDto { UsernameOrEmail = "ghost", Password = "x" });

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Login_WithWrongPassword_ThrowsUnauthorized()
    {
        var user = TestData.User();
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword("correct");
        _users.Setup(r => r.GetByUsernameAsync("arb", It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var act = () => _sut.LoginAsync(new LoginRequestDto { UsernameOrEmail = "arb", Password = "wrong" });

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Login_WithEmail_LooksUpByEmail()
    {
        var user = TestData.User();
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword("correct");
        _users.Setup(r => r.GetByEmailAsync("arb@example.com", It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var response = await _sut.LoginAsync(new LoginRequestDto
        {
            UsernameOrEmail = "arb@example.com",
            Password = "correct"
        });

        response.Username.Should().Be("arb");
        response.Token.Should().Be("jwt-token");
        _users.Verify(r => r.GetByUsernameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}

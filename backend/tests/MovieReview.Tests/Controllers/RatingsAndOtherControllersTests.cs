using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MovieReview.Api.Controllers;
using MovieReview.Api.Domain.Enums;
using MovieReview.Api.DTOs.Genres;
using MovieReview.Api.DTOs.Import;
using MovieReview.Api.DTOs.Ratings;
using MovieReview.Api.DTOs.Titles;
using MovieReview.Api.DTOs.Users;
using MovieReview.Api.Services.Interfaces;
using MovieReview.Tests.TestHelpers;

namespace MovieReview.Tests.Controllers;

public class RatingsControllerTests
{
    private readonly Mock<IRatingService> _ratings = new();
    private readonly RatingsController _sut;

    public RatingsControllerTests()
    {
        _sut = new RatingsController(_ratings.Object).WithUser(userId: 42, role: Roles.User);
    }

    [Fact]
    public async Task Create_PassesAuthenticatedUserId_AndReturns201()
    {
        var dto = new RatingCreateDto { TitleId = 1, Score = 9 };
        _ratings.Setup(s => s.CreateAsync(42, dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RatingResponseDto { Id = 5, Score = 9 });

        var result = await _sut.Create(dto, CancellationToken.None);

        result.Result.Should().BeOfType<ObjectResult>()
              .Which.StatusCode.Should().Be(StatusCodes.Status201Created);
        _ratings.Verify(s => s.CreateAsync(42, dto, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_Returns204_WithCallerIdentity()
    {
        var result = await _sut.Delete(5, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
        _ratings.Verify(s => s.DeleteAsync(5, 42, false, It.IsAny<CancellationToken>()), Times.Once);
    }
}

public class GenresControllerTests
{
    private readonly Mock<IGenreService> _genres = new();
    private readonly GenresController _sut;

    public GenresControllerTests()
    {
        _sut = new GenresController(_genres.Object);
    }

    [Fact]
    public async Task GetAll_Returns200()
    {
        _genres.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
               .ReturnsAsync([new GenreResponseDto { Id = 1, Name = "Action" }]);

        var result = await _sut.GetAll(CancellationToken.None);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Create_Returns201_PointingAtGetById()
    {
        _genres.Setup(s => s.CreateAsync(It.IsAny<GenreCreateDto>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(new GenreResponseDto { Id = 3, Name = "Horror" });

        var result = await _sut.Create(new GenreCreateDto { Name = "Horror" }, CancellationToken.None);

        var created = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.RouteValues!["id"].Should().Be(3);
    }

    [Fact]
    public async Task Delete_Returns204()
    {
        var result = await _sut.Delete(3, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
        _genres.Verify(s => s.DeleteAsync(3, It.IsAny<CancellationToken>()), Times.Once);
    }
}

public class UsersControllerTests
{
    [Fact]
    public async Task Me_ReturnsProfileForAuthenticatedUser()
    {
        var users = new Mock<IUserService>();
        users.Setup(s => s.GetProfileAsync(42, It.IsAny<CancellationToken>()))
             .ReturnsAsync(new UserProfileDto { Id = 42, Username = "arb" });
        var sut = new UsersController(users.Object).WithUser(userId: 42, role: Roles.User);

        var result = await sut.Me(CancellationToken.None);

        result.Result.Should().BeOfType<OkObjectResult>()
              .Which.Value.Should().BeOfType<UserProfileDto>()
              .Which.Username.Should().Be("arb");
    }
}

public class ImportControllerTests
{
    private readonly Mock<ITmdbImportService> _import = new();
    private readonly ImportController _sut;

    public ImportControllerTests()
    {
        _sut = new ImportController(_import.Object).WithUser(userId: 1, role: Roles.Admin);
    }

    [Fact]
    public async Task Search_Returns200()
    {
        _import.Setup(s => s.SearchAsync("dune", It.IsAny<CancellationToken>()))
               .ReturnsAsync([new TmdbSearchItemDto { TmdbId = 1, Name = "Dune" }]);

        var result = await _sut.Search("dune", CancellationToken.None);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Import_Returns201_PointingAtTitlesGetById()
    {
        _import.Setup(s => s.ImportAsync(It.IsAny<TmdbImportRequestDto>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(new TitleDetailDto { Id = 9, Name = "Dune" });

        var result = await _sut.Import(new TmdbImportRequestDto { TmdbId = 438631, MediaType = MediaType.Movie }, CancellationToken.None);

        var created = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ControllerName.Should().Be("Titles");
        created.RouteValues!["id"].Should().Be(9);
    }
}

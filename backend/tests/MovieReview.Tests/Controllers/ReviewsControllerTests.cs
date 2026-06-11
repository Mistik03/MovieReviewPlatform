using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MovieReview.Api.Controllers;
using MovieReview.Api.Domain.Enums;
using MovieReview.Api.DTOs.Reviews;
using MovieReview.Api.Services.Interfaces;
using MovieReview.Tests.TestHelpers;

namespace MovieReview.Tests.Controllers;

public class ReviewsControllerTests
{
    private readonly Mock<IReviewService> _reviews = new();
    private readonly ReviewsController _sut;

    public ReviewsControllerTests()
    {
        _sut = new ReviewsController(_reviews.Object).WithUser(userId: 42, role: Roles.User);
    }

    [Fact]
    public async Task GetByTitle_Returns200()
    {
        _reviews.Setup(s => s.GetByTitleAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync([new ReviewResponseDto { Id = 1, Content = "Great." }]);

        var result = await _sut.GetByTitle(1, CancellationToken.None);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Create_PassesAuthenticatedUserId_AndReturns201()
    {
        var dto = new ReviewCreateDto { TitleId = 1, Content = "A wonderful experience." };
        _reviews.Setup(s => s.CreateAsync(42, dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ReviewResponseDto { Id = 10 });

        var result = await _sut.Create(dto, CancellationToken.None);

        result.Result.Should().BeOfType<CreatedAtActionResult>();
        _reviews.Verify(s => s.CreateAsync(42, dto, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_PassesCallerIdentity_NonAdmin()
    {
        var dto = new ReviewUpdateDto { Content = "Updated content here." };
        _reviews.Setup(s => s.UpdateAsync(10, 42, false, dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ReviewResponseDto { Id = 10 });

        await _sut.Update(10, dto, CancellationToken.None);

        _reviews.Verify(s => s.UpdateAsync(10, 42, false, dto, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_AsAdmin_PassesAdminFlag()
    {
        var admin = new ReviewsController(_reviews.Object).WithUser(userId: 1, role: Roles.Admin);
        var dto = new ReviewUpdateDto { Content = "Moderated content." };
        _reviews.Setup(s => s.UpdateAsync(10, 1, true, dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ReviewResponseDto { Id = 10 });

        await admin.Update(10, dto, CancellationToken.None);

        _reviews.Verify(s => s.UpdateAsync(10, 1, true, dto, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_Returns204()
    {
        var result = await _sut.Delete(10, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
        _reviews.Verify(s => s.DeleteAsync(10, 42, false, It.IsAny<CancellationToken>()), Times.Once);
    }
}

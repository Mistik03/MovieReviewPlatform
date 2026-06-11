using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MovieReview.Api.Controllers;
using MovieReview.Api.Domain.Queries;
using MovieReview.Api.DTOs.Common;
using MovieReview.Api.DTOs.Titles;
using MovieReview.Api.Services.Interfaces;

namespace MovieReview.Tests.Controllers;

public class TitlesControllerTests
{
    private readonly Mock<ITitleService> _titles = new();
    private readonly TitlesController _sut;

    public TitlesControllerTests()
    {
        _sut = new TitlesController(_titles.Object);
    }

    [Fact]
    public async Task GetAll_Returns200_WithPagedResult()
    {
        var paged = new PagedResultDto<TitleResponseDto> { Items = [new TitleResponseDto { Id = 1 }], TotalCount = 1, Page = 1, PageSize = 20 };
        _titles.Setup(s => s.GetPagedAsync(It.IsAny<TitleQueryParams>(), It.IsAny<CancellationToken>())).ReturnsAsync(paged);

        var result = await _sut.GetAll(new TitleQueryParams(), CancellationToken.None);

        result.Result.Should().BeOfType<OkObjectResult>().Which.Value.Should().Be(paged);
    }

    [Fact]
    public async Task GetById_Returns200_WithDetail()
    {
        var detail = new TitleDetailDto { Id = 5, Name = "Inception" };
        _titles.Setup(s => s.GetByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(detail);

        var result = await _sut.GetById(5, CancellationToken.None);

        result.Result.Should().BeOfType<OkObjectResult>().Which.Value.Should().Be(detail);
    }

    [Fact]
    public async Task Create_Returns201_PointingAtGetById()
    {
        var detail = new TitleDetailDto { Id = 7, Name = "New Movie" };
        _titles.Setup(s => s.CreateAsync(It.IsAny<TitleCreateDto>(), It.IsAny<CancellationToken>())).ReturnsAsync(detail);

        var result = await _sut.Create(new TitleCreateDto { Name = "New Movie", ReleaseYear = 2024 }, CancellationToken.None);

        var created = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(TitlesController.GetById));
        created.RouteValues!["id"].Should().Be(7);
        created.Value.Should().Be(detail);
    }

    [Fact]
    public async Task Update_Returns200_WithUpdatedDetail()
    {
        var detail = new TitleDetailDto { Id = 7, Name = "Renamed" };
        _titles.Setup(s => s.UpdateAsync(7, It.IsAny<TitleUpdateDto>(), It.IsAny<CancellationToken>())).ReturnsAsync(detail);

        var result = await _sut.Update(7, new TitleUpdateDto { Name = "Renamed", ReleaseYear = 2024 }, CancellationToken.None);

        result.Result.Should().BeOfType<OkObjectResult>().Which.Value.Should().Be(detail);
    }

    [Fact]
    public async Task Delete_Returns204_AndDelegatesToService()
    {
        var result = await _sut.Delete(7, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
        _titles.Verify(s => s.DeleteAsync(7, It.IsAny<CancellationToken>()), Times.Once);
    }
}

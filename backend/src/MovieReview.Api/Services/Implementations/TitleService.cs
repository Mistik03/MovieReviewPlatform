using MovieReview.Api.Domain.Entities;
using MovieReview.Api.Domain.Exceptions;
using MovieReview.Api.Domain.Queries;
using MovieReview.Api.Domain.ReadModels;
using MovieReview.Api.DTOs.Common;
using MovieReview.Api.DTOs.Titles;
using MovieReview.Api.Repositories.Interfaces;
using MovieReview.Api.Services.Interfaces;
using MovieReview.Api.Services.Mapping;

namespace MovieReview.Api.Services.Implementations;

public class TitleService : ITitleService
{
    private static readonly HashSet<string> AllowedSorts = ["newest", "name", "year", "rating"];

    private readonly ITitleRepository _titles;
    private readonly IGenreRepository _genres;

    public TitleService(ITitleRepository titles, IGenreRepository genres)
    {
        _titles = titles;
        _genres = genres;
    }

    public async Task<PagedResultDto<TitleResponseDto>> GetPagedAsync(TitleQueryParams query, CancellationToken ct = default)
    {
        if (!AllowedSorts.Contains(query.Sort))
            throw new BadRequestException($"Invalid sort '{query.Sort}'. Allowed values: {string.Join(", ", AllowedSorts)}.");

        if (query.Page < 1)
            throw new BadRequestException("Page must be 1 or greater.");

        var (items, totalCount) = await _titles.GetPagedWithStatsAsync(query, ct);

        return new PagedResultDto<TitleResponseDto>
        {
            Items = items.Select(DtoMapper.ToResponse).ToList(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<TitleDetailDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var title = await _titles.GetByIdWithDetailsAsync(id, ct)
            ?? throw new NotFoundException($"Title with id {id} was not found.");

        var stats = await _titles.GetStatsByIdAsync(id, ct)
            ?? new TitleWithStats(title, null, 0, 0);

        return DtoMapper.ToDetail(title, stats);
    }

    public async Task<TitleDetailDto> CreateAsync(TitleCreateDto dto, CancellationToken ct = default)
    {
        // Trim before the uniqueness check — the stored value is trimmed, so the
        // comparison must be too, or "Dune " would slip past and hit the DB index.
        var name = dto.Name.Trim();

        // Business rule: title name must be unique per release year and media type.
        if (await _titles.ExistsByNameYearTypeAsync(name, dto.ReleaseYear, dto.MediaType, null, ct))
            throw new ConflictException($"A {dto.MediaType} named '{name}' ({dto.ReleaseYear}) already exists.");

        var title = new Title
        {
            Name = name,
            MediaType = dto.MediaType,
            Description = dto.Description,
            ReleaseYear = dto.ReleaseYear,
            Director = dto.Director,
            RuntimeMinutes = dto.RuntimeMinutes,
            PosterUrl = dto.PosterUrl,
            BackdropUrl = dto.BackdropUrl,
            TitleGenres = await BuildGenreLinksAsync(dto.GenreIds, ct)
        };

        await _titles.AddAsync(title, ct);
        return await GetByIdAsync(title.Id, ct);
    }

    public async Task<TitleDetailDto> UpdateAsync(int id, TitleUpdateDto dto, CancellationToken ct = default)
    {
        var title = await _titles.GetByIdWithDetailsAsync(id, ct)
            ?? throw new NotFoundException($"Title with id {id} was not found.");

        var name = dto.Name.Trim();
        if (await _titles.ExistsByNameYearTypeAsync(name, dto.ReleaseYear, title.MediaType, id, ct))
            throw new ConflictException($"A {title.MediaType} named '{name}' ({dto.ReleaseYear}) already exists.");

        title.Name = name;
        title.Description = dto.Description;
        title.ReleaseYear = dto.ReleaseYear;
        title.Director = dto.Director;
        title.RuntimeMinutes = dto.RuntimeMinutes;
        title.PosterUrl = dto.PosterUrl;
        title.BackdropUrl = dto.BackdropUrl;

        title.TitleGenres.Clear();
        foreach (var link in await BuildGenreLinksAsync(dto.GenreIds, ct))
            title.TitleGenres.Add(link);

        await _titles.UpdateAsync(title, ct);
        return await GetByIdAsync(id, ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var title = await _titles.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Title with id {id} was not found.");

        // Business rule: a title with reviews or ratings cannot be removed.
        if (await _titles.HasReviewsOrRatingsAsync(id, ct))
            throw new ConflictException("This title still has reviews or ratings and cannot be deleted.");

        await _titles.DeleteAsync(title, ct);
    }

    private async Task<List<TitleGenre>> BuildGenreLinksAsync(List<int> genreIds, CancellationToken ct)
    {
        var links = new List<TitleGenre>();
        foreach (var genreId in genreIds.Distinct())
        {
            _ = await _genres.GetByIdAsync(genreId, ct)
                ?? throw new BadRequestException($"Genre with id {genreId} does not exist.");
            links.Add(new TitleGenre { GenreId = genreId });
        }
        return links;
    }
}

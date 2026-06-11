using MovieReview.Api.Domain.Entities;
using MovieReview.Api.Domain.ReadModels;
using MovieReview.Api.DTOs.Genres;
using MovieReview.Api.DTOs.Ratings;
using MovieReview.Api.DTOs.Reviews;
using MovieReview.Api.DTOs.Titles;

namespace MovieReview.Api.Services.Mapping;

/// <summary>
/// Manual entity-to-DTO mapping, kept in one place so every service returns identical shapes.
/// Entities never leave the service layer.
/// </summary>
public static class DtoMapper
{
    public static GenreResponseDto ToResponse(Genre genre, int titleCount) => new()
    {
        Id = genre.Id,
        Name = genre.Name,
        TitleCount = titleCount
    };

    public static GenreResponseDto ToResponse(GenreWithCount row) => ToResponse(row.Genre, row.TitleCount);

    public static TitleResponseDto ToResponse(TitleWithStats row)
    {
        var dto = new TitleResponseDto();
        FillResponse(dto, row);
        return dto;
    }

    public static TitleDetailDto ToDetail(Title title, TitleWithStats stats)
    {
        var dto = new TitleDetailDto
        {
            TmdbId = title.TmdbId,
            Description = title.Description,
            Director = title.Director,
            RuntimeMinutes = title.RuntimeMinutes,
            CreatedAt = title.CreatedAt,
            Cast = title.CastMembers
                .OrderBy(cm => cm.CastOrder)
                .Select(cm => new CastMemberDto
                {
                    PersonId = cm.PersonId,
                    Name = cm.Person.Name,
                    CharacterName = cm.CharacterName,
                    ProfileImageUrl = cm.Person.ProfileImageUrl,
                    CastOrder = cm.CastOrder
                })
                .ToList()
        };
        FillResponse(dto, stats with { Title = title });
        return dto;
    }

    private static void FillResponse(TitleResponseDto dto, TitleWithStats row)
    {
        var title = row.Title;
        dto.Id = title.Id;
        dto.MediaType = title.MediaType;
        dto.Name = title.Name;
        dto.ReleaseYear = title.ReleaseYear;
        dto.PosterUrl = title.PosterUrl;
        dto.BackdropUrl = title.BackdropUrl;
        dto.AverageRating = row.AverageRating is null ? null : Math.Round(row.AverageRating.Value, 1);
        dto.ReviewCount = row.ReviewCount;
        dto.RatingCount = row.RatingCount;
        dto.Genres = title.TitleGenres
            .Select(tg => new GenreResponseDto { Id = tg.GenreId, Name = tg.Genre.Name })
            .OrderBy(g => g.Name)
            .ToList();
    }

    public static ReviewResponseDto ToResponse(Review review, string? username = null) => new()
    {
        Id = review.Id,
        Content = review.Content,
        CreatedAt = review.CreatedAt,
        UpdatedAt = review.UpdatedAt,
        UserId = review.UserId,
        Username = username ?? review.User.Username,
        TitleId = review.TitleId,
        TitleName = review.Title.Name,
        TitlePosterUrl = review.Title.PosterUrl
    };

    public static RatingResponseDto ToResponse(Rating rating, string? username = null) => new()
    {
        Id = rating.Id,
        Score = rating.Score,
        CreatedAt = rating.CreatedAt,
        UpdatedAt = rating.UpdatedAt,
        UserId = rating.UserId,
        Username = username ?? rating.User.Username,
        TitleId = rating.TitleId,
        TitleName = rating.Title.Name,
        TitlePosterUrl = rating.Title.PosterUrl
    };
}

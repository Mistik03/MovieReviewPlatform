using MovieReview.Api.Domain.Entities;

namespace MovieReview.Api.Domain.ReadModels;

/// <summary>A genre together with the number of titles assigned to it.</summary>
public record GenreWithCount(Genre Genre, int TitleCount);

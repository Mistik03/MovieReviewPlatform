using MovieReview.Api.Domain.Entities;

namespace MovieReview.Api.Domain.ReadModels;

/// <summary>A title together with aggregates computed by the database — never stored.</summary>
public record TitleWithStats(Title Title, double? AverageRating, int ReviewCount, int RatingCount);

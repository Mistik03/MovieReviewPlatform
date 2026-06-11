using MovieReview.Api.Domain.Enums;

namespace MovieReview.Api.Domain.Queries;

/// <summary>Filtering, sorting and paging options for the title catalog.</summary>
public class TitleQueryParams
{
    public const int MaxPageSize = 50;

    public MediaType? MediaType { get; set; }
    public int? GenreId { get; set; }
    public string? Search { get; set; }

    /// <summary>One of: newest (default), name, year, rating.</summary>
    public string Sort { get; set; } = "newest";

    public int Page { get; set; } = 1;

    private int _pageSize = 20;
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = Math.Clamp(value, 1, MaxPageSize);
    }
}

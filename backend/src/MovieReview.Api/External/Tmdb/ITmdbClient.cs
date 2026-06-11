namespace MovieReview.Api.External.Tmdb;

/// <summary>Thin typed HTTP client for the TMDB v3 API.</summary>
public interface ITmdbClient
{
    Task<TmdbSearchResponse> SearchMultiAsync(string query, CancellationToken ct = default);
    Task<TmdbMovieDetails?> GetMovieWithCreditsAsync(int tmdbId, CancellationToken ct = default);
    Task<TmdbTvDetails?> GetTvShowWithCreditsAsync(int tmdbId, CancellationToken ct = default);

    /// <summary>Builds a full image URL from a TMDB image path, or null.</summary>
    string? BuildImageUrl(string? path, string size);
}

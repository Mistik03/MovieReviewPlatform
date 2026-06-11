namespace MovieReview.Api.External.Tmdb;

public class TmdbSettings
{
    public const string SectionName = "Tmdb";

    public string BaseUrl { get; set; } = "https://api.themoviedb.org/3/";
    public string ImageBaseUrl { get; set; } = "https://image.tmdb.org/t/p/";

    /// <summary>Either a TMDB v3 API key or a v4 read access token (JWT) — both are supported.</summary>
    public string ApiKey { get; set; } = "";
}

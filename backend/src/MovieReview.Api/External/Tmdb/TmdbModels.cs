using System.Text.Json.Serialization;

namespace MovieReview.Api.External.Tmdb;

// Wire models for the TMDB v3 API — used only inside the TMDB client and import service.

public class TmdbSearchResponse
{
    [JsonPropertyName("results")]
    public List<TmdbSearchResult> Results { get; set; } = [];
}

public class TmdbSearchResult
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    /// <summary>"movie", "tv" or "person" (from search/multi).</summary>
    [JsonPropertyName("media_type")]
    public string? MediaTypeRaw { get; set; }

    [JsonPropertyName("title")]
    public string? MovieTitle { get; set; }

    [JsonPropertyName("name")]
    public string? TvName { get; set; }

    [JsonPropertyName("overview")]
    public string? Overview { get; set; }

    [JsonPropertyName("release_date")]
    public string? ReleaseDate { get; set; }

    [JsonPropertyName("first_air_date")]
    public string? FirstAirDate { get; set; }

    [JsonPropertyName("poster_path")]
    public string? PosterPath { get; set; }

    [JsonPropertyName("vote_average")]
    public double VoteAverage { get; set; }
}

public class TmdbGenre
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";
}

public class TmdbCastMember
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("character")]
    public string? Character { get; set; }

    [JsonPropertyName("profile_path")]
    public string? ProfilePath { get; set; }

    [JsonPropertyName("order")]
    public int Order { get; set; }
}

public class TmdbCrewMember
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("job")]
    public string? Job { get; set; }
}

public class TmdbCredits
{
    [JsonPropertyName("cast")]
    public List<TmdbCastMember> Cast { get; set; } = [];

    [JsonPropertyName("crew")]
    public List<TmdbCrewMember> Crew { get; set; } = [];
}

public class TmdbMovieDetails
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = "";

    [JsonPropertyName("overview")]
    public string? Overview { get; set; }

    [JsonPropertyName("release_date")]
    public string? ReleaseDate { get; set; }

    [JsonPropertyName("runtime")]
    public int? Runtime { get; set; }

    [JsonPropertyName("poster_path")]
    public string? PosterPath { get; set; }

    [JsonPropertyName("backdrop_path")]
    public string? BackdropPath { get; set; }

    [JsonPropertyName("genres")]
    public List<TmdbGenre> Genres { get; set; } = [];

    [JsonPropertyName("credits")]
    public TmdbCredits Credits { get; set; } = new();
}

public class TmdbTvCreator
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";
}

public class TmdbTvDetails
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("overview")]
    public string? Overview { get; set; }

    [JsonPropertyName("first_air_date")]
    public string? FirstAirDate { get; set; }

    [JsonPropertyName("episode_run_time")]
    public List<int> EpisodeRunTime { get; set; } = [];

    [JsonPropertyName("poster_path")]
    public string? PosterPath { get; set; }

    [JsonPropertyName("backdrop_path")]
    public string? BackdropPath { get; set; }

    [JsonPropertyName("genres")]
    public List<TmdbGenre> Genres { get; set; } = [];

    [JsonPropertyName("created_by")]
    public List<TmdbTvCreator> CreatedBy { get; set; } = [];

    [JsonPropertyName("credits")]
    public TmdbCredits Credits { get; set; } = new();
}

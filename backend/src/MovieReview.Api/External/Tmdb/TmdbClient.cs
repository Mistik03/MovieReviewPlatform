using System.Net;
using Microsoft.Extensions.Options;
using MovieReview.Api.Domain.Exceptions;

namespace MovieReview.Api.External.Tmdb;

public class TmdbClient : ITmdbClient
{
    private readonly HttpClient _http;
    private readonly TmdbSettings _settings;

    public TmdbClient(HttpClient http, IOptions<TmdbSettings> settings)
    {
        _http = http;
        _settings = settings.Value;
    }

    public async Task<TmdbSearchResponse> SearchMultiAsync(string query, CancellationToken ct = default)
    {
        var url = $"search/multi?query={Uri.EscapeDataString(query)}&include_adult=false";
        var response = await SendAsync(url, ct);
        return await response.Content.ReadFromJsonAsync<TmdbSearchResponse>(cancellationToken: ct)
            ?? new TmdbSearchResponse();
    }

    public async Task<TmdbMovieDetails?> GetMovieWithCreditsAsync(int tmdbId, CancellationToken ct = default)
    {
        var response = await SendAsync($"movie/{tmdbId}?append_to_response=credits", ct, allowNotFound: true);
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        return await response.Content.ReadFromJsonAsync<TmdbMovieDetails>(cancellationToken: ct);
    }

    public async Task<TmdbTvDetails?> GetTvShowWithCreditsAsync(int tmdbId, CancellationToken ct = default)
    {
        var response = await SendAsync($"tv/{tmdbId}?append_to_response=credits", ct, allowNotFound: true);
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        return await response.Content.ReadFromJsonAsync<TmdbTvDetails>(cancellationToken: ct);
    }

    public string? BuildImageUrl(string? path, string size) =>
        string.IsNullOrWhiteSpace(path) ? null : $"{_settings.ImageBaseUrl.TrimEnd('/')}/{size}{path}";

    private async Task<HttpResponseMessage> SendAsync(string relativeUrl, CancellationToken ct, bool allowNotFound = false)
    {
        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
            throw new BadRequestException("TMDB integration is not configured — set the Tmdb:ApiKey setting.");

        using var request = new HttpRequestMessage(HttpMethod.Get, BuildUrl(relativeUrl));

        // TMDB v4 read access tokens are JWTs sent as a bearer header;
        // classic v3 keys travel as a query parameter (handled in BuildUrl).
        if (IsV4Token)
            request.Headers.Authorization = new("Bearer", _settings.ApiKey);

        var response = await _http.SendAsync(request, ct);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new BadRequestException("TMDB rejected the configured API key (401). Check Tmdb:ApiKey.");

        if (!response.IsSuccessStatusCode && !(allowNotFound && response.StatusCode == HttpStatusCode.NotFound))
            throw new BadRequestException($"TMDB request failed with status {(int)response.StatusCode}.");

        return response;
    }

    private bool IsV4Token => _settings.ApiKey.StartsWith("eyJ", StringComparison.Ordinal);

    private string BuildUrl(string relativeUrl)
    {
        if (IsV4Token) return relativeUrl;
        var separator = relativeUrl.Contains('?') ? '&' : '?';
        return $"{relativeUrl}{separator}api_key={Uri.EscapeDataString(_settings.ApiKey)}";
    }
}

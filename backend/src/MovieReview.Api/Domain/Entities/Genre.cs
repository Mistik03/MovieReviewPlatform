namespace MovieReview.Api.Domain.Entities;

public class Genre
{
    public int Id { get; set; }

    /// <summary>TMDB genre identifier; null for manually created genres.</summary>
    public int? TmdbId { get; set; }

    public string Name { get; set; } = null!;

    public ICollection<TitleGenre> TitleGenres { get; set; } = new List<TitleGenre>();
}

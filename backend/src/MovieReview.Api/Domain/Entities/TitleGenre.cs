namespace MovieReview.Api.Domain.Entities;

/// <summary>Join entity for the many-to-many relationship between titles and genres.</summary>
public class TitleGenre
{
    public int TitleId { get; set; }
    public Title Title { get; set; } = null!;

    public int GenreId { get; set; }
    public Genre Genre { get; set; } = null!;
}

namespace MovieReview.Api.DTOs.Genres;

public class GenreResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;

    /// <summary>Number of titles assigned to this genre — computed.</summary>
    public int TitleCount { get; set; }
}

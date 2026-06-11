namespace MovieReview.Api.DTOs.Titles;

/// <summary>Full detail view — list fields plus description, crew and cast.</summary>
public class TitleDetailDto : TitleResponseDto
{
    public int? TmdbId { get; set; }
    public string? Description { get; set; }
    public string? Director { get; set; }
    public int? RuntimeMinutes { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<CastMemberDto> Cast { get; set; } = [];
}

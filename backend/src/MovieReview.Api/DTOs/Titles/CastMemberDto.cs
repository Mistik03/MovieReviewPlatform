namespace MovieReview.Api.DTOs.Titles;

public class CastMemberDto
{
    public int PersonId { get; set; }
    public string Name { get; set; } = null!;
    public string? CharacterName { get; set; }
    public string? ProfileImageUrl { get; set; }
    public int CastOrder { get; set; }
}

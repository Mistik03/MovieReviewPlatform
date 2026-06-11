namespace MovieReview.Api.Domain.Entities;

/// <summary>Join entity (with payload) linking a person to a title they appear in.</summary>
public class CastMember
{
    public int TitleId { get; set; }
    public Title Title { get; set; } = null!;

    public int PersonId { get; set; }
    public Person Person { get; set; } = null!;

    public string? CharacterName { get; set; }

    /// <summary>Billing order — lower means more prominent.</summary>
    public int CastOrder { get; set; }
}

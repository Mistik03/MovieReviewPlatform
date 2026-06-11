using System.ComponentModel.DataAnnotations;
using MovieReview.Api.Domain.Enums;

namespace MovieReview.Api.DTOs.Import;

public class TmdbImportRequestDto
{
    [Required, Range(1, int.MaxValue)]
    public int TmdbId { get; set; }

    [Required]
    public MediaType MediaType { get; set; }
}

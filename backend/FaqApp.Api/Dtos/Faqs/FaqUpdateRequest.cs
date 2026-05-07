using System.ComponentModel.DataAnnotations;

namespace FaqApp.Api.Dtos.Faqs;

public class FaqUpdateRequest
{
    [Required]
    [MaxLength(100)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Body { get; set; } = string.Empty;

    [Required]
    public int CategoryId { get; set; }

    public List<int> TagIds { get; set; } = new();

    public bool IsPublished { get; set; }
}
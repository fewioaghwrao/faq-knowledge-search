namespace FaqApp.Api.Dtos.Faqs;

public class FaqListItemDto
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    public string CategoryName { get; set; } = string.Empty;

    public List<string> Tags { get; set; } = new();

    public bool IsPublished { get; set; }

    public int ViewCount { get; set; }

    public DateTime UpdatedAt { get; set; }
}

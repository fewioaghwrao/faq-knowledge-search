namespace FaqApp.Api.Dtos.Faqs;

public class FaqSearchQuery
{
    public string? Keyword { get; set; }

    public int? CategoryId { get; set; }

    public int? TagId { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;

    public bool IncludeUnpublished { get; set; } = false;
}
namespace FaqKnowledgeSearch.Application.Faqs.Public;

public sealed class PublicFaqSearchCondition
{
    public string? Keyword { get; init; }

    public int? CategoryId { get; init; }

    public int? TagId { get; init; }

    public FaqSortOrder SortOrder { get; init; }
        = FaqSortOrder.Relevance;

    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 10;
}
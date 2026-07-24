namespace FaqKnowledgeSearch.Application.Faqs.Public;

public sealed record PublicFaqSearchItem(
    int Id,
    string Title,
    string BodyExcerpt,
    int CategoryId,
    string CategoryName,
    IReadOnlyList<string> TagNames,
    int ViewCount,
    DateTime UpdatedAt,
    int RelevanceScore);
namespace FaqKnowledgeSearch.Application.Faqs.Public;

public sealed record PublicFaqDetail(
    int Id,
    string Title,
    string Body,
    int CategoryId,
    string CategoryName,
    IReadOnlyList<string> TagNames,
    int ViewCount,
    DateTime CreatedAt,
    DateTime UpdatedAt);
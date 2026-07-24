namespace FaqKnowledgeSearch.Application.Faqs.Admin;

public sealed record AdminFaqListItem(
    int Id,
    string Title,
    string BodyPreview,
    string CategoryName,
    bool IsPublished,
    int ViewCount,
    DateTime UpdatedAt);
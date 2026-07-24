namespace FaqKnowledgeSearch.Application.AiSearch.History.Admin;

public sealed record AdminAiSearchHistoryReferenceItem(
    int FaqId,
    string FaqTitle,
    string CategoryName,
    int DisplayOrder,
    int Score);

public sealed record AdminAiSearchHistoryDetail(
    long Id,
    string Question,
    string? Answer,
    bool IsSuccess,
    string? ErrorMessage,
    string ModelName,
    bool UsedExternalAi,
    bool? WasHelpful,
    DateTime CreatedAt,
    IReadOnlyList<AdminAiSearchHistoryReferenceItem> References);
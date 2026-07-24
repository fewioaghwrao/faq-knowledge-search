namespace FaqKnowledgeSearch.Application.AiSearch.History.Admin;

public sealed record AdminAiSearchHistoryListItem(
    long Id,
    string Question,
    string ResponsePreview,
    bool IsSuccess,
    int ReferenceCount,
    bool? WasHelpful,
    string ModelName,
    bool UsedExternalAi,
    DateTime CreatedAt);
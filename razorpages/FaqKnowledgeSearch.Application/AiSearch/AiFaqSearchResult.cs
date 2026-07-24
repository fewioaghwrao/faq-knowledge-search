namespace FaqKnowledgeSearch.Application.AiSearch;

public sealed record AiFaqSearchResult(
    long HistoryId,
    string Question,
    string Answer,
    IReadOnlyList<AiFaqReference> References,
    bool UsedExternalAi);
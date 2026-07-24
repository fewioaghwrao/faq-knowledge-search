namespace FaqKnowledgeSearch.Application.AiSearch.History.Admin;

public enum AdminAiSearchStatusFilter
{
    All = 0,
    Success = 1,
    Failure = 2
}

public enum AdminAiSearchFeedbackFilter
{
    All = 0,
    Helpful = 1,
    NotHelpful = 2,
    None = 3
}

public sealed class AdminAiSearchHistorySearchCondition
{
    public string? Keyword { get; init; }

    public AdminAiSearchStatusFilter Status { get; init; }
        = AdminAiSearchStatusFilter.All;

    public AdminAiSearchFeedbackFilter Feedback { get; init; }
        = AdminAiSearchFeedbackFilter.All;

    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 10;
}
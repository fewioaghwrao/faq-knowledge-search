namespace FaqKnowledgeSearch.Application.AiSearch.Feedback;

public interface IAiSearchFeedbackService
{
    Task<AiSearchFeedbackResult> SetFeedbackAsync(
        long historyId,
        bool wasHelpful,
        CancellationToken cancellationToken = default);
}

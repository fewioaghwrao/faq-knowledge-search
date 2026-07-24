using FaqKnowledgeSearch.Application.AiSearch.History;

namespace FaqKnowledgeSearch.Application.AiSearch.Feedback;

public sealed class AiSearchFeedbackService(
    IAiSearchHistoryRepository historyRepository)
    : IAiSearchFeedbackService
{
    public async Task<AiSearchFeedbackResult> SetFeedbackAsync(
        long historyId,
        bool wasHelpful,
        CancellationToken cancellationToken = default)
    {
        if (historyId <= 0)
        {
            return AiSearchFeedbackResult.NotFound;
        }

        var history =
            await historyRepository.GetByIdForUpdateAsync(
                historyId,
                cancellationToken);

        if (history is null)
        {
            return AiSearchFeedbackResult.NotFound;
        }

        if (!history.IsSuccess ||
            !history.UsedExternalAi)
        {
            return AiSearchFeedbackResult.NotEligible;
        }

        history.SetFeedback(wasHelpful);

        await historyRepository.SaveChangesAsync(
            cancellationToken);

        return AiSearchFeedbackResult.Success;
    }
}
using FaqApp.Api.Dtos.Ai;

namespace FaqApp.Api.Services.Interfaces;

public interface IAiSearchHistoryService
{
    Task<List<AiSearchHistoryListItemDto>> GetListAsync(AiSearchHistoryQuery query);

    Task<AiSearchHistoryDetailDto?> GetDetailAsync(int id);

    Task UpdateFeedbackAsync(int historyId, bool isHelpful);
}
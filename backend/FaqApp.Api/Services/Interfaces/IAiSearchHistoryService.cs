using FaqApp.Api.Dtos.Ai;

namespace FaqApp.Api.Services.Interfaces;

public interface IAiSearchHistoryService
{
    Task<List<AiSearchHistoryListItemDto>> GetListAsync(AiSearchHistoryQuery query);
}

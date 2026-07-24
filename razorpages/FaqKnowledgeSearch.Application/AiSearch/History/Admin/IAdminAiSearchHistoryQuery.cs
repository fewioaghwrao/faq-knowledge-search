using FaqKnowledgeSearch.Application.Common.Models;

namespace FaqKnowledgeSearch.Application.AiSearch.History.Admin;

public interface IAdminAiSearchHistoryQuery
{
    Task<PagedResult<AdminAiSearchHistoryListItem>> SearchAsync(
        AdminAiSearchHistorySearchCondition condition,
        CancellationToken cancellationToken = default);

    Task<AdminAiSearchHistoryDetail?> GetDetailAsync(
        long historyId,
        CancellationToken cancellationToken = default);
}
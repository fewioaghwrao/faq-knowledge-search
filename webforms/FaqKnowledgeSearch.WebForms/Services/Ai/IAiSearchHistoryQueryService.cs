using System.Collections.Generic;
using FaqKnowledgeSearch.WebForms.Dtos.Ai;

namespace FaqKnowledgeSearch.WebForms.Services.Ai
{
    public interface IAiSearchHistoryQueryService
    {
        IReadOnlyList<AiSearchHistoryListItemDto>
            SearchHistories(
                string keyword,
                bool? isSuccess);

        AiSearchHistoryDetailDto GetHistoryById(
            long id);
    }
}
using System.Collections.Generic;
using System.Threading.Tasks;
using FaqKnowledgeSearch.WebForms.Dtos;

namespace FaqKnowledgeSearch.WebForms.Services.Ai
{
    public interface IAiSearchHistoryService
    {
        Task<long> SaveSuccessAsync(
            string question,
            string answer,
            IReadOnlyList<FaqListItemDto> sources);

        Task<long> SaveFailureAsync(
            string question,
            string errorMessage,
            IReadOnlyList<FaqListItemDto> sources);
    }
}
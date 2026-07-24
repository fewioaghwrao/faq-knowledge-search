using FaqKnowledgeSearch.Domain.AiSearch;

namespace FaqKnowledgeSearch.Application.AiSearch.History;

public interface IAiSearchHistoryRepository
{
    Task<AiSearchHistory?> GetByIdForUpdateAsync(
        long historyId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        AiSearchHistory history,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
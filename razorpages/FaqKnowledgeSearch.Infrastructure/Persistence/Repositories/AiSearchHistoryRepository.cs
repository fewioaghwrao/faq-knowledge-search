using FaqKnowledgeSearch.Application.AiSearch.History;
using FaqKnowledgeSearch.Domain.AiSearch;
using Microsoft.EntityFrameworkCore;

namespace FaqKnowledgeSearch.Infrastructure.Persistence.Repositories;

public sealed class AiSearchHistoryRepository(
    AppDbContext dbContext)
    : IAiSearchHistoryRepository
{
    public Task<AiSearchHistory?> GetByIdForUpdateAsync(
        long historyId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.AiSearchHistories
            .SingleOrDefaultAsync(
                history => history.Id == historyId,
                cancellationToken);
    }

    public async Task AddAsync(
        AiSearchHistory history,
        CancellationToken cancellationToken = default)
    {
        await dbContext.AiSearchHistories.AddAsync(
            history,
            cancellationToken);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(
            cancellationToken);
    }
}
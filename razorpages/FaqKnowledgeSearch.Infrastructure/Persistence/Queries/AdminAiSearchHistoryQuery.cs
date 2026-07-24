using FaqKnowledgeSearch.Application.AiSearch.History.Admin;
using FaqKnowledgeSearch.Application.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace FaqKnowledgeSearch.Infrastructure.Persistence.Queries;

public sealed class AdminAiSearchHistoryQuery(
    AppDbContext dbContext)
    : IAdminAiSearchHistoryQuery
{
    public async Task<PagedResult<AdminAiSearchHistoryListItem>>
        SearchAsync(
            AdminAiSearchHistorySearchCondition condition,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(condition);

        var page = Math.Max(condition.Page, 1);
        var pageSize = Math.Clamp(
            condition.PageSize,
            1,
            100);

        var query = dbContext.AiSearchHistories
            .AsNoTracking()
            .AsQueryable();

        var keyword = condition.Keyword?.Trim();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(history =>
                history.Question.Contains(keyword)
                || (history.Answer != null
                    && history.Answer.Contains(keyword))
                || (history.ErrorMessage != null
                    && history.ErrorMessage.Contains(keyword)));
        }

        query = condition.Status switch
        {
            AdminAiSearchStatusFilter.Success =>
                query.Where(history => history.IsSuccess),

            AdminAiSearchStatusFilter.Failure =>
                query.Where(history => !history.IsSuccess),

            _ => query
        };

        query = condition.Feedback switch
        {
            AdminAiSearchFeedbackFilter.Helpful =>
                query.Where(history =>
                    history.WasHelpful == true),

            AdminAiSearchFeedbackFilter.NotHelpful =>
                query.Where(history =>
                    history.WasHelpful == false),

            AdminAiSearchFeedbackFilter.None =>
                query.Where(history =>
                    history.WasHelpful == null),

            _ => query
        };

        var totalCount =
            await query.CountAsync(cancellationToken);

        var rows = await query
            .OrderByDescending(history => history.CreatedAt)
            .ThenByDescending(history => history.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(history => new
            {
                history.Id,
                history.Question,
                history.Answer,
                history.ErrorMessage,
                history.IsSuccess,
                history.WasHelpful,
                history.ModelName,
                history.UsedExternalAi,
                history.CreatedAt,
                ReferenceCount = history.References.Count
            })
            .ToListAsync(cancellationToken);

        var items = rows
            .Select(row =>
            {
                var sourceText = row.IsSuccess
                    ? row.Answer
                    : row.ErrorMessage;

                return new AdminAiSearchHistoryListItem(
                    row.Id,
                    row.Question,
                    CreatePreview(sourceText),
                    row.IsSuccess,
                    row.ReferenceCount,
                    row.WasHelpful,
                    row.ModelName,
                    row.UsedExternalAi,
                    row.CreatedAt);
            })
            .ToList();

        return new PagedResult<AdminAiSearchHistoryListItem>(
            items,
            totalCount,
            page,
            pageSize);
    }

    public async Task<AdminAiSearchHistoryDetail?> GetDetailAsync(
        long historyId,
        CancellationToken cancellationToken = default)
    {
        var history = await dbContext.AiSearchHistories
            .AsNoTracking()
            .Include(item => item.References)
            .SingleOrDefaultAsync(
                item => item.Id == historyId,
                cancellationToken);

        if (history is null)
        {
            return null;
        }

        var references = history.References
            .OrderBy(reference => reference.DisplayOrder)
            .ThenBy(reference => reference.Id)
            .Select(reference =>
                new AdminAiSearchHistoryReferenceItem(
                    reference.FaqId,
                    reference.FaqTitle,
                    reference.CategoryName,
                    reference.DisplayOrder,
                    reference.Score))
            .ToList();

        return new AdminAiSearchHistoryDetail(
            history.Id,
            history.Question,
            history.Answer,
            history.IsSuccess,
            history.ErrorMessage,
            history.ModelName,
            history.UsedExternalAi,
            history.WasHelpful,
            history.CreatedAt,
            references);
    }

    private static string CreatePreview(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "内容なし";
        }

        var normalized = value
            .Replace("\r", " ")
            .Replace("\n", " ")
            .Trim();

        return normalized.Length <= 100
            ? normalized
            : normalized[..100] + "…";
    }
}
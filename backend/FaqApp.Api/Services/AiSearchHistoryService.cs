using FaqApp.Api.Data;
using FaqApp.Api.Dtos.Ai;
using FaqApp.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FaqApp.Api.Services;

public class AiSearchHistoryService : IAiSearchHistoryService
{
    private readonly AppDbContext _dbContext;

    public AiSearchHistoryService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<AiSearchHistoryListItemDto>> GetListAsync(AiSearchHistoryQuery query)
    {
        var page = query.Page <= 0 ? 1 : query.Page;
        var pageSize = query.PageSize <= 0 ? 20 : query.PageSize;
        pageSize = Math.Min(pageSize, 100);

        var histories = _dbContext.AiSearchHistories
            .Include(x => x.Sources)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            var keyword = query.Keyword.Trim();

            histories = histories.Where(x =>
                x.Question.Contains(keyword) ||
                (x.AiAnswer != null && x.AiAnswer.Contains(keyword)) ||
                (x.ErrorMessage != null && x.ErrorMessage.Contains(keyword)));
        }

        if (query.IsSuccess.HasValue)
        {
            histories = histories.Where(x => x.IsSuccess == query.IsSuccess.Value);
        }

        if (query.IsHelpful.HasValue)
        {
            histories = histories.Where(x => x.IsHelpful == query.IsHelpful.Value);
        }

        return await histories
            .OrderByDescending(x => x.ExecutedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new AiSearchHistoryListItemDto
            {
                Id = x.Id,
                Question = x.Question,
                AnswerPreview = x.AiAnswer == null
                    ? null
                    : x.AiAnswer.Length <= 80
                        ? x.AiAnswer
                        : x.AiAnswer.Substring(0, 80) + "...",
                IsSuccess = x.IsSuccess,
                ErrorMessage = x.ErrorMessage,
                IsHelpful = x.IsHelpful,
                SourceCount = x.Sources.Count,
                ExecutedAt = x.ExecutedAt
            })
            .ToListAsync();
    }
}
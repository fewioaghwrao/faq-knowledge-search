using FaqApp.Api.Data;
using FaqApp.Api.Dtos.Ai;
using FaqApp.Api.Dtos.Faqs;
using FaqApp.Api.Entities;
using FaqApp.Api.Services.Interfaces;
using FaqApp.Api.Settings;
using Microsoft.Extensions.Options;

namespace FaqApp.Api.Services;

public class AiService : IAiService
{
    private readonly IFaqService _faqService;
    private readonly IAiApiClient _aiApiClient;
    private readonly AiSettings _aiSettings;
    private readonly ILogger<AiService> _logger;
    private readonly AppDbContext _dbContext;

    public AiService(
        IFaqService faqService,
        IAiApiClient aiApiClient,
        IOptions<AiSettings> aiOptions,
        ILogger<AiService> logger,
        AppDbContext dbContext)
    {
        _faqService = faqService;
        _aiApiClient = aiApiClient;
        _aiSettings = aiOptions.Value;
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<AiSearchResponse> SearchAsync(AiSearchRequest request)
    {
        var question = request.Question.Trim();

        if (string.IsNullOrWhiteSpace(question))
        {
            return new AiSearchResponse
            {
                Answer = null,
                Disclaimer = null,
                Sources = new List<AiSourceDto>(),
                Message = "質問文を入力してください。",
                AiHistoryId = 0
            };
        }

        var faqs = await _faqService.SearchAsync(new FaqSearchQuery
        {
            Keyword = question,
            Page = 1,
            PageSize = _aiSettings.MaxContextFaqCount <= 0
                ? 5
                : _aiSettings.MaxContextFaqCount,
            Sort = "score",
            Highlight = false,
            IncludeUnpublished = false
        });

        if (faqs.Count == 0)
        {
            var noFaqHistory = new AiSearchHistory
            {
                Question = question,
                SearchKeywords = question,
                AiAnswer = null,
                IsSuccess = false,
                ErrorMessage = "該当するFAQが見つかりませんでした。",
                ExecutedAt = DateTime.UtcNow
            };

            _dbContext.AiSearchHistories.Add(noFaqHistory);
            await _dbContext.SaveChangesAsync();

            return new AiSearchResponse
            {
                Answer = null,
                Disclaimer = null,
                Sources = new List<AiSourceDto>(),
                Message = "該当するFAQが見つかりませんでした。管理者にご相談ください。",
                AiHistoryId = noFaqHistory.Id
            };
        }

        var faqContexts = faqs.Select((faq, index) => BuildFaqContext(faq, index + 1));

        string answer;

        try
        {
            answer = await _aiApiClient.GenerateAnswerAsync(
                question,
                faqContexts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI回答の生成に失敗しました。");

            var failureHistory = new AiSearchHistory
            {
                Question = question,
                SearchKeywords = question,
                AiAnswer = null,
                IsSuccess = false,
                ErrorMessage = ex.Message,
                ExecutedAt = DateTime.UtcNow,
                Sources = faqs.Select((faq, index) => new AiSearchHistorySource
                {
                    FaqId = faq.Id,
                    FaqTitle = faq.Title,
                    DisplayOrder = index + 1,
                    Score = faq.Score
                }).ToList()
            };

            _dbContext.AiSearchHistories.Add(failureHistory);
            await _dbContext.SaveChangesAsync();

            return new AiSearchResponse
            {
                Answer = null,
                Disclaimer = null,
                Sources = faqs.Select(x => new AiSourceDto
                {
                    Id = x.Id,
                    Title = x.Title,
                    Url = $"/faqs/{x.Id}"
                }).ToList(),
                Message = "AI回答の生成に失敗しました。参照元FAQをご確認ください。",
                AiHistoryId = failureHistory.Id
            };
        }

        var successHistory = new AiSearchHistory
        {
            Question = question,
            SearchKeywords = question,
            AiAnswer = answer,
            IsSuccess = true,
            ErrorMessage = null,
            ExecutedAt = DateTime.UtcNow,
            Sources = faqs.Select((faq, index) => new AiSearchHistorySource
            {
                FaqId = faq.Id,
                FaqTitle = faq.Title,
                DisplayOrder = index + 1,
                Score = faq.Score
            }).ToList()
        };

        _dbContext.AiSearchHistories.Add(successHistory);
        await _dbContext.SaveChangesAsync();

        return new AiSearchResponse
        {
            Answer = answer,
            Disclaimer = "この回答はFAQをもとに生成されています。必ず参照元を確認してください。",
            Sources = faqs.Select(x => new AiSourceDto
            {
                Id = x.Id,
                Title = x.Title,
                Url = $"/faqs/{x.Id}"
            }).ToList(),
            Message = null,
            AiHistoryId = successHistory.Id
        };
    }

    private static string BuildFaqContext(FaqListItemDto faq, int index)
    {
        var body = string.IsNullOrWhiteSpace(faq.Body)
            ? "FAQ本文が登録されていません。"
            : faq.Body.Trim();

        return $"""
        [FAQ{index}]
        ID: {faq.Id}
        タイトル: {faq.Title}
        カテゴリ: {faq.CategoryName}
        本文:
        {body}
        """;
    }
}
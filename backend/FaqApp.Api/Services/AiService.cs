using FaqApp.Api.Dtos.Ai;
using FaqApp.Api.Dtos.Faqs;
using FaqApp.Api.Services.Interfaces;

namespace FaqApp.Api.Services;

public class AiService : IAiService
{
    private readonly IFaqService _faqService;

    public AiService(IFaqService faqService)
    {
        _faqService = faqService;
    }

    public async Task<AiSearchResponse> SearchAsync(AiSearchRequest request)
    {
        var question = request.Question.Trim();

        var faqs = await _faqService.SearchAsync(new FaqSearchQuery
        {
            Keyword = question,
            Page = 1,
            PageSize = 5,
            Sort = "score",
            Highlight = false,
            IncludeUnpublished = false
        });

        if (faqs.Count == 0)
        {
            return new AiSearchResponse
            {
                Answer = null,
                Disclaimer = null,
                Sources = new List<AiSourceDto>(),
                Message = "該当するFAQが見つかりませんでした。管理者にご相談ください。",
                AiHistoryId = 0
            };
        }

        var topFaq = faqs.First();

        var answer = BuildMockAnswer(question, faqs);

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
            AiHistoryId = 0
        };
    }

    private static string BuildMockAnswer(string question, List<FaqListItemDto> faqs)
    {
        var topFaq = faqs.First();

        var lines = new List<string>
        {
            "【モックAI回答】",
            "",
            $"質問「{question}」に関連するFAQとして、主に「{topFaq.Title}」が見つかりました。",
            "",
            "現時点では外部AI APIは呼び出していないため、以下はFAQ本文をもとにした仮の要約です。",
            "",
            CreateExcerpt(topFaq.Body),
            "",
            "詳細は参照元FAQをご確認ください。"
        };

        if (faqs.Count >= 2)
        {
            lines.Add("");
            lines.Add("関連するFAQもあわせて確認してください。");

            foreach (var faq in faqs.Skip(1).Take(3))
            {
                lines.Add($"- {faq.Title}");
            }
        }

        return string.Join(Environment.NewLine, lines);
    }

    private static string CreateExcerpt(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return "FAQ本文が登録されていません。";
        }

        var normalized = body.Replace("\r\n", "\n").Replace("\r", "\n").Trim();

        return normalized.Length <= 250
            ? normalized
            : normalized[..250] + "...";
    }
}
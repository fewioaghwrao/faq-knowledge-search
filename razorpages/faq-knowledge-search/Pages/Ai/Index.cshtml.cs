using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Security.Cryptography;
using FaqKnowledgeSearch.Application.AiSearch;
using FaqKnowledgeSearch.Application.AiSearch.Feedback;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FaqKnowledgeSearch.Razor.Pages.Ai;

public sealed class IndexModel : PageModel
{
    private const string FeedbackProtectionPurpose =
        "FaqKnowledgeSearch.AiFeedback.v1";

    private readonly IAiFaqSearchService _aiFaqSearchService;

    private readonly IAiSearchFeedbackService
        _feedbackService;

    private readonly IDataProtector _feedbackProtector;

    public IndexModel(
        IAiFaqSearchService aiFaqSearchService,
        IAiSearchFeedbackService feedbackService,
        IDataProtectionProvider dataProtectionProvider)
    {
        _aiFaqSearchService = aiFaqSearchService;
        _feedbackService = feedbackService;

        _feedbackProtector =
            dataProtectionProvider.CreateProtector(
                FeedbackProtectionPurpose);
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public AiFaqSearchResult? Result
    { get; private set; }

    public string? FeedbackToken
    { get; private set; }

    public bool IsRateLimited { get; private set; }

    public int RetryAfterSeconds { get; private set; } = 60;

    public void OnGet(
        bool rateLimited = false,
        int retryAfter = 60)
    {
        IsRateLimited = rateLimited;

        RetryAfterSeconds = Math.Clamp(
            retryAfter,
            1,
            60);
    }

    public async Task<IActionResult> OnPostAsync(
        CancellationToken cancellationToken)
    {
        Input.Question =
            Input.Question?.Trim()
            ?? string.Empty;

        if (string.IsNullOrWhiteSpace(Input.Question))
        {
            ModelState.AddModelError(
                "Input.Question",
                "質問・検索キーワードを入力してください。");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            Result =
                await _aiFaqSearchService.SearchAsync(
                    Input.Question,
                    cancellationToken);

            if (Result.UsedExternalAi)
            {
                FeedbackToken =
                    CreateFeedbackToken(
                        Result.HistoryId);
            }
        }
        catch (ArgumentException exception)
        {
            ModelState.AddModelError(
                "Input.Question",
                exception.Message);
        }
        catch (AiAnswerGenerationException exception)
        {
            ModelState.AddModelError(
                string.Empty,
                exception.Message);
        }

        return Page();
    }

    public async Task<IActionResult> OnPostFeedbackAsync(
        string feedbackToken,
        bool wasHelpful,
        CancellationToken cancellationToken)
    {
        if (!TryReadHistoryId(
                feedbackToken,
                out var historyId))
        {
            return new JsonResult(new
            {
                success = false,
                message =
                    "フィードバック情報が正しくありません。"
            })
            {
                StatusCode = 400
            };
        }

        var result =
            await _feedbackService.SetFeedbackAsync(
                historyId,
                wasHelpful,
                cancellationToken);

        return result switch
        {
            AiSearchFeedbackResult.Success =>
                new JsonResult(new
                {
                    success = true,
                    wasHelpful,
                    message =
                        "フィードバックを登録しました。"
                }),

            AiSearchFeedbackResult.NotFound =>
                new JsonResult(new
                {
                    success = false,
                    message =
                        "対象のAI検索履歴が見つかりませんでした。"
                })
                {
                    StatusCode = 404
                },

            _ =>
                new JsonResult(new
                {
                    success = false,
                    message =
                        "この回答にはフィードバックを登録できません。"
                })
                {
                    StatusCode = 400
                }
        };
    }

    private string CreateFeedbackToken(long historyId)
    {
        var value = historyId.ToString(
            CultureInfo.InvariantCulture);

        return _feedbackProtector.Protect(value);
    }

    private bool TryReadHistoryId(
        string? feedbackToken,
        out long historyId)
    {
        historyId = 0;

        if (string.IsNullOrWhiteSpace(feedbackToken))
        {
            return false;
        }

        try
        {
            var unprotected =
                _feedbackProtector.Unprotect(
                    feedbackToken);

            return long.TryParse(
                unprotected,
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out historyId)
                && historyId > 0;
        }
        catch (CryptographicException)
        {
            return false;
        }
    }

    public sealed class InputModel
    {
        [Required(
            ErrorMessage =
                "質問・検索キーワードを入力してください。")]
        [StringLength(
            500,
            ErrorMessage =
                "質問は500文字以内で入力してください。")]
        [Display(Name = "質問・検索キーワード")]
        public string Question { get; set; }
            = string.Empty;
    }
}
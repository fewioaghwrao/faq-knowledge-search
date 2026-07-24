using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FaqKnowledgeSearch.Razor.Pages;

[AllowAnonymous]
[ResponseCache(
    Duration = 0,
    Location = ResponseCacheLocation.None,
    NoStore = true)]
public sealed class StatusCodeModel : PageModel
{
    public int Code { get; private set; }

    public string Label { get; private set; }
        = "HTTP Error";

    public string TitleText { get; private set; }
        = "要求を処理できませんでした";

    public string Description { get; private set; }
        = "指定された要求を正常に処理できませんでした。";

    public string? OriginalPath { get; private set; }

    public void OnGet(int statusCode)
    {
        Code = statusCode is >= 400 and <= 599
            ? statusCode
            : StatusCodes.Status500InternalServerError;

        // 直接 /StatusCode/404 を開いた場合も
        // 正しいHTTPステータスを返す
        Response.StatusCode = Code;

        var reExecuteFeature =
            HttpContext.Features
                .Get<IStatusCodeReExecuteFeature>();

        OriginalPath =
            reExecuteFeature?.OriginalPath;

        SetDisplayContent();
    }

    private void SetDisplayContent()
    {
        switch (Code)
        {
            case StatusCodes.Status400BadRequest:
                Label = "Bad Request";
                TitleText =
                    "リクエストを処理できませんでした";
                Description =
                    "送信された内容に問題があります。"
                    + "入力内容を確認して、もう一度お試しください。";
                break;

            case StatusCodes.Status401Unauthorized:
                Label = "Unauthorized";
                TitleText =
                    "ログインが必要です";
                Description =
                    "このページを利用するには、"
                    + "ログインが必要です。";
                break;

            case StatusCodes.Status403Forbidden:
                Label = "Forbidden";
                TitleText =
                    "アクセス権限がありません";
                Description =
                    "このページを表示する権限がありません。"
                    + "必要な場合はシステム管理者へお問い合わせください。";
                break;

            case StatusCodes.Status404NotFound:
                Label = "Not Found";
                TitleText =
                    "ページが見つかりません";
                Description =
                    "指定されたページは削除されたか、"
                    + "URLが変更された可能性があります。";
                break;

            case StatusCodes.Status405MethodNotAllowed:
                Label = "Method Not Allowed";
                TitleText =
                    "許可されていない操作です";
                Description =
                    "このURLでは、指定された操作方法を利用できません。";
                break;

            case StatusCodes.Status429TooManyRequests:
                Label = "Too Many Requests";
                TitleText =
                    "アクセス回数の上限に達しました";
                Description =
                    "短時間にアクセスが集中しています。"
                    + "少し時間を置いてから再度お試しください。";
                break;

            case StatusCodes.Status500InternalServerError:
                Label = "Internal Server Error";
                TitleText =
                    "サーバーでエラーが発生しました";
                Description =
                    "一時的な問題の可能性があります。"
                    + "時間を置いてから再度お試しください。";
                break;

            case StatusCodes.Status503ServiceUnavailable:
                Label = "Service Unavailable";
                TitleText =
                    "現在サービスを利用できません";
                Description =
                    "メンテナンス中または一時的に混雑しています。"
                    + "時間を置いてから再度お試しください。";
                break;

            default:
                Label = "HTTP Error";
                TitleText =
                    "要求を処理できませんでした";
                Description =
                    "予期しないHTTPエラーが発生しました。";
                break;
        }
    }
}
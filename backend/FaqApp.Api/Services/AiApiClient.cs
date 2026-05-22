using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FaqApp.Api.Services.Interfaces;
using FaqApp.Api.Settings;
using Microsoft.Extensions.Options;


namespace FaqApp.Api.Services;

public class AiApiClient : IAiApiClient
{

    private readonly HttpClient _httpClient;
    private readonly AiSettings _settings;
    private readonly ILogger<AiApiClient> _logger;

    public AiApiClient(
        HttpClient httpClient,
        IOptions<AiSettings> options,
        ILogger<AiApiClient> logger)
    {
        _httpClient = httpClient;
        _settings = options.Value;
        _logger = logger;
    }

    public async Task<string> GenerateAnswerAsync(
        string question,
        IEnumerable<string> faqContexts,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            throw new InvalidOperationException("AI APIキーが設定されていません。");
        }

        if (string.IsNullOrWhiteSpace(_settings.Endpoint))
        {
            throw new InvalidOperationException("AI APIエンドポイントが設定されていません。");
        }

        var systemPrompt = BuildSystemPrompt();
        var userPrompt = BuildUserPrompt(question, faqContexts);

        var requestBody = new
        {
            model = _settings.Model,
            input = new object[]
            {
                new
                {
                    role = "system",
                    content = systemPrompt
                },
                new
                {
                    role = "user",
                    content = userPrompt
                }
            },
            max_output_tokens = 1000
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, _settings.Endpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiKey);
        request.Content = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json");

        _logger.LogInformation(
            "OpenAI API呼び出し開始 Model={Model}",
            _settings.Model);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "OpenAI API呼び出し失敗 StatusCode={StatusCode} Body={Body}",
                (int)response.StatusCode,
                responseJson);

            throw new InvalidOperationException("AI回答の生成に失敗しました。");
        }

        var answer = ExtractOutputText(responseJson);

        if (string.IsNullOrWhiteSpace(answer))
        {
            _logger.LogWarning("OpenAI APIの応答から回答本文を取得できませんでした。Body={Body}", responseJson);
            throw new InvalidOperationException("AI回答が空でした。");
        }

        return answer.Trim();
    }

    private static string BuildSystemPrompt()
    {
        return """
        あなたは社内FAQ検索システムのアシスタントです。
        以下のルールを必ず守って回答してください。

        【回答ルール】
        - 提供されたFAQコンテキストの内容だけをもとに回答してください。
        - FAQに記載されていない内容は断言しないでください。
        - FAQにない手順、原因、担当部署、問い合わせ先を推測しないでください。
        - 機密情報、個人情報、認証情報を含む内容は出力しないでください。
        - 利用者向けに、簡潔で分かりやすい日本語で回答してください。
        - 最後に必ず「詳細は参照元FAQをご確認ください。」を付けてください。
        """;
    }

    private static string BuildUserPrompt(
        string question,
        IEnumerable<string> faqContexts)
    {
        var context = string.Join("\n\n", faqContexts);

        return $"""
        【FAQコンテキスト】
        {context}

        【ユーザーの質問】
        {question}

        【回答形式】
        - まず結論を簡潔に書いてください。
        - 必要に応じて箇条書きで補足してください。
        - FAQに根拠がない場合は「該当するFAQからは判断できません。」と回答してください。
        """;
    }

    private static string? ExtractOutputText(string responseJson)
    {
        using var document = JsonDocument.Parse(responseJson);
        var root = document.RootElement;

        if (!root.TryGetProperty("output", out var outputArray) ||
            outputArray.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        var texts = new List<string>();

        foreach (var outputItem in outputArray.EnumerateArray())
        {
            if (!outputItem.TryGetProperty("content", out var contentArray) ||
                contentArray.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            foreach (var contentItem in contentArray.EnumerateArray())
            {
                if (!contentItem.TryGetProperty("type", out var typeElement))
                {
                    continue;
                }

                if (typeElement.GetString() != "output_text")
                {
                    continue;
                }

                if (contentItem.TryGetProperty("text", out var textElement))
                {
                    var text = textElement.GetString();

                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        texts.Add(text);
                    }
                }
            }
        }

        return texts.Count == 0
            ? null
            : string.Join("\n", texts);
    }
}
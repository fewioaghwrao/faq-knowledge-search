using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using FaqKnowledgeSearch.WebForms.Settings;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FaqKnowledgeSearch.WebForms.Services.Ai
{
    public class AiApiClient : IAiApiClient
    {
        private static readonly HttpClient HttpClient =
            new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(30)
            };

        private readonly AiSettings _settings;

        public AiApiClient(AiSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            _settings = settings;
        }

        public async Task<string> GenerateAnswerAsync(
            string question,
            string faqContext)
        {
            ValidateSettings();

            if (string.IsNullOrWhiteSpace(question))
            {
                throw new ArgumentException(
                    "質問文が入力されていません。",
                    "question");
            }

            if (string.IsNullOrWhiteSpace(faqContext))
            {
                throw new ArgumentException(
                    "FAQコンテキストが設定されていません。",
                    "faqContext");
            }

            var requestBody = new
            {
                model = _settings.Model,
                instructions = BuildSystemPrompt(),
                input = BuildUserPrompt(
                    question,
                    faqContext),
                max_output_tokens = 1000
            };

            var requestJson =
                JsonConvert.SerializeObject(requestBody);

            using (var request = new HttpRequestMessage(
                HttpMethod.Post,
                _settings.Endpoint))
            {
                request.Headers.Authorization =
                    new AuthenticationHeaderValue(
                        "Bearer",
                        _settings.ApiKey);

                request.Content = new StringContent(
                    requestJson,
                    Encoding.UTF8,
                    "application/json");

                Trace.TraceInformation(
                    "AI API呼び出し開始 Model={0}",
                    _settings.Model);

                using (var response =
                    await HttpClient.SendAsync(request))
                {
                    var responseJson =
                        await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        Trace.TraceWarning(
                            "AI API呼び出し失敗 StatusCode={0}",
                            (int)response.StatusCode);

                        throw new InvalidOperationException(
                            "AI回答の生成に失敗しました。");
                    }

                    var answer =
                        ExtractOutputText(responseJson);

                    if (string.IsNullOrWhiteSpace(answer))
                    {
                        Trace.TraceWarning(
                            "AI APIの応答から回答本文を取得できませんでした。");

                        throw new InvalidOperationException(
                            "AI回答が空でした。");
                    }

                    return answer.Trim();
                }
            }
        }

        private void ValidateSettings()
        {
            if (string.IsNullOrWhiteSpace(_settings.ApiKey))
            {
                throw new InvalidOperationException(
                    "AI APIキーが設定されていません。");
            }

            if (string.IsNullOrWhiteSpace(_settings.Endpoint))
            {
                throw new InvalidOperationException(
                    "AI APIエンドポイントが設定されていません。");
            }

            if (string.IsNullOrWhiteSpace(_settings.Model))
            {
                throw new InvalidOperationException(
                    "AIモデルが設定されていません。");
            }
        }

        private static string BuildSystemPrompt()
        {
            return string.Join(
                Environment.NewLine,
                new[]
                {
                    "あなたは社内FAQ検索システムのアシスタントです。",
                    "以下のルールを必ず守って回答してください。",
                    string.Empty,
                    "【回答ルール】",
                    "- 提供されたFAQコンテキストの内容だけをもとに回答してください。",
                    "- FAQに記載されていない内容は断言しないでください。",
                    "- FAQにない手順、原因、担当部署、問い合わせ先を推測しないでください。",
                    "- 機密情報、個人情報、認証情報を含む内容は出力しないでください。",
                    "- 利用者向けに、簡潔で分かりやすい日本語で回答してください。",
                    "- 最後に必ず「詳細は参照元FAQをご確認ください。」を付けてください。"
                });
        }

        private static string BuildUserPrompt(
            string question,
            string faqContext)
        {
            return string.Join(
                Environment.NewLine,
                new[]
                {
                    "【FAQコンテキスト】",
                    faqContext,
                    string.Empty,
                    "【ユーザーの質問】",
                    question,
                    string.Empty,
                    "【回答形式】",
                    "- まず結論を簡潔に書いてください。",
                    "- 必要に応じて箇条書きで補足してください。",
                    "- FAQに根拠がない場合は「該当するFAQからは判断できません。」と回答してください。"
                });
        }

        private static string ExtractOutputText(
            string responseJson)
        {
            if (string.IsNullOrWhiteSpace(responseJson))
            {
                return null;
            }

            var root = JObject.Parse(responseJson);
            var outputArray = root["output"] as JArray;

            if (outputArray == null)
            {
                return null;
            }

            var texts = new List<string>();

            foreach (var outputItem in outputArray)
            {
                var contentArray =
                    outputItem["content"] as JArray;

                if (contentArray == null)
                {
                    continue;
                }

                foreach (var contentItem in contentArray)
                {
                    var type =
                        (string)contentItem["type"];

                    if (!string.Equals(
                            type,
                            "output_text",
                            StringComparison.Ordinal))
                    {
                        continue;
                    }

                    var text =
                        (string)contentItem["text"];

                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        texts.Add(text);
                    }
                }
            }

            return texts.Count == 0
                ? null
                : string.Join(
                    Environment.NewLine,
                    texts);
        }
    }
}
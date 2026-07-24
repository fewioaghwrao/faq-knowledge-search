using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FaqKnowledgeSearch.Application.AiSearch;
using FaqKnowledgeSearch.Infrastructure.Ai;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace FaqKnowledgeSearch.Infrastructure.Tests.Ai;

public sealed class OpenAiAnswerGeneratorTests
{
    // =========================================================
    // ValidateSettings
    // =========================================================

    [Fact]
    public async Task GenerateAsync_ProviderIsUnsupported_ThrowsExceptionWithoutSendingRequest()
    {
        // Arrange
        var handler =
            CreateJsonResponseHandler(
                HttpStatusCode.OK,
                """
                {
                  "output_text": "回答です。"
                }
                """);

        var settings =
            CreateSettings(
                provider: "AzureOpenAI");

        var sut =
            CreateSut(
                handler,
                settings);

        // Act
        var exception =
            await Assert.ThrowsAsync<AiAnswerGenerationException>(
                () => sut.GenerateAsync(
                    "質問",
                    []));

        // Assert
        Assert.Equal(
            "未対応のAIプロバイダーです: AzureOpenAI",
            exception.Message);

        Assert.Equal(0, handler.CallCount);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData("\r\n")]
    public async Task GenerateAsync_ApiKeyIsEmpty_ThrowsExceptionWithoutSendingRequest(
        string apiKey)
    {
        // Arrange
        var handler =
            CreateJsonResponseHandler(
                HttpStatusCode.OK,
                """
                {
                  "output_text": "回答です。"
                }
                """);

        var settings =
            CreateSettings(
                apiKey: apiKey);

        var sut =
            CreateSut(
                handler,
                settings);

        // Act
        var exception =
            await Assert.ThrowsAsync<AiAnswerGenerationException>(
                () => sut.GenerateAsync(
                    "質問",
                    []));

        // Assert
        Assert.Equal(
            "AIサービスのAPIキーが設定されていません。",
            exception.Message);

        Assert.Equal(0, handler.CallCount);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData("\r\n")]
    public async Task GenerateAsync_ModelIsEmpty_ThrowsExceptionWithoutSendingRequest(
        string model)
    {
        // Arrange
        var handler =
            CreateJsonResponseHandler(
                HttpStatusCode.OK,
                """
                {
                  "output_text": "回答です。"
                }
                """);

        var settings =
            CreateSettings(
                model: model);

        var sut =
            CreateSut(
                handler,
                settings);

        // Act
        var exception =
            await Assert.ThrowsAsync<AiAnswerGenerationException>(
                () => sut.GenerateAsync(
                    "質問",
                    []));

        // Assert
        Assert.Equal(
            "AIモデル名が設定されていません。",
            exception.Message);

        Assert.Equal(0, handler.CallCount);
    }

    [Theory]
    [InlineData("OpenAI")]
    [InlineData("openai")]
    [InlineData("OPENAI")]
    [InlineData("OpEnAi")]
    public async Task GenerateAsync_ProviderHasDifferentCasing_AcceptsProvider(
        string provider)
    {
        // Arrange
        var handler =
            CreateJsonResponseHandler(
                HttpStatusCode.OK,
                """
                {
                  "output_text": "回答です。"
                }
                """);

        var settings =
            CreateSettings(
                provider: provider);

        var sut =
            CreateSut(
                handler,
                settings);

        // Act
        var result =
            await sut.GenerateAsync(
                "質問",
                [CreateReference()]);

        // Assert
        Assert.Equal(
            "回答です。",
            result);

        Assert.Equal(1, handler.CallCount);
    }

    // =========================================================
    // 正常レスポンス
    // =========================================================

    [Fact]
    public async Task GenerateAsync_TopLevelOutputTextExists_ReturnsTrimmedAnswer()
    {
        // Arrange
        var handler =
            CreateJsonResponseHandler(
                HttpStatusCode.OK,
                """
                {
                  "output_text": "  設定画面から変更できます。  "
                }
                """);

        var sut =
            CreateSut(
                handler,
                CreateSettings());

        // Act
        var result =
            await sut.GenerateAsync(
                "パスワードの変更方法は？",
                [CreateReference()]);

        // Assert
        Assert.Equal(
            "設定画面から変更できます。",
            result);

        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task GenerateAsync_NestedOutputTextExists_JoinsAnswerParts()
    {
        // Arrange
        var handler =
            CreateJsonResponseHandler(
                HttpStatusCode.OK,
                """
                {
                  "output": [
                    {
                      "content": [
                        {
                          "type": "output_text",
                          "text": "手順1です。"
                        },
                        {
                          "type": "other",
                          "text": "この値は対象外です。"
                        },
                        {
                          "type": "output_text",
                          "text": "手順2です。"
                        }
                      ]
                    }
                  ]
                }
                """);

        var sut =
            CreateSut(
                handler,
                CreateSettings());

        // Act
        var result =
            await sut.GenerateAsync(
                "操作方法は？",
                [CreateReference()]);

        // Assert
        Assert.Equal(
            $"手順1です。{Environment.NewLine}手順2です。",
            result);
    }

    [Fact]
    public async Task GenerateAsync_NestedOutputContainsMultipleItems_JoinsAllAnswerParts()
    {
        // Arrange
        var handler =
            CreateJsonResponseHandler(
                HttpStatusCode.OK,
                """
                {
                  "output": [
                    {
                      "content": [
                        {
                          "type": "output_text",
                          "text": "回答その1"
                        }
                      ]
                    },
                    {
                      "content": [
                        {
                          "type": "output_text",
                          "text": "回答その2"
                        }
                      ]
                    }
                  ]
                }
                """);

        var sut =
            CreateSut(
                handler,
                CreateSettings());

        // Act
        var result =
            await sut.GenerateAsync(
                "質問",
                [CreateReference()]);

        // Assert
        Assert.Equal(
            $"回答その1{Environment.NewLine}回答その2",
            result);
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("""{"output_text": ""}""")]
    [InlineData("""{"output_text": "   "}""")]
    [InlineData("""{"output": []}""")]
    [InlineData("""{"output": {}}""")]
    public async Task GenerateAsync_ResponseDoesNotContainAnswer_ThrowsException(
        string responseJson)
    {
        // Arrange
        var handler =
            CreateJsonResponseHandler(
                HttpStatusCode.OK,
                responseJson);

        var sut =
            CreateSut(
                handler,
                CreateSettings());

        // Act
        var exception =
            await Assert.ThrowsAsync<AiAnswerGenerationException>(
                () => sut.GenerateAsync(
                    "質問",
                    [CreateReference()]));

        // Assert
        Assert.Equal(
            "AIサービスから回答本文を取得できませんでした。",
            exception.Message);
    }

    [Fact]
    public async Task GenerateAsync_ResponseIsInvalidJson_ThrowsWrappedException()
    {
        // Arrange
        var handler =
            CreateJsonResponseHandler(
                HttpStatusCode.OK,
                "これはJSONではありません。");

        var sut =
            CreateSut(
                handler,
                CreateSettings());

        // Act
        var exception =
            await Assert.ThrowsAsync<AiAnswerGenerationException>(
                () => sut.GenerateAsync(
                    "質問",
                    [CreateReference()]));

        // Assert
        Assert.Equal(
            "AIサービスの応答を読み取れませんでした。",
            exception.Message);

        Assert.NotNull(exception.InnerException);

        Assert.IsAssignableFrom<JsonException>(
            exception.InnerException);
    }

    // =========================================================
    // HTTPステータスエラー
    // =========================================================

    [Theory]
    [InlineData(
        HttpStatusCode.Unauthorized,
        "AIサービスの認証に失敗しました。APIキーを確認してください。")]
    [InlineData(
        HttpStatusCode.TooManyRequests,
        "AIサービスの利用上限に達したか、短時間に検索が集中しています。時間を置いて再度お試しください。")]
    [InlineData(
        HttpStatusCode.InternalServerError,
        "AIサービス側で一時的なエラーが発生しました。時間を置いて再度お試しください。")]
    [InlineData(
        HttpStatusCode.BadGateway,
        "AIサービス側で一時的なエラーが発生しました。時間を置いて再度お試しください。")]
    [InlineData(
        HttpStatusCode.ServiceUnavailable,
        "AIサービス側で一時的なエラーが発生しました。時間を置いて再度お試しください。")]
    [InlineData(
        HttpStatusCode.BadRequest,
        "AI回答の生成に失敗しました。")]
    [InlineData(
        HttpStatusCode.Forbidden,
        "AI回答の生成に失敗しました。")]
    public async Task GenerateAsync_ResponseIsError_ThrowsStatusSpecificException(
        HttpStatusCode statusCode,
        string expectedMessage)
    {
        // Arrange
        var handler =
            CreateJsonResponseHandler(
                statusCode,
                """
                {
                  "error": {
                    "message": "API側のエラー内容"
                  }
                }
                """);

        var sut =
            CreateSut(
                handler,
                CreateSettings());

        // Act
        var exception =
            await Assert.ThrowsAsync<AiAnswerGenerationException>(
                () => sut.GenerateAsync(
                    "質問",
                    [CreateReference()]));

        // Assert
        Assert.Equal(
            expectedMessage,
            exception.Message);

        Assert.Equal(1, handler.CallCount);
    }

    // =========================================================
    // 通信例外
    // =========================================================

    [Fact]
    public async Task GenerateAsync_RequestTimesOut_ThrowsTimeoutException()
    {
        // Arrange
        var timeoutException =
            new TaskCanceledException(
                "テスト用タイムアウト");

        var handler =
            new StubHttpMessageHandler(
                (_, _) =>
                    throw timeoutException);

        var sut =
            CreateSut(
                handler,
                CreateSettings());

        // Act
        var exception =
            await Assert.ThrowsAsync<AiAnswerGenerationException>(
                () => sut.GenerateAsync(
                    "質問",
                    [CreateReference()],
                    CancellationToken.None));

        // Assert
        Assert.Equal(
            "AI回答の生成がタイムアウトしました。"
            + "時間を置いて再度お試しください。",
            exception.Message);

        Assert.Same(
            timeoutException,
            exception.InnerException);

        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task GenerateAsync_HttpRequestFails_ThrowsCommunicationException()
    {
        // Arrange
        var httpException =
            new HttpRequestException(
                "テスト用通信エラー");

        var handler =
            new StubHttpMessageHandler(
                (_, _) =>
                    throw httpException);

        var sut =
            CreateSut(
                handler,
                CreateSettings());

        // Act
        var exception =
            await Assert.ThrowsAsync<AiAnswerGenerationException>(
                () => sut.GenerateAsync(
                    "質問",
                    [CreateReference()]));

        // Assert
        Assert.Equal(
            "AIサービスとの通信に失敗しました。",
            exception.Message);

        Assert.Same(
            httpException,
            exception.InnerException);

        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task GenerateAsync_UserCancelsRequest_PropagatesCancellation()
    {
        // Arrange
        using var cancellationTokenSource =
            new CancellationTokenSource();

        cancellationTokenSource.Cancel();

        var handler =
            new StubHttpMessageHandler(
                (_, cancellationToken) =>
                    Task.FromCanceled<HttpResponseMessage>(
                        cancellationToken));

        var sut =
            CreateSut(
                handler,
                CreateSettings());

        // Act・Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => sut.GenerateAsync(
                "質問",
                [CreateReference()],
                cancellationTokenSource.Token));
    }

    // =========================================================
    // HTTPリクエスト内容
    // =========================================================

    [Fact]
    public async Task GenerateAsync_ValidRequest_SendsExpectedHttpRequest()
    {
        // Arrange
        var handler =
            CreateJsonResponseHandler(
                HttpStatusCode.OK,
                """
                {
                  "output_text": "回答です。"
                }
                """);

        var settings =
            CreateSettings(
                apiKey: "test-secret-api-key",
                model: "gpt-test-model",
                maxOutputTokens: 321);

        IReadOnlyList<AiFaqReference> references =
        [
            new AiFaqReference(
                Id: 10,
                Title: "パスワード変更方法",
                Body: "設定画面からパスワードを変更できます。",
                CategoryName: "アカウント",
                Tags:
                [
                    "ログイン",
                    "パスワード"
                ],
                Score: 100)
        ];

        var sut =
            CreateSut(
                handler,
                settings);

        // Act
        var result =
            await sut.GenerateAsync(
                "パスワードの変更方法は？",
                references);

        // Assert
        Assert.Equal(
            "回答です。",
            result);

        var request =
            Assert.IsType<CapturedHttpRequest>(
                handler.LastRequest);

        Assert.Equal(
            HttpMethod.Post,
            request.Method);

        Assert.Equal(
            "https://api.openai.com/v1/responses",
            request.RequestUri?.ToString());

        Assert.Equal(
            "Bearer",
            request.Authorization?.Scheme);

        Assert.Equal(
            "test-secret-api-key",
            request.Authorization?.Parameter);

        using var document =
            JsonDocument.Parse(
                request.Body);

        var root =
            document.RootElement;

        Assert.Equal(
            "gpt-test-model",
            root.GetProperty("model")
                .GetString());

        Assert.Equal(
            321,
            root.GetProperty("max_output_tokens")
                .GetInt32());

        Assert.False(
            root.GetProperty("store")
                .GetBoolean());

        var instructions =
            root.GetProperty("instructions")
                .GetString();

        Assert.NotNull(instructions);

        Assert.Contains(
            "提供された参照FAQだけを根拠に回答してください。",
            instructions);

        Assert.Contains(
            "参照FAQにない内容を推測して作らないでください。",
            instructions);

        var input =
            root.GetProperty("input")
                .GetString();

        Assert.NotNull(input);

        Assert.Contains(
            "【利用者の質問】",
            input);

        Assert.Contains(
            "パスワードの変更方法は？",
            input);

        Assert.Contains(
            "【参照FAQ】",
            input);

        Assert.Contains(
            "--- FAQ ID: 10 ---",
            input);

        Assert.Contains(
            "タイトル: パスワード変更方法",
            input);

        Assert.Contains(
            "カテゴリ: アカウント",
            input);

        Assert.Contains(
            "タグ: ログイン, パスワード",
            input);

        Assert.Contains(
            "本文:",
            input);

        Assert.Contains(
            "設定画面からパスワードを変更できます。",
            input);

        Assert.Contains(
            "上記FAQを根拠に質問へ回答してください。",
            input);
    }

    [Fact]
    public async Task GenerateAsync_ReferenceHasNoTags_DoesNotAddTagLine()
    {
        // Arrange
        var handler =
            CreateJsonResponseHandler(
                HttpStatusCode.OK,
                """
                {
                  "output_text": "回答です。"
                }
                """);

        var reference =
            new AiFaqReference(
                Id: 1,
                Title: "タグなしFAQ",
                Body: "FAQ本文",
                CategoryName: "一般",
                Tags: [],
                Score: 10);

        var sut =
            CreateSut(
                handler,
                CreateSettings());

        // Act
        await sut.GenerateAsync(
            "質問",
            [reference]);

        // Assert
        var request =
            Assert.IsType<CapturedHttpRequest>(
                handler.LastRequest);

        using var document =
            JsonDocument.Parse(
                request.Body);

        var input =
            document.RootElement
                .GetProperty("input")
                .GetString();

        Assert.NotNull(input);

        Assert.Contains(
            "タイトル: タグなしFAQ",
            input);

        Assert.Contains(
            "カテゴリ: 一般",
            input);

        Assert.DoesNotContain(
            "タグ:",
            input);
    }

    [Fact]
    public async Task GenerateAsync_MultipleReferences_AddsAllReferencesToInput()
    {
        // Arrange
        var handler =
            CreateJsonResponseHandler(
                HttpStatusCode.OK,
                """
                {
                  "output_text": "回答です。"
                }
                """);

        IReadOnlyList<AiFaqReference> references =
        [
            new AiFaqReference(
                Id: 1,
                Title: "FAQその1",
                Body: "本文その1",
                CategoryName: "カテゴリ1",
                Tags: ["タグ1"],
                Score: 100),

            new AiFaqReference(
                Id: 2,
                Title: "FAQその2",
                Body: "本文その2",
                CategoryName: "カテゴリ2",
                Tags: ["タグ2"],
                Score: 80)
        ];

        var sut =
            CreateSut(
                handler,
                CreateSettings());

        // Act
        await sut.GenerateAsync(
            "質問",
            references);

        // Assert
        var request =
            Assert.IsType<CapturedHttpRequest>(
                handler.LastRequest);

        using var document =
            JsonDocument.Parse(
                request.Body);

        var input =
            document.RootElement
                .GetProperty("input")
                .GetString();

        Assert.NotNull(input);

        Assert.Contains(
            "--- FAQ ID: 1 ---",
            input);

        Assert.Contains(
            "タイトル: FAQその1",
            input);

        Assert.Contains(
            "本文その1",
            input);

        Assert.Contains(
            "--- FAQ ID: 2 ---",
            input);

        Assert.Contains(
            "タイトル: FAQその2",
            input);

        Assert.Contains(
            "本文その2",
            input);
    }

    [Fact]
    public async Task GenerateAsync_ReferenceBodyIsExactly4000Characters_DoesNotTruncateBody()
    {
        // Arrange
        var handler =
            CreateJsonResponseHandler(
                HttpStatusCode.OK,
                """
                {
                  "output_text": "回答です。"
                }
                """);

        var body =
            new string(
                'あ',
                4_000);

        var reference =
            CreateReference(
                body: body);

        var sut =
            CreateSut(
                handler,
                CreateSettings());

        // Act
        await sut.GenerateAsync(
            "質問",
            [reference]);

        // Assert
        var input =
            GetRequestInput(
                handler);

        Assert.Contains(
            body,
            input);

        Assert.DoesNotContain(
            body + "…",
            input);
    }

    [Fact]
    public async Task GenerateAsync_ReferenceBodyExceeds4000Characters_TruncatesBody()
    {
        // Arrange
        var handler =
            CreateJsonResponseHandler(
                HttpStatusCode.OK,
                """
                {
                  "output_text": "回答です。"
                }
                """);

        var first4000Characters =
            new string(
                'あ',
                4_000);

        var originalBody =
            first4000Characters
            + "この部分は送信されない";

        var reference =
            CreateReference(
                body: originalBody);

        var sut =
            CreateSut(
                handler,
                CreateSettings());

        // Act
        await sut.GenerateAsync(
            "質問",
            [reference]);

        // Assert
        var input =
            GetRequestInput(
                handler);

        Assert.Contains(
            first4000Characters + "…",
            input);

        Assert.DoesNotContain(
            "この部分は送信されない",
            input);
    }

    // =========================================================
    // Test helpers
    // =========================================================

    private static OpenAiAnswerGenerator CreateSut(
        HttpMessageHandler handler,
        OpenAiSettings settings)
    {
        var httpClient =
            new HttpClient(
                handler,
                disposeHandler: false)
            {
                BaseAddress =
                    new Uri(
                        "https://api.openai.com/v1/")
            };

        return new OpenAiAnswerGenerator(
            httpClient,
            settings,
            NullLogger<OpenAiAnswerGenerator>.Instance);
    }

    private static OpenAiSettings CreateSettings(
        string provider = "OpenAI",
        string apiKey = "test-api-key",
        string model = "gpt-test",
        int maxOutputTokens = 500)
    {
        return new OpenAiSettings
        {
            Provider = provider,
            ApiKey = apiKey,
            Model = model,
            MaxOutputTokens = maxOutputTokens
        };
    }

    private static AiFaqReference CreateReference(
        int id = 1,
        string title = "テストFAQ",
        string body = "テスト用FAQ本文です。",
        string categoryName = "テストカテゴリ",
        IReadOnlyList<string>? tags = null,
        int score = 100)
    {
        return new AiFaqReference(
            Id: id,
            Title: title,
            Body: body,
            CategoryName: categoryName,
            Tags:
                tags ??
                [
                    "テスト",
                    "FAQ"
                ],
            Score: score);
    }

    private static StubHttpMessageHandler
        CreateJsonResponseHandler(
            HttpStatusCode statusCode,
            string responseBody)
    {
        return new StubHttpMessageHandler(
            (_, _) =>
                Task.FromResult(
                    new HttpResponseMessage(
                        statusCode)
                    {
                        Content =
                            new StringContent(
                                responseBody,
                                Encoding.UTF8,
                                "application/json")
                    }));
    }

    private static string GetRequestInput(
        StubHttpMessageHandler handler)
    {
        var request =
            Assert.IsType<CapturedHttpRequest>(
                handler.LastRequest);

        using var document =
            JsonDocument.Parse(
                request.Body);

        var input =
            document.RootElement
                .GetProperty("input")
                .GetString();

        Assert.NotNull(input);

        return input;
    }

    private sealed class StubHttpMessageHandler(
        Func<
            HttpRequestMessage,
            CancellationToken,
            Task<HttpResponseMessage>> sendAsync)
        : HttpMessageHandler
    {
        private readonly Func<
            HttpRequestMessage,
            CancellationToken,
            Task<HttpResponseMessage>> _sendAsync =
                sendAsync;

        public int CallCount { get; private set; }

        public CapturedHttpRequest? LastRequest
        {
            get;
            private set;
        }

        protected override async Task<HttpResponseMessage>
            SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
        {
            CallCount++;

            var body =
                request.Content is null
                    ? string.Empty
                    : await request.Content
                        .ReadAsStringAsync(
                            cancellationToken);

            LastRequest =
                new CapturedHttpRequest(
                    request.Method,
                    request.RequestUri,
                    request.Headers.Authorization,
                    body);

            return await _sendAsync(
                request,
                cancellationToken);
        }
    }

    private sealed record CapturedHttpRequest(
        HttpMethod Method,
        Uri? RequestUri,
        AuthenticationHeaderValue? Authorization,
        string Body);
}
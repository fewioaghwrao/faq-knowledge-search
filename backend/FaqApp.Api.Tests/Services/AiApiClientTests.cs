using FaqApp.Api.Services;
using FaqApp.Api.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;

namespace FaqApp.Api.Tests.Services;

public class AiApiClientTests
{
    [Fact]
    public async Task GenerateAnswerAsync_ApiKeyIsEmpty_ThrowsInvalidOperationException()
    {
        // Arrange
        var client = CreateClient(new AiSettings
        {
            ApiKey = "",
            Endpoint = "https://example.com/v1/responses",
            Model = "gpt-5.4-mini"
        });

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            client.GenerateAnswerAsync(
                "ログインできない場合は？",
                new[] { "パスワード再設定を確認してください。" }));

        Assert.Equal("AI APIキーが設定されていません。", ex.Message);
    }

    [Fact]
    public async Task GenerateAnswerAsync_EndpointIsEmpty_ThrowsInvalidOperationException()
    {
        // Arrange
        var client = CreateClient(new AiSettings
        {
            ApiKey = "dummy-api-key",
            Endpoint = "",
            Model = "gpt-5.4-mini"
        });

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            client.GenerateAnswerAsync(
                "ログインできない場合は？",
                new[] { "パスワード再設定を確認してください。" }));

        Assert.Equal("AI APIエンドポイントが設定されていません。", ex.Message);
    }

    [Fact]
    public async Task GenerateAnswerAsync_SuccessResponse_ReturnsAnswer()
    {
        // Arrange
        var responseJson = """
        {
          "output": [
            {
              "content": [
                {
                  "type": "output_text",
                  "text": "パスワード再設定を確認してください。詳細は参照元FAQをご確認ください。"
                }
              ]
            }
          ]
        }
        """;

        var handler = new FakeHttpMessageHandler(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
            });

        var client = CreateClient(
            new AiSettings
            {
                ApiKey = "dummy-api-key",
                Endpoint = "https://example.com/v1/responses",
                Model = "gpt-5.4-mini"
            },
            handler);

        // Act
        var result = await client.GenerateAnswerAsync(
            "ログインできない場合は？",
            new[] { "パスワード再設定を確認してください。" });

        // Assert
        Assert.Equal(
            "パスワード再設定を確認してください。詳細は参照元FAQをご確認ください。",
            result);
    }

    [Fact]
    public async Task GenerateAnswerAsync_ApiReturnsError_ThrowsInvalidOperationException()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler(
            new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent(
                    """{ "error": "bad request" }""",
                    Encoding.UTF8,
                    "application/json")
            });

        var client = CreateClient(
            new AiSettings
            {
                ApiKey = "dummy-api-key",
                Endpoint = "https://example.com/v1/responses",
                Model = "gpt-5.4-mini"
            },
            handler);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            client.GenerateAnswerAsync(
                "ログインできない場合は？",
                new[] { "FAQ本文" }));

        Assert.Equal("AI回答の生成に失敗しました。", ex.Message);
    }

    [Fact]
    public async Task GenerateAnswerAsync_OutputTextIsMissing_ThrowsInvalidOperationException()
    {
        // Arrange
        var responseJson = """
        {
          "output": [
            {
              "content": [
                {
                  "type": "message",
                  "text": "これは対象外です"
                }
              ]
            }
          ]
        }
        """;

        var handler = new FakeHttpMessageHandler(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
            });

        var client = CreateClient(
            new AiSettings
            {
                ApiKey = "dummy-api-key",
                Endpoint = "https://example.com/v1/responses",
                Model = "gpt-5.4-mini"
            },
            handler);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            client.GenerateAnswerAsync(
                "ログインできない場合は？",
                new[] { "FAQ本文" }));

        Assert.Equal("AI回答が空でした。", ex.Message);
    }

    [Fact]
    public async Task GenerateAnswerAsync_SendsBearerTokenAndPrompt()
    {
        // Arrange
        var responseJson = """
        {
          "output": [
            {
              "content": [
                {
                  "type": "output_text",
                  "text": "回答本文"
                }
              ]
            }
          ]
        }
        """;

        var handler = new FakeHttpMessageHandler(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
            });

        var client = CreateClient(
            new AiSettings
            {
                ApiKey = "dummy-api-key",
                Endpoint = "https://example.com/v1/responses",
                Model = "gpt-5.4-mini"
            },
            handler);

        // Act
        await client.GenerateAnswerAsync(
            "ログインできない場合は？",
            new[] { "パスワード再設定を確認してください。" });

        // Assert
        Assert.NotNull(handler.LastRequest);
        Assert.Equal("Bearer", handler.LastRequest!.Headers.Authorization?.Scheme);
        Assert.Equal("dummy-api-key", handler.LastRequest.Headers.Authorization?.Parameter);

        var requestJson = handler.LastRequestBody;

        using var document = JsonDocument.Parse(requestJson);
        var root = document.RootElement;

        Assert.Equal("gpt-5.4-mini", root.GetProperty("model").GetString());

        var input = root.GetProperty("input");

        var userMessage = input.EnumerateArray()
            .First(x => x.GetProperty("role").GetString() == "user");

        var userContent = userMessage.GetProperty("content").GetString();

        Assert.Contains("ログインできない場合は？", userContent);
        Assert.Contains("パスワード再設定を確認してください。", userContent);
    }

    private static AiApiClient CreateClient(
        AiSettings settings,
        FakeHttpMessageHandler? handler = null)
    {
        handler ??= new FakeHttpMessageHandler(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """
                    {
                      "output": [
                        {
                          "content": [
                            {
                              "type": "output_text",
                              "text": "回答本文"
                            }
                          ]
                        }
                      ]
                    }
                    """,
                    Encoding.UTF8,
                    "application/json")
            });

        var httpClient = new HttpClient(handler);

        var options = Options.Create(settings);

        var logger = Mock.Of<ILogger<AiApiClient>>();

        return new AiApiClient(httpClient, options, logger);
    }

    private sealed class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpResponseMessage _response;

        public HttpRequestMessage? LastRequest { get; private set; }

        public string LastRequestBody { get; private set; } = string.Empty;

        public FakeHttpMessageHandler(HttpResponseMessage response)
        {
            _response = response;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            LastRequest = request;

            if (request.Content is not null)
            {
                LastRequestBody = await request.Content.ReadAsStringAsync(cancellationToken);
            }

            return _response;
        }
    }
}
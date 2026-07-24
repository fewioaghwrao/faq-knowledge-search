using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace FaqKnowledgeSearch.Razor.Tests.Pages;

public sealed class StatusCodePageTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public StatusCodePageTests(
        WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Get404_ReturnsNotFoundAndDisplaysExpectedContent()
    {
        // Act
        var response =
            await _client.GetAsync("/StatusCode/404");

        var html =
            await response.Content.ReadAsStringAsync();

        var decodedHtml =
            WebUtility.HtmlDecode(html);

        // Assert
        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);

        Assert.Contains(
            "404 Not Found",
            decodedHtml);

        Assert.Contains(
            "ページが見つかりません",
            decodedHtml);

        Assert.Contains(
            "ホームへ戻る",
            decodedHtml);

        Assert.Contains(
            "FAQ検索を開く",
            decodedHtml);
    }
}
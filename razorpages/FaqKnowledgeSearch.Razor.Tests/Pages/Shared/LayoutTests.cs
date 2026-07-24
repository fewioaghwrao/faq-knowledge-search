using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace FaqKnowledgeSearch.Razor.Tests.Pages.Shared;

public sealed class LayoutTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public LayoutTests(
        WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Get_WhenAnonymous_DisplaysLoginLink()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        Assert.Contains("Knowledge FAQ", html);
        Assert.Contains("ホーム", html);
        Assert.Contains("FAQ検索", html);
        Assert.Contains("AI検索", html);

        Assert.Contains("ログイン", html);
        Assert.Contains("/Account/Login", html);

        Assert.DoesNotContain(
            "data-bs-target=\"#logoutConfirmModal\"",
            html);

        Assert.DoesNotContain(
            "id=\"logoutConfirmModal\"",
            html);

        Assert.DoesNotContain(
            ">管理画面<",
            html);
    }
}
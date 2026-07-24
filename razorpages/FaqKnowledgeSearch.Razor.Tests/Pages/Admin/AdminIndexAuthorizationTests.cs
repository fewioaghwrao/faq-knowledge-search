using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.WebUtilities;
using System.Net;

namespace FaqKnowledgeSearch.Razor.Tests.Pages.Admin;

public sealed class AdminIndexAuthorizationTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AdminIndexAuthorizationTests(
        WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

[Fact]
public async Task Get_WhenUnauthenticated_RedirectsToLogin()
{
    // Arrange
    var client = _factory.CreateClient(
        new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

    // Act
    var response = await client.GetAsync("/Admin");

    // Assert
    Assert.Equal(
        HttpStatusCode.Redirect,
        response.StatusCode);

    var location = Assert.IsType<Uri>(
        response.Headers.Location);

    Assert.Equal(
        "/Account/Login",
        location.AbsolutePath);

    var query =
        QueryHelpers.ParseQuery(location.Query);

    Assert.True(query.TryGetValue(
        "ReturnUrl",
        out var returnUrl));

    Assert.Equal(
        "/Admin",
        returnUrl.ToString());
}
}

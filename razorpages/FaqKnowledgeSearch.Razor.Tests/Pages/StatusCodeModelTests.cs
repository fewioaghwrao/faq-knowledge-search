using FaqKnowledgeSearch.Razor.Pages;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;

namespace FaqKnowledgeSearch.Razor.Tests.Pages;

public sealed class StatusCodeModelTests
{
    [Theory]
    [InlineData(
        400,
        "Bad Request",
        "リクエストを処理できませんでした",
        "送信された内容に問題があります。入力内容を確認して、もう一度お試しください。")]
    [InlineData(
        401,
        "Unauthorized",
        "ログインが必要です",
        "このページを利用するには、ログインが必要です。")]
    [InlineData(
        403,
        "Forbidden",
        "アクセス権限がありません",
        "このページを表示する権限がありません。必要な場合はシステム管理者へお問い合わせください。")]
    [InlineData(
        404,
        "Not Found",
        "ページが見つかりません",
        "指定されたページは削除されたか、URLが変更された可能性があります。")]
    [InlineData(
        405,
        "Method Not Allowed",
        "許可されていない操作です",
        "このURLでは、指定された操作方法を利用できません。")]
    [InlineData(
        429,
        "Too Many Requests",
        "アクセス回数の上限に達しました",
        "短時間にアクセスが集中しています。少し時間を置いてから再度お試しください。")]
    [InlineData(
        500,
        "Internal Server Error",
        "サーバーでエラーが発生しました",
        "一時的な問題の可能性があります。時間を置いてから再度お試しください。")]
    [InlineData(
        503,
        "Service Unavailable",
        "現在サービスを利用できません",
        "メンテナンス中または一時的に混雑しています。時間を置いてから再度お試しください。")]
    public void OnGet_WhenKnownStatusCode_SetsExpectedContent(
        int statusCode,
        string expectedLabel,
        string expectedTitle,
        string expectedDescription)
    {
        // Arrange
        var sut = CreateSut();

        // Act
        sut.OnGet(statusCode);

        // Assert
        Assert.Equal(statusCode, sut.Code);
        Assert.Equal(statusCode, sut.Response.StatusCode);
        Assert.Equal(expectedLabel, sut.Label);
        Assert.Equal(expectedTitle, sut.TitleText);
        Assert.Equal(expectedDescription, sut.Description);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(200)]
    [InlineData(399)]
    [InlineData(600)]
    [InlineData(999)]
    public void OnGet_WhenStatusCodeIsOutsideErrorRange_UsesStatus500(
        int statusCode)
    {
        // Arrange
        var sut = CreateSut();

        // Act
        sut.OnGet(statusCode);

        // Assert
        Assert.Equal(
            StatusCodes.Status500InternalServerError,
            sut.Code);

        Assert.Equal(
            StatusCodes.Status500InternalServerError,
            sut.Response.StatusCode);

        Assert.Equal(
            "Internal Server Error",
            sut.Label);

        Assert.Equal(
            "サーバーでエラーが発生しました",
            sut.TitleText);
    }

    [Fact]
    public void OnGet_WhenStatusCodeIsValidButNotDefined_UsesGenericContent()
    {
        // Arrange
        const int statusCode = 418;

        var sut = CreateSut();

        // Act
        sut.OnGet(statusCode);

        // Assert
        Assert.Equal(418, sut.Code);
        Assert.Equal(418, sut.Response.StatusCode);

        Assert.Equal(
            "HTTP Error",
            sut.Label);

        Assert.Equal(
            "要求を処理できませんでした",
            sut.TitleText);

        Assert.Equal(
            "予期しないHTTPエラーが発生しました。",
            sut.Description);
    }

    [Fact]
    public void OnGet_WhenReExecuteFeatureExists_SetsOriginalPath()
    {
        // Arrange
        const string originalPath =
            "/not-found-page";

        var feature =
            new Mock<IStatusCodeReExecuteFeature>();

        feature
            .SetupGet(x => x.OriginalPath)
            .Returns(originalPath);

        var sut = CreateSut();

        sut.HttpContext.Features.Set(
            feature.Object);

        // Act
        sut.OnGet(
            StatusCodes.Status404NotFound);

        // Assert
        Assert.Equal(
            originalPath,
            sut.OriginalPath);
    }

    [Fact]
    public void OnGet_WhenReExecuteFeatureDoesNotExist_OriginalPathIsNull()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        sut.OnGet(
            StatusCodes.Status404NotFound);

        // Assert
        Assert.Null(sut.OriginalPath);
    }

    private static StatusCodeModel CreateSut()
    {
        var httpContext =
            new DefaultHttpContext();

        return new StatusCodeModel
        {
            PageContext = new PageContext
            {
                HttpContext = httpContext
            }
        };
    }
}

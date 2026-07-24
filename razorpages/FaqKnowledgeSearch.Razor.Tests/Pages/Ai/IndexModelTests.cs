using FaqKnowledgeSearch.Application.AiSearch;
using FaqKnowledgeSearch.Application.AiSearch.Feedback;
using FaqKnowledgeSearch.Razor.Pages.Ai;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using System.Globalization;
using System.Text.Json;

namespace FaqKnowledgeSearch.Razor.Tests.Pages.Ai;

public sealed class IndexModelTests
{
    private const string FeedbackPurpose =
        "FaqKnowledgeSearch.AiFeedback.v1";

    [Theory]
    [InlineData(false, 60, false, 60)]
    [InlineData(true, 30, true, 30)]
    [InlineData(true, 0, true, 1)]
    [InlineData(true, -10, true, 1)]
    [InlineData(true, 61, true, 60)]
    [InlineData(true, 100, true, 60)]
    public void OnGet_SetsRateLimitInformation(
        bool rateLimited,
        int retryAfter,
        bool expectedRateLimited,
        int expectedRetryAfter)
    {
        // Arrange
        var sut = CreateSut();

        // Act
        sut.OnGet(
            rateLimited,
            retryAfter);

        // Assert
        Assert.Equal(
            expectedRateLimited,
            sut.IsRateLimited);

        Assert.Equal(
            expectedRetryAfter,
            sut.RetryAfterSeconds);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public async Task OnPostAsync_WhenQuestionIsEmpty_AddsErrorWithoutSearching(
        string question)
    {
        // Arrange
        var searchService =
            new Mock<IAiFaqSearchService>();

        var sut = CreateSut(
            searchService: searchService);

        sut.Input.Question = question;

        // Act
        var result = await sut.OnPostAsync(
            CancellationToken.None);

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.Equal(string.Empty, sut.Input.Question);

        Assert.Contains(
            sut.ModelState["Input.Question"]!.Errors,
            error =>
                error.ErrorMessage ==
                "質問・検索キーワードを入力してください。");

        searchService.Verify(
            x => x.SearchAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task OnPostAsync_TrimsQuestionBeforeSearching()
    {
        // Arrange
        var searchService =
            new Mock<IAiFaqSearchService>();

        /*
         * CreateSearchResultは、実際のAiFaqSearchResultの
         * コンストラクター定義に合わせて実装してください。
         */
        var searchResult =
            CreateSearchResult(
                usedExternalAi: false);

        searchService
            .Setup(x => x.SearchAsync(
                "CSV取込エラー",
                CancellationToken.None))
            .ReturnsAsync(searchResult);

        var sut = CreateSut(
            searchService: searchService);

        sut.Input.Question =
            "  CSV取込エラー  ";

        // Act
        var result = await sut.OnPostAsync(
            CancellationToken.None);

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.Equal(
            "CSV取込エラー",
            sut.Input.Question);

        Assert.Same(
            searchResult,
            sut.Result);

        searchService.Verify(
            x => x.SearchAsync(
                "CSV取込エラー",
                CancellationToken.None),
            Times.Once);
    }

    [Fact]
    public async Task OnPostAsync_WhenArgumentExceptionOccurs_AddsQuestionError()
    {
        // Arrange
        const string errorMessage =
            "質問が長すぎます。";

        var searchService =
            new Mock<IAiFaqSearchService>();

        searchService
            .Setup(x => x.SearchAsync(
                It.IsAny<string>(),
                CancellationToken.None))
            .ThrowsAsync(
                new ArgumentException(errorMessage));

        var sut = CreateSut(
            searchService: searchService);

        sut.Input.Question =
            "テスト質問";

        // Act
        var result = await sut.OnPostAsync(
            CancellationToken.None);

        // Assert
        Assert.IsType<PageResult>(result);

        Assert.Contains(
            sut.ModelState["Input.Question"]!.Errors,
            error =>
                error.ErrorMessage == errorMessage);

        Assert.Null(sut.Result);
        Assert.Null(sut.FeedbackToken);
    }

    [Fact]
    public async Task OnPostFeedbackAsync_WhenTokenIsEmpty_ReturnsBadRequest()
    {
        // Arrange
        var feedbackService =
            new Mock<IAiSearchFeedbackService>();

        var sut = CreateSut(
            feedbackService: feedbackService);

        // Act
        var result =
            await sut.OnPostFeedbackAsync(
                feedbackToken: string.Empty,
                wasHelpful: true,
                CancellationToken.None);

        // Assert
        var jsonResult =
            Assert.IsType<JsonResult>(result);

        Assert.Equal(
            400,
            jsonResult.StatusCode);

        var body = GetJsonBody(jsonResult);

        Assert.False(
            body.GetProperty("success")
                .GetBoolean());

        Assert.Equal(
            "フィードバック情報が正しくありません。",
            body.GetProperty("message")
                .GetString());

        feedbackService.Verify(
            x => x.SetFeedbackAsync(
                It.IsAny<long>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task OnPostFeedbackAsync_WhenTokenIsInvalid_ReturnsBadRequest()
    {
        // Arrange
        var feedbackService =
            new Mock<IAiSearchFeedbackService>();

        var sut = CreateSut(
            feedbackService: feedbackService);

        // Act
        var result =
            await sut.OnPostFeedbackAsync(
                feedbackToken: "invalid-token",
                wasHelpful: false,
                CancellationToken.None);

        // Assert
        var jsonResult =
            Assert.IsType<JsonResult>(result);

        Assert.Equal(400, jsonResult.StatusCode);

        feedbackService.Verify(
            x => x.SetFeedbackAsync(
                It.IsAny<long>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("abc")]
    public async Task OnPostFeedbackAsync_WhenProtectedValueIsInvalid_ReturnsBadRequest(
        string protectedValue)
    {
        // Arrange
        var provider =
            new EphemeralDataProtectionProvider();

        var protector =
            provider.CreateProtector(
                FeedbackPurpose);

        var token =
            protector.Protect(protectedValue);

        var feedbackService =
            new Mock<IAiSearchFeedbackService>();

        var sut = CreateSut(
            feedbackService: feedbackService,
            dataProtectionProvider: provider);

        // Act
        var result =
            await sut.OnPostFeedbackAsync(
                token,
                wasHelpful: true,
                CancellationToken.None);

        // Assert
        var jsonResult =
            Assert.IsType<JsonResult>(result);

        Assert.Equal(400, jsonResult.StatusCode);

        feedbackService.Verify(
            x => x.SetFeedbackAsync(
                It.IsAny<long>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task OnPostFeedbackAsync_WhenSuccessful_ReturnsSuccessJson(
        bool wasHelpful)
    {
        // Arrange
        const long historyId = 123;

        var provider =
            new EphemeralDataProtectionProvider();

        var token =
            provider
                .CreateProtector(FeedbackPurpose)
                .Protect(
                    historyId.ToString(
                        CultureInfo.InvariantCulture));

        var feedbackService =
            new Mock<IAiSearchFeedbackService>();

        feedbackService
            .Setup(x => x.SetFeedbackAsync(
                historyId,
                wasHelpful,
                CancellationToken.None))
            .ReturnsAsync(
                AiSearchFeedbackResult.Success);

        var sut = CreateSut(
            feedbackService: feedbackService,
            dataProtectionProvider: provider);

        // Act
        var result =
            await sut.OnPostFeedbackAsync(
                token,
                wasHelpful,
                CancellationToken.None);

        // Assert
        var jsonResult =
            Assert.IsType<JsonResult>(result);

        // StatusCode未指定なので、実際には200扱い
        Assert.Null(jsonResult.StatusCode);

        var body = GetJsonBody(jsonResult);

        Assert.True(
            body.GetProperty("success")
                .GetBoolean());

        Assert.Equal(
            wasHelpful,
            body.GetProperty("wasHelpful")
                .GetBoolean());

        Assert.Equal(
            "フィードバックを登録しました。",
            body.GetProperty("message")
                .GetString());

        feedbackService.Verify(
            x => x.SetFeedbackAsync(
                historyId,
                wasHelpful,
                CancellationToken.None),
            Times.Once);
    }

    [Fact]
    public async Task OnPostFeedbackAsync_WhenHistoryDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        const long historyId = 123;

        var provider =
            new EphemeralDataProtectionProvider();

        var token =
            provider
                .CreateProtector(FeedbackPurpose)
                .Protect(
                    historyId.ToString(
                        CultureInfo.InvariantCulture));

        var feedbackService =
            new Mock<IAiSearchFeedbackService>();

        feedbackService
            .Setup(x => x.SetFeedbackAsync(
                historyId,
                true,
                CancellationToken.None))
            .ReturnsAsync(
                AiSearchFeedbackResult.NotFound);

        var sut = CreateSut(
            feedbackService: feedbackService,
            dataProtectionProvider: provider);

        // Act
        var result =
            await sut.OnPostFeedbackAsync(
                token,
                wasHelpful: true,
                CancellationToken.None);

        // Assert
        var jsonResult =
            Assert.IsType<JsonResult>(result);

        Assert.Equal(
            404,
            jsonResult.StatusCode);

        var body = GetJsonBody(jsonResult);

        Assert.False(
            body.GetProperty("success")
                .GetBoolean());

        Assert.Equal(
            "対象のAI検索履歴が見つかりませんでした。",
            body.GetProperty("message")
                .GetString());
    }

    private static IndexModel CreateSut(
        Mock<IAiFaqSearchService>? searchService = null,
        Mock<IAiSearchFeedbackService>? feedbackService = null,
        IDataProtectionProvider? dataProtectionProvider = null)
    {
        searchService ??=
            new Mock<IAiFaqSearchService>();

        feedbackService ??=
            new Mock<IAiSearchFeedbackService>();

        dataProtectionProvider ??=
            new EphemeralDataProtectionProvider();

        return new IndexModel(
            searchService.Object,
            feedbackService.Object,
            dataProtectionProvider);
    }

    private static string Serialize(
        JsonResult result)
    {
        return JsonSerializer.Serialize(
            result.Value);
    }

    private static AiFaqSearchResult CreateSearchResult(
        bool usedExternalAi,
        long historyId = 123)
    {
        return new AiFaqSearchResult(
            HistoryId: historyId,
            Question: "CSV取込エラー",
            Answer: "文字コードとファイル形式を確認してください。",
            References: [],
            UsedExternalAi: usedExternalAi);
    }

    private static JsonElement GetJsonBody(
    JsonResult result)
    {
        var json = JsonSerializer.Serialize(
            result.Value);

        using var document =
            JsonDocument.Parse(json);

        return document.RootElement.Clone();
    }
}
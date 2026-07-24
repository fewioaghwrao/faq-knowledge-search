using FaqKnowledgeSearch.Application.AiSearch.History.Admin;
using FaqKnowledgeSearch.Razor.Pages.Admin.AiSearchHistories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;

namespace FaqKnowledgeSearch.Razor.Tests.Pages.Admin
    .AiSearchHistories;

public sealed class DetailsModelTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task OnGetAsync_WhenIdIsNotPositive_ReturnsNotFound(
        long id)
    {
        // Arrange
        var historyQuery =
            new Mock<IAdminAiSearchHistoryQuery>();

        var sut = new DetailsModel(historyQuery.Object)
        {
            Id = id
        };

        // Act
        var result = await sut.OnGetAsync(
            CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundResult>(result);

        historyQuery.Verify(
            x => x.GetDetailAsync(
                It.IsAny<long>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task OnGetAsync_WhenHistoryDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        const long historyId = 10;

        var historyQuery =
            new Mock<IAdminAiSearchHistoryQuery>();

        historyQuery
            .Setup(x => x.GetDetailAsync(
                historyId,
                CancellationToken.None))
            .ReturnsAsync(
                (AdminAiSearchHistoryDetail?)null);

        var sut = new DetailsModel(historyQuery.Object)
        {
            Id = historyId
        };

        // Act
        var result = await sut.OnGetAsync(
            CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundResult>(result);

        historyQuery.Verify(
            x => x.GetDetailAsync(
                historyId,
                CancellationToken.None),
            Times.Once);
    }

    [Fact]
    public async Task OnGetAsync_WhenHistoryExists_SetsHistoryAndReturnsPage()
    {
        // Arrange
        const long historyId = 10;

        var expectedHistory = CreateHistoryDetail(
            id: historyId);

        var historyQuery =
            new Mock<IAdminAiSearchHistoryQuery>();

        historyQuery
            .Setup(x => x.GetDetailAsync(
                historyId,
                CancellationToken.None))
            .ReturnsAsync(expectedHistory);

        var sut = new DetailsModel(historyQuery.Object)
        {
            Id = historyId
        };

        // Act
        var result = await sut.OnGetAsync(
            CancellationToken.None);

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.Same(expectedHistory, sut.History);

        historyQuery.Verify(
            x => x.GetDetailAsync(
                historyId,
                CancellationToken.None),
            Times.Once);
    }

    [Fact]
    public async Task OnGetAsync_ForwardsCancellationToken()
    {
        // Arrange
        const long historyId = 10;

        using var cancellationTokenSource =
            new CancellationTokenSource();

        var cancellationToken =
            cancellationTokenSource.Token;

        var expectedHistory = CreateHistoryDetail(
            id: historyId);

        var historyQuery =
            new Mock<IAdminAiSearchHistoryQuery>();

        historyQuery
            .Setup(x => x.GetDetailAsync(
                historyId,
                cancellationToken))
            .ReturnsAsync(expectedHistory);

        var sut = new DetailsModel(historyQuery.Object)
        {
            Id = historyId
        };

        // Act
        await sut.OnGetAsync(cancellationToken);

        // Assert
        historyQuery.Verify(
            x => x.GetDetailAsync(
                historyId,
                cancellationToken),
            Times.Once);
    }

    private static AdminAiSearchHistoryDetail CreateHistoryDetail(
        long id,
        bool isSuccess = true)
    {
        return new AdminAiSearchHistoryDetail(
            Id: id,
            Question: "パスワードを変更する方法は？",
            Answer: isSuccess
                ? "管理画面から変更できます。"
                : null,
            IsSuccess: isSuccess,
            ErrorMessage: isSuccess
                ? null
                : "AI回答の生成に失敗しました。",
            ModelName: "test-model",
            UsedExternalAi: false,
            WasHelpful: true,
            CreatedAt: DateTime.UtcNow,
            References: Array.Empty<
                AdminAiSearchHistoryReferenceItem>());
    }
}
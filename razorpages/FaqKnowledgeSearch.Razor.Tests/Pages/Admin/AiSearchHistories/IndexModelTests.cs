using FaqKnowledgeSearch.Application.AiSearch.History.Admin;
using FaqKnowledgeSearch.Application.Common.Models;
using FaqKnowledgeSearch.Razor.Pages.Admin.AiSearchHistories;
using Moq;

namespace FaqKnowledgeSearch.Razor.Tests.Pages.Admin
    .AiSearchHistories;

public sealed class IndexModelTests
{
    [Fact]
    public async Task OnGetAsync_WhenPageNumberIsZero_NormalizesToOne()
    {
        // Arrange
        var expectedResult = CreateResult(
            totalCount: 0,
            page: 1);

        var query =
            new Mock<IAdminAiSearchHistoryQuery>();

        query
            .Setup(x => x.SearchAsync(
                It.IsAny<
                    AdminAiSearchHistorySearchCondition>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var sut = new IndexModel(query.Object)
        {
            PageNumber = 0
        };

        // Act
        await sut.OnGetAsync(
            CancellationToken.None);

        // Assert
        Assert.Equal(1, sut.PageNumber);
        Assert.Same(expectedResult, sut.Result);

        query.Verify(
            x => x.SearchAsync(
                It.Is<
                    AdminAiSearchHistorySearchCondition>(
                    condition =>
                        condition.Page == 1
                        && condition.PageSize == 10),
                CancellationToken.None),
            Times.Once);
    }

    [Fact]
    public async Task OnGetAsync_PassesFiltersToQuery()
    {
        // Arrange
        var expectedResult = CreateResult(
            totalCount: 1,
            page: 2);

        var query =
            new Mock<IAdminAiSearchHistoryQuery>();

        query
            .Setup(x => x.SearchAsync(
                It.IsAny<
                    AdminAiSearchHistorySearchCondition>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var sut = new IndexModel(query.Object)
        {
            Keyword = "ログイン",
            Status =
                AdminAiSearchStatusFilter.Failure,
            Feedback =
                AdminAiSearchFeedbackFilter.NotHelpful,
            PageNumber = 2
        };

        // Act
        await sut.OnGetAsync(
            CancellationToken.None);

        // Assert
        query.Verify(
            x => x.SearchAsync(
                It.Is<
                    AdminAiSearchHistorySearchCondition>(
                    condition =>
                        condition.Keyword == "ログイン"
                        && condition.Status
                            == AdminAiSearchStatusFilter
                                .Failure
                        && condition.Feedback
                            == AdminAiSearchFeedbackFilter
                                .NotHelpful
                        && condition.Page == 2
                        && condition.PageSize == 10),
                CancellationToken.None),
            Times.Once);
    }

    [Fact]
    public async Task OnGetAsync_WhenPageExceedsLastPage_SearchesLastPageAgain()
    {
        // Arrange
        // 12件・10件単位なので最終ページは2
        var firstResult = CreateResult(
            totalCount: 12,
            page: 5);

        var correctedResult = CreateResult(
            totalCount: 12,
            page: 2);

        var requestedPages = new List<int>();

        var query =
            new Mock<IAdminAiSearchHistoryQuery>();

        query
            .Setup(x => x.SearchAsync(
                It.IsAny<
                    AdminAiSearchHistorySearchCondition>(),
                It.IsAny<CancellationToken>()))
            .Callback<
                AdminAiSearchHistorySearchCondition,
                CancellationToken>(
                (condition, _) =>
                    requestedPages.Add(condition.Page))
            .ReturnsAsync((
                AdminAiSearchHistorySearchCondition condition,
                CancellationToken _) =>
                condition.Page == 5
                    ? firstResult
                    : correctedResult);

        var sut = new IndexModel(query.Object)
        {
            PageNumber = 5
        };

        // Act
        await sut.OnGetAsync(
            CancellationToken.None);

        // Assert
        Assert.Equal(2, sut.PageNumber);
        Assert.Same(correctedResult, sut.Result);
        Assert.Equal([5, 2], requestedPages);

        query.Verify(
            x => x.SearchAsync(
                It.IsAny<
                    AdminAiSearchHistorySearchCondition>(),
                CancellationToken.None),
            Times.Exactly(2));
    }

    [Fact]
    public async Task OnGetAsync_WhenPageIsValid_SearchesOnlyOnce()
    {
        // Arrange
        var result = CreateResult(
            totalCount: 25,
            page: 2);

        var query =
            new Mock<IAdminAiSearchHistoryQuery>();

        query
            .Setup(x => x.SearchAsync(
                It.IsAny<
                    AdminAiSearchHistorySearchCondition>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        var sut = new IndexModel(query.Object)
        {
            PageNumber = 2
        };

        // Act
        await sut.OnGetAsync(
            CancellationToken.None);

        // Assert
        Assert.Equal(2, sut.PageNumber);
        Assert.Same(result, sut.Result);

        query.Verify(
            x => x.SearchAsync(
                It.IsAny<
                    AdminAiSearchHistorySearchCondition>(),
                CancellationToken.None),
            Times.Once);
    }

    [Fact]
    public async Task OnGetAsync_ForwardsCancellationToken()
    {
        // Arrange
        using var cancellationTokenSource =
            new CancellationTokenSource();

        var token =
            cancellationTokenSource.Token;

        var query =
            new Mock<IAdminAiSearchHistoryQuery>();

        query
            .Setup(x => x.SearchAsync(
                It.IsAny<
                    AdminAiSearchHistorySearchCondition>(),
                token))
            .ReturnsAsync(CreateResult(
                totalCount: 0,
                page: 1));

        var sut = new IndexModel(query.Object);

        // Act
        await sut.OnGetAsync(token);

        // Assert
        query.Verify(
            x => x.SearchAsync(
                It.IsAny<
                    AdminAiSearchHistorySearchCondition>(),
                token),
            Times.Once);
    }

    [Fact]
    public void DisplayTotalPages_WhenResultIsEmpty_ReturnsOne()
    {
        // Arrange
        var query =
            new Mock<IAdminAiSearchHistoryQuery>();

        var sut = new IndexModel(query.Object);

        // Act
        var result = sut.DisplayTotalPages;

        // Assert
        Assert.Equal(1, result);
    }

    private static PagedResult<
        AdminAiSearchHistoryListItem> CreateResult(
        int totalCount,
        int page)
    {
        return new PagedResult<
            AdminAiSearchHistoryListItem>(
            Items: [],
            TotalCount: totalCount,
            Page: page,
            PageSize: 10);
    }
}

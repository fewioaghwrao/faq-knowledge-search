using FaqKnowledgeSearch.Application.Common.Models;
using FaqKnowledgeSearch.Application.Faqs.Public;
using FaqKnowledgeSearch.Razor.Pages.Faqs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;

namespace FaqKnowledgeSearch.Razor.Tests.Pages.Faqs;

public sealed class IndexModelTests
{
    [Fact]
    public async Task OnGetAsync_PassesSearchConditionToService()
    {
        // Arrange
        using var cancellationTokenSource =
            new CancellationTokenSource();

        var cancellationToken =
            cancellationTokenSource.Token;

        PublicFaqSearchCondition? capturedCondition = null;

        var expectedResult = CreateResult(
            totalCount: 15,
            page: 2);

        var service =
            new Mock<IPublicFaqService>();

        service
            .Setup(x => x.SearchAsync(
                It.IsAny<PublicFaqSearchCondition>(),
                cancellationToken))
            .Callback<
                PublicFaqSearchCondition,
                CancellationToken>(
                (condition, _) =>
                    capturedCondition = condition)
            .ReturnsAsync(expectedResult);

        var sut = CreateSut(
            service,
            cancellationToken);

        sut.Keyword = "ログイン";
        sut.SortOrder = FaqSortOrder.MostViewed;
        sut.PageNumber = 2;

        // Act
        await sut.OnGetAsync();

        // Assert
        Assert.NotNull(capturedCondition);

        Assert.Equal(
            "ログイン",
            capturedCondition.Keyword);

        Assert.Equal(
            FaqSortOrder.MostViewed,
            capturedCondition.SortOrder);

        Assert.Equal(
            2,
            capturedCondition.Page);

        Assert.Equal(
            10,
            capturedCondition.PageSize);

        Assert.Same(
            expectedResult,
            sut.SearchResult);

        service.Verify(
            x => x.SearchAsync(
                It.IsAny<PublicFaqSearchCondition>(),
                cancellationToken),
            Times.Once);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task OnGetAsync_WhenServiceCorrectsPage_ReflectsCorrectedPageNumber(
        int requestedPage)
    {
        // Arrange
        var service =
            new Mock<IPublicFaqService>();

        var correctedResult = CreateResult(
            totalCount: 5,
            page: 1);

        service
            .Setup(x => x.SearchAsync(
                It.Is<
                    PublicFaqSearchCondition>(
                    condition =>
                        condition.Page == requestedPage),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(correctedResult);

        var sut = CreateSut(service);

        sut.PageNumber = requestedPage;

        // Act
        await sut.OnGetAsync();

        // Assert
        Assert.Equal(
            1,
            sut.PageNumber);

        Assert.Same(
            correctedResult,
            sut.SearchResult);
    }

    [Fact]
    public async Task OnGetAsync_ForwardsRequestAbortedToken()
    {
        // Arrange
        using var cancellationTokenSource =
            new CancellationTokenSource();

        var cancellationToken =
            cancellationTokenSource.Token;

        var service =
            new Mock<IPublicFaqService>();

        service
            .Setup(x => x.SearchAsync(
                It.IsAny<PublicFaqSearchCondition>(),
                cancellationToken))
            .ReturnsAsync(CreateResult(
                totalCount: 0,
                page: 1));

        var sut = CreateSut(
            service,
            cancellationToken);

        // Act
        await sut.OnGetAsync();

        // Assert
        service.Verify(
            x => x.SearchAsync(
                It.IsAny<PublicFaqSearchCondition>(),
                cancellationToken),
            Times.Once);
    }

    [Theory]
    [InlineData(
        FaqSortOrder.Relevance,
        "関連度順")]
    [InlineData(
        FaqSortOrder.Newest,
        "新着順")]
    [InlineData(
        FaqSortOrder.MostViewed,
        "閲覧数順")]
    public void GetSortOrderLabel_ReturnsExpectedLabel(
        FaqSortOrder sortOrder,
        string expectedLabel)
    {
        // Arrange
        var service =
            new Mock<IPublicFaqService>();

        var sut = CreateSut(service);

        sut.SortOrder = sortOrder;

        // Act
        var result =
            sut.GetSortOrderLabel();

        // Assert
        Assert.Equal(
            expectedLabel,
            result);
    }

    [Fact]
    public void GetSortOrderLabel_WhenValueIsUnknown_ReturnsRelevanceLabel()
    {
        // Arrange
        var service =
            new Mock<IPublicFaqService>();

        var sut = CreateSut(service);

        sut.SortOrder =
            (FaqSortOrder)999;

        // Act
        var result =
            sut.GetSortOrderLabel();

        // Assert
        Assert.Equal(
            "関連度順",
            result);
    }

    private static IndexModel CreateSut(
        Mock<IPublicFaqService> service,
        CancellationToken requestAborted = default)
    {
        var httpContext =
            new DefaultHttpContext();

        httpContext.RequestAborted =
            requestAborted;

        return new IndexModel(
            service.Object)
        {
            PageContext = new PageContext
            {
                HttpContext = httpContext
            }
        };
    }

    private static PagedResult<
        PublicFaqSearchItem> CreateResult(
        int totalCount,
        int page)
    {
        return new PagedResult<
            PublicFaqSearchItem>(
            Items: [],
            TotalCount: totalCount,
            Page: page,
            PageSize: 10);
    }
}

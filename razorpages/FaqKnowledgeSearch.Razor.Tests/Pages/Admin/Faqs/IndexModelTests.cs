using FaqKnowledgeSearch.Application.Common.Models;
using FaqKnowledgeSearch.Application.Faqs.Admin;
using FaqKnowledgeSearch.Razor.Pages.Admin.Faqs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace FaqKnowledgeSearch.Razor.Tests.Pages.Admin.Faqs;

public sealed class IndexModelTests
{
    [Fact]
    public async Task OnGetAsync_WhenPageNumberIsZero_NormalizesToOne()
    {
        // Arrange
        var expectedResult = CreateResult(
            totalCount: 0,
            page: 1);

        var adminFaqQuery = new Mock<IAdminFaqQuery>();
        var adminFaqService = new Mock<IAdminFaqService>();

        adminFaqQuery
            .Setup(x => x.SearchAsync(
                1,
                5,
                CancellationToken.None))
            .ReturnsAsync(expectedResult);

        var sut = CreateSut(
            adminFaqQuery,
            adminFaqService);

        sut.PageNumber = 0;

        // Act
        await sut.OnGetAsync(CancellationToken.None);

        // Assert
        Assert.Equal(1, sut.PageNumber);
        Assert.Same(expectedResult, sut.Result);

        adminFaqQuery.Verify(
            x => x.SearchAsync(
                1,
                5,
                CancellationToken.None),
            Times.Once);
    }

    [Fact]
    public async Task OnGetAsync_WhenPageIsValid_SearchesOnlyOnce()
    {
        // Arrange
        var expectedResult = CreateResult(
            totalCount: 12,
            page: 2);

        var adminFaqQuery = new Mock<IAdminFaqQuery>();
        var adminFaqService = new Mock<IAdminFaqService>();

        adminFaqQuery
            .Setup(x => x.SearchAsync(
                2,
                5,
                CancellationToken.None))
            .ReturnsAsync(expectedResult);

        var sut = CreateSut(
            adminFaqQuery,
            adminFaqService);

        sut.PageNumber = 2;

        // Act
        await sut.OnGetAsync(CancellationToken.None);

        // Assert
        Assert.Equal(2, sut.PageNumber);
        Assert.Same(expectedResult, sut.Result);

        adminFaqQuery.Verify(
            x => x.SearchAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task OnGetAsync_WhenPageExceedsLastPage_SearchesLastPageAgain()
    {
        // Arrange
        // 12件、1ページ5件なので最終ページは3
        var firstResult = CreateResult(
            totalCount: 12,
            page: 10);

        var correctedResult = CreateResult(
            totalCount: 12,
            page: 3);

        var adminFaqQuery = new Mock<IAdminFaqQuery>();
        var adminFaqService = new Mock<IAdminFaqService>();

        adminFaqQuery
            .Setup(x => x.SearchAsync(
                10,
                5,
                CancellationToken.None))
            .ReturnsAsync(firstResult);

        adminFaqQuery
            .Setup(x => x.SearchAsync(
                3,
                5,
                CancellationToken.None))
            .ReturnsAsync(correctedResult);

        var sut = CreateSut(
            adminFaqQuery,
            adminFaqService);

        sut.PageNumber = 10;

        // Act
        await sut.OnGetAsync(CancellationToken.None);

        // Assert
        Assert.Equal(3, sut.PageNumber);
        Assert.Same(correctedResult, sut.Result);

        adminFaqQuery.Verify(
            x => x.SearchAsync(
                10,
                5,
                CancellationToken.None),
            Times.Once);

        adminFaqQuery.Verify(
            x => x.SearchAsync(
                3,
                5,
                CancellationToken.None),
            Times.Once);
    }

    [Fact]
    public async Task OnGetAsync_ForwardsCancellationToken()
    {
        // Arrange
        using var cancellationTokenSource =
            new CancellationTokenSource();

        var cancellationToken =
            cancellationTokenSource.Token;

        var adminFaqQuery = new Mock<IAdminFaqQuery>();
        var adminFaqService = new Mock<IAdminFaqService>();

        adminFaqQuery
            .Setup(x => x.SearchAsync(
                1,
                5,
                cancellationToken))
            .ReturnsAsync(CreateResult(
                totalCount: 0,
                page: 1));

        var sut = CreateSut(
            adminFaqQuery,
            adminFaqService);

        // Act
        await sut.OnGetAsync(cancellationToken);

        // Assert
        adminFaqQuery.Verify(
            x => x.SearchAsync(
                1,
                5,
                cancellationToken),
            Times.Once);
    }

    [Fact]
    public void DisplayTotalPages_WhenResultIsEmpty_ReturnsOne()
    {
        // Arrange
        var adminFaqQuery = new Mock<IAdminFaqQuery>();
        var adminFaqService = new Mock<IAdminFaqService>();

        var sut = CreateSut(
            adminFaqQuery,
            adminFaqService);

        // Act
        var result = sut.DisplayTotalPages;

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public async Task OnPostDeleteAsync_WhenDeleteSucceeds_SetsSuccessMessage()
    {
        // Arrange
        const int faqId = 10;
        const int pageNumber = 2;

        var adminFaqQuery = new Mock<IAdminFaqQuery>();
        var adminFaqService = new Mock<IAdminFaqService>();

        adminFaqService
            .Setup(x => x.DeleteAsync(
                faqId,
                CancellationToken.None))
            .ReturnsAsync(true);

        var sut = CreateSut(
            adminFaqQuery,
            adminFaqService);

        // Act
        var result = await sut.OnPostDeleteAsync(
            faqId,
            pageNumber,
            CancellationToken.None);

        // Assert
        var redirect =
            Assert.IsType<RedirectToPageResult>(result);

        Assert.Equal(
            "/Admin/Faqs/Index",
            redirect.PageName);

        Assert.NotNull(redirect.RouteValues);

        Assert.Equal(
            pageNumber,
            redirect.RouteValues["pageNumber"]);

        Assert.Equal(
            "FAQ ID #10 を削除しました。",
            sut.TempData["SuccessMessage"]);

        Assert.False(
            sut.TempData.ContainsKey("ErrorMessage"));

        adminFaqService.Verify(
            x => x.DeleteAsync(
                faqId,
                CancellationToken.None),
            Times.Once);
    }

    [Fact]
    public async Task OnPostDeleteAsync_WhenFaqDoesNotExist_SetsErrorMessage()
    {
        // Arrange
        const int faqId = 999;
        const int pageNumber = 3;

        var adminFaqQuery = new Mock<IAdminFaqQuery>();
        var adminFaqService = new Mock<IAdminFaqService>();

        adminFaqService
            .Setup(x => x.DeleteAsync(
                faqId,
                CancellationToken.None))
            .ReturnsAsync(false);

        var sut = CreateSut(
            adminFaqQuery,
            adminFaqService);

        // Act
        var result = await sut.OnPostDeleteAsync(
            faqId,
            pageNumber,
            CancellationToken.None);

        // Assert
        var redirect =
            Assert.IsType<RedirectToPageResult>(result);

        Assert.Equal(
            "/Admin/Faqs/Index",
            redirect.PageName);

        Assert.NotNull(redirect.RouteValues);

        Assert.Equal(
            pageNumber,
            redirect.RouteValues["pageNumber"]);

        Assert.Equal(
            "削除対象のFAQが見つかりませんでした。",
            sut.TempData["ErrorMessage"]);

        Assert.False(
            sut.TempData.ContainsKey("SuccessMessage"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task OnPostDeleteAsync_WhenPageNumberIsNotPositive_RedirectsToPageOne(
        int pageNumber)
    {
        // Arrange
        const int faqId = 10;

        var adminFaqQuery = new Mock<IAdminFaqQuery>();
        var adminFaqService = new Mock<IAdminFaqService>();

        adminFaqService
            .Setup(x => x.DeleteAsync(
                faqId,
                CancellationToken.None))
            .ReturnsAsync(true);

        var sut = CreateSut(
            adminFaqQuery,
            adminFaqService);

        // Act
        var result = await sut.OnPostDeleteAsync(
            faqId,
            pageNumber,
            CancellationToken.None);

        // Assert
        var redirect =
            Assert.IsType<RedirectToPageResult>(result);

        Assert.NotNull(redirect.RouteValues);

        Assert.Equal(
            1,
            redirect.RouteValues["pageNumber"]);
    }

    private static IndexModel CreateSut(
        Mock<IAdminFaqQuery> adminFaqQuery,
        Mock<IAdminFaqService> adminFaqService)
    {
        var services = new ServiceCollection();

        services.AddSingleton<ITempDataProvider>(
            Mock.Of<ITempDataProvider>());

        services.AddSingleton<
            ITempDataDictionaryFactory,
            TempDataDictionaryFactory>();

        var httpContext = new DefaultHttpContext
        {
            RequestServices =
                services.BuildServiceProvider()
        };

        return new IndexModel(
            adminFaqQuery.Object,
            adminFaqService.Object)
        {
            PageContext = new PageContext
            {
                HttpContext = httpContext
            }
        };
    }

    private static PagedResult<AdminFaqListItem> CreateResult(
        int totalCount,
        int page)
    {
        return new PagedResult<AdminFaqListItem>(
            Items: [],
            TotalCount: totalCount,
            Page: page,
            PageSize: 5);
    }
}
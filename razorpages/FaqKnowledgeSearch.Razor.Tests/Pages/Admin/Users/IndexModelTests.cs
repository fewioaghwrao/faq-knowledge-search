using System.Security.Claims;
using FaqKnowledgeSearch.Application.Common.Models;
using FaqKnowledgeSearch.Application.Users.Admin;
using FaqKnowledgeSearch.Razor.Pages.Admin.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace FaqKnowledgeSearch.Razor.Tests.Pages.Admin.Users;

public sealed class IndexModelTests
{
    [Fact]
    public async Task OnGetAsync_SetsCurrentUserIdAndSearchesUsers()
    {
        // Arrange
        const string currentUserId = "current-user-id";

        var expectedResult = CreateResult(
            totalCount: 3,
            page: 1);

        var service = new Mock<IAdminUserService>();

        service
            .Setup(x => x.SearchAsync(
                1,
                5,
                CancellationToken.None))
            .ReturnsAsync(expectedResult);

        var sut = CreateSut(
            service,
            currentUserId);

        // Act
        await sut.OnGetAsync(
            CancellationToken.None);

        // Assert
        Assert.Equal(
            currentUserId,
            sut.CurrentUserId);

        Assert.Equal(1, sut.PageNumber);
        Assert.Same(expectedResult, sut.Result);

        service.Verify(
            x => x.SearchAsync(
                1,
                5,
                CancellationToken.None),
            Times.Once);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task OnGetAsync_WhenPageNumberIsNotPositive_NormalizesToOne(
        int pageNumber)
    {
        // Arrange
        var service = new Mock<IAdminUserService>();

        service
            .Setup(x => x.SearchAsync(
                1,
                5,
                CancellationToken.None))
            .ReturnsAsync(CreateResult(
                totalCount: 0,
                page: 1));

        var sut = CreateSut(
            service,
            "current-user-id");

        sut.PageNumber = pageNumber;

        // Act
        await sut.OnGetAsync(
            CancellationToken.None);

        // Assert
        Assert.Equal(1, sut.PageNumber);

        service.Verify(
            x => x.SearchAsync(
                1,
                5,
                CancellationToken.None),
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

        var service = new Mock<IAdminUserService>();

        service
            .Setup(x => x.SearchAsync(
                10,
                5,
                CancellationToken.None))
            .ReturnsAsync(firstResult);

        service
            .Setup(x => x.SearchAsync(
                3,
                5,
                CancellationToken.None))
            .ReturnsAsync(correctedResult);

        var sut = CreateSut(
            service,
            "current-user-id");

        sut.PageNumber = 10;

        // Act
        await sut.OnGetAsync(
            CancellationToken.None);

        // Assert
        Assert.Equal(3, sut.PageNumber);
        Assert.Same(correctedResult, sut.Result);

        service.Verify(
            x => x.SearchAsync(
                10,
                5,
                CancellationToken.None),
            Times.Once);

        service.Verify(
            x => x.SearchAsync(
                3,
                5,
                CancellationToken.None),
            Times.Once);
    }

    [Fact]
    public async Task OnGetAsync_WhenNameIdentifierDoesNotExist_SetsEmptyCurrentUserId()
    {
        // Arrange
        var service = new Mock<IAdminUserService>();

        service
            .Setup(x => x.SearchAsync(
                1,
                5,
                CancellationToken.None))
            .ReturnsAsync(CreateResult(
                totalCount: 0,
                page: 1));

        var sut = CreateSut(
            service,
            currentUserId: null);

        // Act
        await sut.OnGetAsync(
            CancellationToken.None);

        // Assert
        Assert.Equal(
            string.Empty,
            sut.CurrentUserId);
    }

    [Fact]
    public async Task OnPostToggleActiveAsync_WhenCurrentUserIdDoesNotExist_ReturnsChallenge()
    {
        // Arrange
        var service = new Mock<IAdminUserService>();

        var sut = CreateSut(
            service,
            currentUserId: null);

        // Act
        var result =
            await sut.OnPostToggleActiveAsync(
                id: "target-user-id",
                isActive: false,
                pageNumber: 1,
                CancellationToken.None);

        // Assert
        Assert.IsType<ChallengeResult>(result);

        service.Verify(
            x => x.SetActiveAsync(
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData(
        true,
        "ユーザーを有効化しました。")]
    [InlineData(
        false,
        "ユーザーを無効化しました。")]
    public async Task OnPostToggleActiveAsync_WhenSuccessful_SetsSuccessMessage(
        bool isActive,
        string expectedMessage)
    {
        // Arrange
        const string targetUserId =
            "target-user-id";

        const string currentUserId =
            "current-user-id";

        var service = new Mock<IAdminUserService>();

        service
            .Setup(x => x.SetActiveAsync(
                targetUserId,
                isActive,
                currentUserId,
                CancellationToken.None))
            .ReturnsAsync(
                AdminUserStatusChangeResult.Success);

        var sut = CreateSut(
            service,
            currentUserId);

        // Act
        var result =
            await sut.OnPostToggleActiveAsync(
                targetUserId,
                isActive,
                pageNumber: 2,
                CancellationToken.None);

        // Assert
        var redirect =
            Assert.IsType<RedirectToPageResult>(
                result);

        Assert.Equal(
            "/Admin/Users/Index",
            redirect.PageName);

        Assert.NotNull(
            redirect.RouteValues);

        Assert.Equal(
            2,
            redirect.RouteValues["pageNumber"]);

        Assert.Equal(
            expectedMessage,
            sut.TempData["SuccessMessage"]);

        Assert.False(
            sut.TempData.ContainsKey(
                "ErrorMessage"));

        service.Verify(
            x => x.SetActiveAsync(
                targetUserId,
                isActive,
                currentUserId,
                CancellationToken.None),
            Times.Once);
    }

    [Theory]
    [InlineData(
        AdminUserStatusChangeResult.NotFound,
        "対象のユーザーが見つかりませんでした。")]
    [InlineData(
        AdminUserStatusChangeResult.CannotChangeAdmin,
        "管理者ユーザーの状態は変更できません。")]
    [InlineData(
        AdminUserStatusChangeResult.CannotChangeCurrentUser,
        "ログイン中のユーザー自身は無効化できません。")]
    public async Task OnPostToggleActiveAsync_WhenChangeFails_SetsExpectedErrorMessage(
        AdminUserStatusChangeResult serviceResult,
        string expectedMessage)
    {
        // Arrange
        const string targetUserId =
            "target-user-id";

        const string currentUserId =
            "current-user-id";

        var service = new Mock<IAdminUserService>();

        service
            .Setup(x => x.SetActiveAsync(
                targetUserId,
                false,
                currentUserId,
                CancellationToken.None))
            .ReturnsAsync(serviceResult);

        var sut = CreateSut(
            service,
            currentUserId);

        // Act
        var result =
            await sut.OnPostToggleActiveAsync(
                targetUserId,
                isActive: false,
                pageNumber: 3,
                CancellationToken.None);

        // Assert
        var redirect =
            Assert.IsType<RedirectToPageResult>(
                result);

        Assert.Equal(
            "/Admin/Users/Index",
            redirect.PageName);

        Assert.Equal(
            expectedMessage,
            sut.TempData["ErrorMessage"]);

        Assert.False(
            sut.TempData.ContainsKey(
                "SuccessMessage"));
    }

    [Fact]
    public async Task OnPostToggleActiveAsync_WhenResultIsUnknown_SetsGenericErrorMessage()
    {
        // Arrange
        const string targetUserId =
            "target-user-id";

        const string currentUserId =
            "current-user-id";

        var service = new Mock<IAdminUserService>();

        service
            .Setup(x => x.SetActiveAsync(
                targetUserId,
                false,
                currentUserId,
                CancellationToken.None))
            .ReturnsAsync(
                (AdminUserStatusChangeResult)999);

        var sut = CreateSut(
            service,
            currentUserId);

        // Act
        var result =
            await sut.OnPostToggleActiveAsync(
                targetUserId,
                isActive: false,
                pageNumber: 1,
                CancellationToken.None);

        // Assert
        Assert.IsType<RedirectToPageResult>(
            result);

        Assert.Equal(
            "ユーザー状態の変更に失敗しました。",
            sut.TempData["ErrorMessage"]);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task OnPostToggleActiveAsync_WhenPageNumberIsNotPositive_RedirectsToPageOne(
        int pageNumber)
    {
        // Arrange
        const string targetUserId =
            "target-user-id";

        const string currentUserId =
            "current-user-id";

        var service = new Mock<IAdminUserService>();

        service
            .Setup(x => x.SetActiveAsync(
                targetUserId,
                true,
                currentUserId,
                CancellationToken.None))
            .ReturnsAsync(
                AdminUserStatusChangeResult.Success);

        var sut = CreateSut(
            service,
            currentUserId);

        // Act
        var result =
            await sut.OnPostToggleActiveAsync(
                targetUserId,
                isActive: true,
                pageNumber,
                CancellationToken.None);

        // Assert
        var redirect =
            Assert.IsType<RedirectToPageResult>(
                result);

        Assert.NotNull(
            redirect.RouteValues);

        Assert.Equal(
            1,
            redirect.RouteValues["pageNumber"]);
    }

    [Fact]
    public void DisplayTotalPages_WhenResultIsEmpty_ReturnsOne()
    {
        // Arrange
        var service = new Mock<IAdminUserService>();

        var sut = CreateSut(
            service,
            "current-user-id");

        // Act
        var totalPages =
            sut.DisplayTotalPages;

        // Assert
        Assert.Equal(1, totalPages);
    }

    private static IndexModel CreateSut(
        Mock<IAdminUserService> service,
        string? currentUserId)
    {
        var services =
            new ServiceCollection();

        services.AddSingleton<
            ITempDataProvider,
            TestTempDataProvider>();

        services.AddSingleton<
            ITempDataDictionaryFactory,
            TempDataDictionaryFactory>();

        var claims = new List<Claim>();

        if (!string.IsNullOrWhiteSpace(
                currentUserId))
        {
            claims.Add(
                new Claim(
                    ClaimTypes.NameIdentifier,
                    currentUserId));
        }

        var identity = new ClaimsIdentity(
            claims,
            authenticationType: "Test");

        var principal =
            new ClaimsPrincipal(identity);

        var httpContext =
            new DefaultHttpContext
            {
                RequestServices =
                    services.BuildServiceProvider(),
                User = principal
            };

        return new IndexModel(service.Object)
        {
            PageContext = new PageContext
            {
                HttpContext = httpContext
            }
        };
    }

    private static PagedResult<AdminUserListItem>
        CreateResult(
            int totalCount,
            int page)
    {
        return new PagedResult<AdminUserListItem>(
            Items: [],
            TotalCount: totalCount,
            Page: page,
            PageSize: 5);
    }

    private sealed class TestTempDataProvider
        : ITempDataProvider
    {
        public IDictionary<string, object>
            LoadTempData(
                HttpContext context)
        {
            return new Dictionary<
                string,
                object>();
        }

        public void SaveTempData(
            HttpContext context,
            IDictionary<string, object> values)
        {
        }
    }
}
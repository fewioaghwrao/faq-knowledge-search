using FaqKnowledgeSearch.Application.Faqs.Admin;
using FaqKnowledgeSearch.Razor.Pages.Admin.Faqs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace FaqKnowledgeSearch.Razor.Tests.Pages.Admin.Faqs;

public sealed class CreateModelTests
{
    [Fact]
    public async Task OnGetAsync_LoadsCategoryAndTagOptions()
    {
        // Arrange
        using var cancellationTokenSource =
            new CancellationTokenSource();

        var cancellationToken =
            cancellationTokenSource.Token;

        IReadOnlyList<AdminFaqOption> categories =
        [
            new(1, "アカウント"),
        new(2, "システム")
        ];

        IReadOnlyList<AdminFaqOption> tags =
        [
            new(1, "ログイン"),
        new(2, "障害対応")
        ];

        var formOptions =
            new AdminFaqFormOptions(
                Categories: categories,
                Tags: tags);

        var service =
            new Mock<IAdminFaqService>();

        service
            .Setup(x => x.GetFormOptionsAsync(
                cancellationToken))
            .ReturnsAsync(formOptions);

        var sut = CreateSut(service);

        // Act
        await sut.OnGetAsync(cancellationToken);

        // Assert
        Assert.Same(
            categories,
            sut.CategoryOptions);

        Assert.Same(
            tags,
            sut.TagOptions);

        service.Verify(
            x => x.GetFormOptionsAsync(
                cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task OnPostAsync_WhenTagIdsAreInvalid_AddsErrorAndReturnsPage()
    {
        // Arrange
        var service = new Mock<IAdminFaqService>();

        SetupFormOptions(service);

        var sut = CreateSut(service);

        sut.Input = CreateValidInput();
        sut.Input.TagIds = "1,abc,3";

        // Act
        var result = await sut.OnPostAsync(
            CancellationToken.None);

        // Assert
        Assert.IsType<PageResult>(result);

        var errors =
            sut.ModelState["Input.TagIds"]!.Errors;

        Assert.Contains(
            errors,
            error =>
                error.ErrorMessage ==
                "タグIDは「1,2」のように数値をカンマ区切りで入力してください。");

        Assert.NotEmpty(sut.CategoryOptions);
        Assert.NotEmpty(sut.TagOptions);

        service.Verify(
            x => x.CreateAsync(
                It.IsAny<AdminFaqCommand>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        service.Verify(
            x => x.GetFormOptionsAsync(
                CancellationToken.None),
            Times.Once);
    }

    [Fact]
    public async Task OnPostAsync_WhenModelStateIsInvalid_DoesNotCreateFaq()
    {
        // Arrange
        var service = new Mock<IAdminFaqService>();

        SetupFormOptions(service);

        var sut = CreateSut(service);

        sut.Input = CreateValidInput();

        sut.ModelState.AddModelError(
            "Input.Title",
            "タイトルを入力してください。");

        // Act
        var result = await sut.OnPostAsync(
            CancellationToken.None);

        // Assert
        Assert.IsType<PageResult>(result);

        service.Verify(
            x => x.CreateAsync(
                It.IsAny<AdminFaqCommand>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        service.Verify(
            x => x.GetFormOptionsAsync(
                CancellationToken.None),
            Times.Once);
    }

    [Fact]
    public async Task OnPostAsync_WhenInputIsValid_CreatesFaqAndRedirects()
    {
        // Arrange
        using var cancellationTokenSource =
            new CancellationTokenSource();

        var cancellationToken =
            cancellationTokenSource.Token;

        AdminFaqCommand? capturedCommand = null;

        var service = new Mock<IAdminFaqService>();

        service
            .Setup(x => x.CreateAsync(
                It.IsAny<AdminFaqCommand>(),
                cancellationToken))
            .Callback<AdminFaqCommand, CancellationToken>(
                (command, _) =>
                    capturedCommand = command)
            .ReturnsAsync(10);

        var sut = CreateSut(service);

        sut.Input = new FaqFormInput
        {
            Title = "ログインできない場合",
            Body = "パスワードとアカウント状態を確認します。",
            CategoryId = 2,
            TagIds = "1,2,1",
            IsPublished = true
        };

        // Act
        var result = await sut.OnPostAsync(
            cancellationToken);

        // Assert
        var redirect =
            Assert.IsType<RedirectToPageResult>(result);

        Assert.Equal(
            "/Admin/Faqs/Index",
            redirect.PageName);

        Assert.NotNull(capturedCommand);

        Assert.Equal(
            "ログインできない場合",
            capturedCommand.Title);

        Assert.Equal(
            "パスワードとアカウント状態を確認します。",
            capturedCommand.Body);

        Assert.Equal(
            2,
            capturedCommand.CategoryId);

        Assert.Equal(
            [1, 2],
            capturedCommand.TagIds);

        Assert.True(
            capturedCommand.IsPublished);

        Assert.Equal(
            "FAQを登録しました。",
            sut.TempData["SuccessMessage"]);

        service.Verify(
            x => x.CreateAsync(
                It.IsAny<AdminFaqCommand>(),
                cancellationToken),
            Times.Once);

        service.Verify(
            x => x.GetFormOptionsAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task OnPostAsync_WhenServiceThrowsArgumentException_AddsErrorAndReturnsPage()
    {
        // Arrange
        const string errorMessage =
            "指定されたカテゴリが存在しません。";

        var service = new Mock<IAdminFaqService>();

        service
            .Setup(x => x.CreateAsync(
                It.IsAny<AdminFaqCommand>(),
                CancellationToken.None))
            .ThrowsAsync(
                new ArgumentException(errorMessage));

        SetupFormOptions(service);

        var sut = CreateSut(service);

        sut.Input = CreateValidInput();

        // Act
        var result = await sut.OnPostAsync(
            CancellationToken.None);

        // Assert
        Assert.IsType<PageResult>(result);

        var errors =
            sut.ModelState[string.Empty]!.Errors;

        Assert.Contains(
            errors,
            error =>
                error.ErrorMessage == errorMessage);

        Assert.NotEmpty(sut.CategoryOptions);
        Assert.NotEmpty(sut.TagOptions);

        Assert.False(
            sut.TempData.ContainsKey(
                "SuccessMessage"));

        service.Verify(
            x => x.CreateAsync(
                It.IsAny<AdminFaqCommand>(),
                CancellationToken.None),
            Times.Once);

        service.Verify(
            x => x.GetFormOptionsAsync(
                CancellationToken.None),
            Times.Once);
    }

    private static void SetupFormOptions(
        Mock<IAdminFaqService> service)
    {
        IReadOnlyList<AdminFaqOption> categories =
        [
            new(1, "アカウント")
        ];

        IReadOnlyList<AdminFaqOption> tags =
        [
            new(1, "ログイン")
        ];

        var formOptions =
            new AdminFaqFormOptions(
                Categories: categories,
                Tags: tags);

        service
            .Setup(x => x.GetFormOptionsAsync(
                CancellationToken.None))
            .ReturnsAsync(formOptions);
    }

    private static FaqFormInput CreateValidInput()
    {
        return new FaqFormInput
        {
            Title = "ログイン方法",
            Body = "ログイン画面から認証してください。",
            CategoryId = 1,
            TagIds = "1,2",
            IsPublished = true
        };
    }

    private static CreateModel CreateSut(
        Mock<IAdminFaqService> service)
    {
        var services =
            new ServiceCollection();

        services.AddSingleton<
            ITempDataProvider,
            TestTempDataProvider>();

        services.AddSingleton<
            IStartupFilter>(_ =>
                Mock.Of<IStartupFilter>());

        services.AddSingleton<
            ITempDataDictionaryFactory,
            TempDataDictionaryFactory>();

        var httpContext =
            new DefaultHttpContext
            {
                RequestServices =
                    services.BuildServiceProvider()
            };

        return new CreateModel(service.Object)
        {
            PageContext = new PageContext
            {
                HttpContext = httpContext
            }
        };
    }

    private sealed class TestTempDataProvider
        : ITempDataProvider
    {
        public IDictionary<string, object>
            LoadTempData(HttpContext context)
        {
            return new Dictionary<string, object>();
        }

        public void SaveTempData(
            HttpContext context,
            IDictionary<string, object> values)
        {
        }
    }
}

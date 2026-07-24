using FaqKnowledgeSearch.Application.Faqs.Admin;
using FaqKnowledgeSearch.Razor.Pages.Admin.Faqs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace FaqKnowledgeSearch.Razor.Tests.Pages.Admin.Faqs;

public sealed class EditModelTests
{
    [Fact]
    public async Task OnGetAsync_WhenFaqExists_SetsInputAndLoadsOptions()
    {
        // Arrange
        const int faqId = 10;

        var editData = new AdminFaqEditData(
            Id: faqId,
            Title: "ログインできない場合",
            Body: "パスワードを確認してください。",
            CategoryId: 2,
            TagIds: [1, 3],
            IsPublished: true);

        var formOptions = CreateFormOptions();

        var service = new Mock<IAdminFaqService>();

        service
            .Setup(x => x.GetEditDataAsync(
                faqId,
                CancellationToken.None))
            .ReturnsAsync(editData);

        service
            .Setup(x => x.GetFormOptionsAsync(
                CancellationToken.None))
            .ReturnsAsync(formOptions);

        var sut = CreateSut(service);
        sut.Id = faqId;

        // Act
        var result = await sut.OnGetAsync(
            CancellationToken.None);

        // Assert
        Assert.IsType<PageResult>(result);

        Assert.Equal(
            "ログインできない場合",
            sut.Input.Title);

        Assert.Equal(
            "パスワードを確認してください。",
            sut.Input.Body);

        Assert.Equal(2, sut.Input.CategoryId);
        Assert.Equal("1,3", sut.Input.TagIds);
        Assert.True(sut.Input.IsPublished);

        Assert.Same(
            formOptions.Categories,
            sut.CategoryOptions);

        Assert.Same(
            formOptions.Tags,
            sut.TagOptions);

        service.Verify(
            x => x.GetEditDataAsync(
                faqId,
                CancellationToken.None),
            Times.Once);

        service.Verify(
            x => x.GetFormOptionsAsync(
                CancellationToken.None),
            Times.Once);
    }

    [Fact]
    public async Task OnGetAsync_WhenFaqDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        const int faqId = 999;

        var service = new Mock<IAdminFaqService>();

        service
            .Setup(x => x.GetEditDataAsync(
                faqId,
                CancellationToken.None))
            .ReturnsAsync(
                (AdminFaqEditData?)null);

        var sut = CreateSut(service);
        sut.Id = faqId;

        // Act
        var result = await sut.OnGetAsync(
            CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundResult>(result);

        service.Verify(
            x => x.GetFormOptionsAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task OnPostAsync_WhenTagIdsAreInvalid_ReturnsPageWithoutUpdating()
    {
        // Arrange
        var service = new Mock<IAdminFaqService>();

        service
            .Setup(x => x.GetFormOptionsAsync(
                CancellationToken.None))
            .ReturnsAsync(CreateFormOptions());

        var sut = CreateSut(service);

        sut.Id = 10;
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

        service.Verify(
            x => x.UpdateAsync(
                It.IsAny<int>(),
                It.IsAny<AdminFaqCommand>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        service.Verify(
            x => x.GetFormOptionsAsync(
                CancellationToken.None),
            Times.Once);
    }

    [Fact]
    public async Task OnPostAsync_WhenUpdateSucceeds_UpdatesAndRedirects()
    {
        // Arrange
        const int faqId = 10;

        AdminFaqCommand? capturedCommand = null;

        var service = new Mock<IAdminFaqService>();

        service
            .Setup(x => x.UpdateAsync(
                faqId,
                It.IsAny<AdminFaqCommand>(),
                CancellationToken.None))
            .Callback<int, AdminFaqCommand, CancellationToken>(
                (_, command, _) =>
                    capturedCommand = command)
            .ReturnsAsync(true);

        var sut = CreateSut(service);

        sut.Id = faqId;
        sut.Input = new FaqFormInput
        {
            Title = "更新後のタイトル",
            Body = "更新後の本文です。",
            CategoryId = 3,
            TagIds = "1,2,1",
            IsPublished = false
        };

        // Act
        var result = await sut.OnPostAsync(
            CancellationToken.None);

        // Assert
        var redirect =
            Assert.IsType<RedirectToPageResult>(result);

        Assert.Equal(
            "/Admin/Faqs/Index",
            redirect.PageName);

        Assert.NotNull(capturedCommand);

        Assert.Equal(
            "更新後のタイトル",
            capturedCommand.Title);

        Assert.Equal(
            "更新後の本文です。",
            capturedCommand.Body);

        Assert.Equal(
            3,
            capturedCommand.CategoryId);

        Assert.Equal(
            [1, 2],
            capturedCommand.TagIds);

        Assert.False(
            capturedCommand.IsPublished);

        Assert.Equal(
            "FAQを更新しました。",
            sut.TempData["SuccessMessage"]);

        service.Verify(
            x => x.GetFormOptionsAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task OnPostAsync_WhenUpdateTargetDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        const int faqId = 999;

        var service = new Mock<IAdminFaqService>();

        service
            .Setup(x => x.UpdateAsync(
                faqId,
                It.IsAny<AdminFaqCommand>(),
                CancellationToken.None))
            .ReturnsAsync(false);

        var sut = CreateSut(service);

        sut.Id = faqId;
        sut.Input = CreateValidInput();

        // Act
        var result = await sut.OnPostAsync(
            CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundResult>(result);

        Assert.False(
            sut.TempData.ContainsKey(
                "SuccessMessage"));

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
            .Setup(x => x.UpdateAsync(
                It.IsAny<int>(),
                It.IsAny<AdminFaqCommand>(),
                CancellationToken.None))
            .ThrowsAsync(
                new ArgumentException(errorMessage));

        service
            .Setup(x => x.GetFormOptionsAsync(
                CancellationToken.None))
            .ReturnsAsync(CreateFormOptions());

        var sut = CreateSut(service);

        sut.Id = 10;
        sut.Input = CreateValidInput();

        // Act
        var result = await sut.OnPostAsync(
            CancellationToken.None);

        // Assert
        Assert.IsType<PageResult>(result);

        Assert.Contains(
            sut.ModelState[string.Empty]!.Errors,
            error =>
                error.ErrorMessage == errorMessage);

        Assert.NotEmpty(sut.CategoryOptions);
        Assert.NotEmpty(sut.TagOptions);

        Assert.False(
            sut.TempData.ContainsKey(
                "SuccessMessage"));
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

    private static AdminFaqFormOptions CreateFormOptions()
    {
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

        return new AdminFaqFormOptions(
            Categories: categories,
            Tags: tags);
    }

    private static EditModel CreateSut(
        Mock<IAdminFaqService> service)
    {
        var services =
            new ServiceCollection();

        services.AddSingleton<
            ITempDataProvider,
            TestTempDataProvider>();

        services.AddSingleton<
            ITempDataDictionaryFactory,
            TempDataDictionaryFactory>();

        var httpContext =
            new DefaultHttpContext
            {
                RequestServices =
                    services.BuildServiceProvider()
            };

        return new EditModel(service.Object)
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
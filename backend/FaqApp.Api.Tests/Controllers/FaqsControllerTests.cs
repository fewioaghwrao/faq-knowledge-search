using FaqApp.Api.Controller;
using FaqApp.Api.Dtos.Faqs;
using FaqApp.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace FaqApp.Api.Tests.Controllers;

public class FaqsControllerTests
{
    private readonly Mock<IFaqService> _faqServiceMock = new();

    private FaqsController CreateController()
    {
        return new FaqsController(_faqServiceMock.Object);
    }

    [Fact]
    public async Task Search_ReturnsOk()
    {
        // Arrange
        var query = new FaqSearchQuery
        {
            Keyword = "ログイン"
        };

        var faqs = new List<FaqListItemDto>
        {
            new FaqListItemDto
            {
                Id = 1,
                Title = "ログインできない場合",
                Body = "パスワードを確認してください。",
                CategoryName = "ログイン障害",
                IsPublished = true,
                ViewCount = 0,
                UpdatedAt = DateTime.UtcNow
            }
        };

        _faqServiceMock
            .Setup(x => x.SearchAsync(query))
            .ReturnsAsync(faqs);

        var controller = CreateController();

        // Act
        var result = await controller.Search(query);

        // Assert
        var actionResult = Assert.IsType<ActionResult<List<FaqListItemDto>>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);

        Assert.Same(faqs, okResult.Value);

        _faqServiceMock.Verify(x => x.SearchAsync(query), Times.Once);
    }

    [Fact]
    public async Task GetById_ExistingFaq_ReturnsOk()
    {
        // Arrange
        var id = 1;

        var faq = new FaqListItemDto
        {
            Id = id,
            Title = "CSV取込エラー",
            Body = "文字コードを確認してください。",
            CategoryName = "CSV取込",
            IsPublished = true,
            ViewCount = 5,
            UpdatedAt = DateTime.UtcNow
        };

        _faqServiceMock
            .Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync(faq);

        var controller = CreateController();

        // Act
        var result = await controller.GetById(id);

        // Assert
        var actionResult = Assert.IsType<ActionResult<FaqListItemDto>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);

        Assert.Same(faq, okResult.Value);

        _faqServiceMock.Verify(x => x.GetByIdAsync(id), Times.Once);
    }

    [Fact]
    public async Task GetById_NotFound_ReturnsNotFound()
    {
        // Arrange
        var id = 999;

        _faqServiceMock
            .Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync((FaqListItemDto?)null);

        var controller = CreateController();

        // Act
        var result = await controller.GetById(id);

        // Assert
        var actionResult = Assert.IsType<ActionResult<FaqListItemDto>>(result);
        Assert.IsType<NotFoundObjectResult>(actionResult.Result);

        _faqServiceMock.Verify(x => x.GetByIdAsync(id), Times.Once);
    }

    [Fact]
    public async Task Create_ValidRequest_ReturnsCreatedAtAction()
    {
        // Arrange
        var request = new FaqCreateRequest
        {
            Title = "PDF出力できない場合",
            Body = "対象データと権限を確認してください。",
            CategoryId = 1,
            IsPublished = true
        };

        var createdFaq = new FaqListItemDto
        {
            Id = 10,
            Title = request.Title,
            Body = request.Body,
            CategoryName = "PDF出力",
            IsPublished = true,
            ViewCount = 0,
            UpdatedAt = DateTime.UtcNow
        };

        _faqServiceMock
            .Setup(x => x.CreateAsync(request))
            .ReturnsAsync(createdFaq);

        var controller = CreateController();

        // Act
        var result = await controller.Create(request);

        // Assert
        var actionResult = Assert.IsType<ActionResult<FaqListItemDto>>(result);
        var createdResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);

        Assert.Equal(nameof(FaqsController.GetById), createdResult.ActionName);
        Assert.Equal(createdFaq.Id, createdResult.RouteValues?["id"]);
        Assert.Same(createdFaq, createdResult.Value);

        _faqServiceMock.Verify(x => x.CreateAsync(request), Times.Once);
    }

    [Fact]
    public async Task Create_InvalidOperation_ReturnsBadRequest()
    {
        // Arrange
        var request = new FaqCreateRequest
        {
            Title = "カテゴリ不正",
            Body = "本文",
            CategoryId = 999,
            IsPublished = true
        };

        _faqServiceMock
            .Setup(x => x.CreateAsync(request))
            .ThrowsAsync(new InvalidOperationException("指定されたカテゴリが存在しません。"));

        var controller = CreateController();

        // Act
        var result = await controller.Create(request);

        // Assert
        var actionResult = Assert.IsType<ActionResult<FaqListItemDto>>(result);
        Assert.IsType<BadRequestObjectResult>(actionResult.Result);

        _faqServiceMock.Verify(x => x.CreateAsync(request), Times.Once);
    }

    [Fact]
    public async Task Update_ExistingFaq_ReturnsOk()
    {
        // Arrange
        var id = 1;

        var request = new FaqUpdateRequest
        {
            Title = "CSV取込エラー 修正版",
            Body = "UTF-8で保存してください。",
            CategoryId = 2,
            IsPublished = true
        };

        var updatedFaq = new FaqListItemDto
        {
            Id = id,
            Title = request.Title,
            Body = request.Body,
            CategoryName = "CSV取込",
            IsPublished = true,
            ViewCount = 5,
            UpdatedAt = DateTime.UtcNow
        };

        _faqServiceMock
            .Setup(x => x.UpdateAsync(id, request))
            .ReturnsAsync(updatedFaq);

        var controller = CreateController();

        // Act
        var result = await controller.Update(id, request);

        // Assert
        var actionResult = Assert.IsType<ActionResult<FaqListItemDto>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);

        Assert.Same(updatedFaq, okResult.Value);

        _faqServiceMock.Verify(x => x.UpdateAsync(id, request), Times.Once);
    }

    [Fact]
    public async Task Update_NotFound_ReturnsNotFound()
    {
        // Arrange
        var id = 999;

        var request = new FaqUpdateRequest
        {
            Title = "存在しないFAQ",
            Body = "本文",
            CategoryId = 1,
            IsPublished = true
        };

        _faqServiceMock
            .Setup(x => x.UpdateAsync(id, request))
            .ReturnsAsync((FaqListItemDto?)null);

        var controller = CreateController();

        // Act
        var result = await controller.Update(id, request);

        // Assert
        var actionResult = Assert.IsType<ActionResult<FaqListItemDto>>(result);
        Assert.IsType<NotFoundObjectResult>(actionResult.Result);

        _faqServiceMock.Verify(x => x.UpdateAsync(id, request), Times.Once);
    }

    [Fact]
    public async Task Update_InvalidOperation_ReturnsBadRequest()
    {
        // Arrange
        var id = 1;

        var request = new FaqUpdateRequest
        {
            Title = "カテゴリ不正",
            Body = "本文",
            CategoryId = 999,
            IsPublished = true
        };

        _faqServiceMock
            .Setup(x => x.UpdateAsync(id, request))
            .ThrowsAsync(new InvalidOperationException("指定されたカテゴリが存在しません。"));

        var controller = CreateController();

        // Act
        var result = await controller.Update(id, request);

        // Assert
        var actionResult = Assert.IsType<ActionResult<FaqListItemDto>>(result);
        Assert.IsType<BadRequestObjectResult>(actionResult.Result);

        _faqServiceMock.Verify(x => x.UpdateAsync(id, request), Times.Once);
    }

    [Fact]
    public async Task Delete_ExistingFaq_ReturnsNoContent()
    {
        // Arrange
        var id = 1;

        _faqServiceMock
            .Setup(x => x.DeleteAsync(id))
            .ReturnsAsync(true);

        var controller = CreateController();

        // Act
        var result = await controller.Delete(id);

        // Assert
        Assert.IsType<NoContentResult>(result);

        _faqServiceMock.Verify(x => x.DeleteAsync(id), Times.Once);
    }

    [Fact]
    public async Task Delete_NotFound_ReturnsNotFound()
    {
        // Arrange
        var id = 999;

        _faqServiceMock
            .Setup(x => x.DeleteAsync(id))
            .ReturnsAsync(false);

        var controller = CreateController();

        // Act
        var result = await controller.Delete(id);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);

        _faqServiceMock.Verify(x => x.DeleteAsync(id), Times.Once);
    }

    [Theory]
    [InlineData(nameof(FaqsController.Create))]
    [InlineData(nameof(FaqsController.Update))]
    [InlineData(nameof(FaqsController.Delete))]
    public void AdminOnlyActions_HaveAuthorizeAdminAttribute(string methodName)
    {
        // Arrange
        var method = typeof(FaqsController)
            .GetMethods()
            .First(x => x.Name == methodName);

        // Act
        var attribute = method
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: false)
            .OfType<AuthorizeAttribute>()
            .FirstOrDefault();

        // Assert
        Assert.NotNull(attribute);
        Assert.Equal("Admin", attribute!.Roles);
    }
}
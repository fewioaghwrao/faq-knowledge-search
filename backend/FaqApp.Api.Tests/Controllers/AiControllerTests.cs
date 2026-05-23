using FaqApp.Api.Controller;
using FaqApp.Api.Dtos.Ai;
using FaqApp.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Moq;
using Xunit;

namespace FaqApp.Api.Tests.Controllers;

public class AiControllerTests
{
    private readonly Mock<IAiService> _aiServiceMock = new();
    private readonly Mock<IAiSearchHistoryService> _historyServiceMock = new();

    private AiController CreateController()
    {
        return new AiController(
            _aiServiceMock.Object,
            _historyServiceMock.Object);
    }

    [Fact]
    public async Task Search_ValidRequest_ReturnsOk()
    {
        // Arrange
        var request = new AiSearchRequest
        {
            Question = "ログインできない場合は？"
        };

        var response = new AiSearchResponse
        {
            Answer = "パスワード再設定を確認してください。",
            Disclaimer = "詳細は参照元FAQをご確認ください。",
            Sources = new(),
            Message = null,
            AiHistoryId = 1
        };

        _aiServiceMock
            .Setup(x => x.SearchAsync(request))
            .ReturnsAsync(response);

        var controller = CreateController();

        // Act
        var result = await controller.Search(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Same(response, okResult.Value);

        _aiServiceMock.Verify(x => x.SearchAsync(request), Times.Once);
    }

    [Fact]
    public async Task Search_InvalidModelState_ReturnsBadRequest()
    {
        // Arrange
        var request = new AiSearchRequest
        {
            Question = ""
        };

        var controller = CreateController();
        controller.ModelState.AddModelError("Question", "質問は必須です。");

        // Act
        var result = await controller.Search(request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);

        _aiServiceMock.Verify(
            x => x.SearchAsync(It.IsAny<AiSearchRequest>()),
            Times.Never);
    }

    [Fact]
    public async Task GetHistories_ReturnsOk()
    {
        // Arrange
        var query = new AiSearchHistoryQuery();

        var histories = new List<AiSearchHistoryListItemDto>
        {
            new AiSearchHistoryListItemDto
            {
                Id = 1,
                Question = "ログインできない",
                AnswerPreview = "ログイン情報を確認してください。",
                IsSuccess = true,
                ErrorMessage = null,
                IsHelpful = null,
                SourceCount = 2,
                ExecutedAt = DateTime.UtcNow
            }
        };

        _historyServiceMock
            .Setup(x => x.GetListAsync(query))
            .ReturnsAsync(histories);

        var controller = CreateController();

        // Act
        var result = await controller.GetHistories(query);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Same(histories, okResult.Value);

        _historyServiceMock.Verify(x => x.GetListAsync(query), Times.Once);
    }

    [Fact]
    public async Task GetHistoryDetail_ExistingId_ReturnsOk()
    {
        // Arrange
        var id = 1;

        var detail = new AiSearchHistoryDetailDto
        {
            Id = id,
            Question = "ログインできない",
            SearchKeywords = "ログイン パスワード",
            AiAnswer = "パスワード再設定を確認してください。",
            IsSuccess = true,
            ErrorMessage = null,
            IsHelpful = null,
            ExecutedAt = DateTime.UtcNow,
            Sources = new List<AiSearchHistorySourceDto>()
        };

        _historyServiceMock
            .Setup(x => x.GetDetailAsync(id))
            .ReturnsAsync(detail);

        var controller = CreateController();

        // Act
        var result = await controller.GetHistoryDetail(id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Same(detail, okResult.Value);

        _historyServiceMock.Verify(x => x.GetDetailAsync(id), Times.Once);
    }

    [Fact]
    public async Task GetHistoryDetail_NotFound_ReturnsNotFound()
    {
        // Arrange
        var id = 999;

        _historyServiceMock
            .Setup(x => x.GetDetailAsync(id))
            .ReturnsAsync((AiSearchHistoryDetailDto?)null);

        var controller = CreateController();

        // Act
        var result = await controller.GetHistoryDetail(id);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);

        _historyServiceMock.Verify(x => x.GetDetailAsync(id), Times.Once);
    }

    [Fact]
    public async Task UpdateFeedback_ExistingHistory_ReturnsNoContent()
    {
        // Arrange
        var id = 1;
        var request = new AiSearchFeedbackRequestDto
        {
            IsHelpful = true
        };

        _historyServiceMock
            .Setup(x => x.UpdateFeedbackAsync(id, request.IsHelpful))
            .Returns(Task.CompletedTask);

        var controller = CreateController();

        // Act
        var result = await controller.UpdateFeedback(id, request);

        // Assert
        Assert.IsType<NoContentResult>(result);

        _historyServiceMock.Verify(
            x => x.UpdateFeedbackAsync(id, request.IsHelpful),
            Times.Once);
    }

    [Fact]
    public async Task UpdateFeedback_NotFound_ReturnsNotFound()
    {
        // Arrange
        var id = 999;
        var request = new AiSearchFeedbackRequestDto
        {
            IsHelpful = false
        };

        _historyServiceMock
            .Setup(x => x.UpdateFeedbackAsync(id, request.IsHelpful))
            .ThrowsAsync(new KeyNotFoundException("指定されたAI検索履歴は存在しません。"));

        var controller = CreateController();

        // Act
        var result = await controller.UpdateFeedback(id, request);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);

        _historyServiceMock.Verify(
            x => x.UpdateFeedbackAsync(id, request.IsHelpful),
            Times.Once);
    }

    [Fact]
    public void Search_HasEnableRateLimitingAttribute()
    {
        // Arrange
        var method = typeof(AiController).GetMethod(nameof(AiController.Search));

        // Act
        var attribute = method?
            .GetCustomAttributes(typeof(EnableRateLimitingAttribute), inherit: false)
            .OfType<EnableRateLimitingAttribute>()
            .FirstOrDefault();

        // Assert
        Assert.NotNull(attribute);
        Assert.Equal("AiSearchPolicy", attribute!.PolicyName);
    }
}
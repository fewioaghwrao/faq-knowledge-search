using FaqApp.Api.Data;
using FaqApp.Api.Dtos.Ai;
using FaqApp.Api.Dtos.Faqs;
using FaqApp.Api.Entities;
using FaqApp.Api.Services;
using FaqApp.Api.Services.Interfaces;
using FaqApp.Api.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace FaqApp.Api.Tests.Services;

public class AiServiceTests
{
    private readonly Mock<IFaqService> _faqServiceMock = new();
    private readonly Mock<IAiApiClient> _aiApiClientMock = new();

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private AiService CreateService(AppDbContext dbContext, int maxContextFaqCount = 5)
    {
        var settings = Options.Create(new AiSettings
        {
            MaxContextFaqCount = maxContextFaqCount
        });

        var logger = Mock.Of<ILogger<AiService>>();

        return new AiService(
            _faqServiceMock.Object,
            _aiApiClientMock.Object,
            settings,
            logger,
            dbContext);
    }

    [Fact]
    public async Task SearchAsync_QuestionIsEmpty_ReturnsMessage()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var service = CreateService(dbContext);

        var request = new AiSearchRequest
        {
            Question = "   "
        };

        // Act
        var result = await service.SearchAsync(request);

        // Assert
        Assert.Null(result.Answer);
        Assert.Null(result.Disclaimer);
        Assert.Empty(result.Sources);
        Assert.Equal("質問文を入力してください。", result.Message);
        Assert.Equal(0, result.AiHistoryId);

        _faqServiceMock.Verify(
            x => x.SearchAsync(It.IsAny<FaqSearchQuery>()),
            Times.Never);

        _aiApiClientMock.Verify(
            x => x.GenerateAnswerAsync(
                It.IsAny<string>(),
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task SearchAsync_NoFaqs_ReturnsNoFaqMessageAndSavesHistory()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var service = CreateService(dbContext);

        var request = new AiSearchRequest
        {
            Question = "存在しない質問"
        };

        _faqServiceMock
            .Setup(x => x.SearchAsync(It.IsAny<FaqSearchQuery>()))
            .ReturnsAsync(new List<FaqListItemDto>());

        // Act
        var result = await service.SearchAsync(request);

        // Assert
        Assert.Null(result.Answer);
        Assert.Null(result.Disclaimer);
        Assert.Empty(result.Sources);
        Assert.Equal("該当するFAQが見つかりませんでした。管理者にご相談ください。", result.Message);
        Assert.True(result.AiHistoryId > 0);

        var history = await dbContext.AiSearchHistories.FirstAsync();

        Assert.Equal("存在しない質問", history.Question);
        Assert.False(history.IsSuccess);
        Assert.Equal("該当するFAQが見つかりませんでした。", history.ErrorMessage);

        _aiApiClientMock.Verify(
            x => x.GenerateAnswerAsync(
                It.IsAny<string>(),
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task SearchAsync_AiSuccess_ReturnsAnswerAndSavesSuccessHistory()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var service = CreateService(dbContext);

        var request = new AiSearchRequest
        {
            Question = "ログインできない場合は？"
        };

        var faqs = new List<FaqListItemDto>
        {
            new FaqListItemDto
            {
                Id = 1,
                Title = "ログインできない場合",
                Body = "パスワード再設定を確認してください。",
                CategoryName = "ログイン障害",
                Score = 10
            }
        };

        _faqServiceMock
            .Setup(x => x.SearchAsync(It.IsAny<FaqSearchQuery>()))
            .ReturnsAsync(faqs);

        _aiApiClientMock
            .Setup(x => x.GenerateAnswerAsync(
                "ログインできない場合は？",
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("パスワード再設定を確認してください。詳細は参照元FAQをご確認ください。");

        // Act
        var result = await service.SearchAsync(request);

        // Assert
        Assert.Equal("パスワード再設定を確認してください。詳細は参照元FAQをご確認ください。", result.Answer);
        Assert.Equal("この回答はFAQをもとに生成されています。必ず参照元を確認してください。", result.Disclaimer);
        Assert.Null(result.Message);
        Assert.Single(result.Sources);
        Assert.Equal(1, result.Sources[0].Id);
        Assert.Equal("/faqs/1", result.Sources[0].Url);
        Assert.True(result.AiHistoryId > 0);

        var history = await dbContext.AiSearchHistories
            .Include(x => x.Sources)
            .FirstAsync();

        Assert.True(history.IsSuccess);
        Assert.Equal("ログインできない場合は？", history.Question);
        Assert.Equal(result.Answer, history.AiAnswer);
        Assert.Null(history.ErrorMessage);
        Assert.Single(history.Sources);

        var source = history.Sources.First();
        Assert.Equal(1, source.FaqId);
    }

    [Fact]
    public async Task SearchAsync_AiFailure_ReturnsErrorMessageAndSavesFailureHistory()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var service = CreateService(dbContext);

        var request = new AiSearchRequest
        {
            Question = "ログインできない場合は？"
        };

        var faqs = new List<FaqListItemDto>
        {
            new FaqListItemDto
            {
                Id = 1,
                Title = "ログインできない場合",
                Body = "パスワード再設定を確認してください。",
                CategoryName = "ログイン障害",
                Score = 10
            }
        };

        _faqServiceMock
            .Setup(x => x.SearchAsync(It.IsAny<FaqSearchQuery>()))
            .ReturnsAsync(faqs);

        _aiApiClientMock
            .Setup(x => x.GenerateAnswerAsync(
                It.IsAny<string>(),
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("AI回答が空でした。"));

        // Act
        var result = await service.SearchAsync(request);

        // Assert
        Assert.Null(result.Answer);
        Assert.Null(result.Disclaimer);
        Assert.Equal("AI回答の生成に失敗しました。参照元FAQをご確認ください。", result.Message);
        Assert.Single(result.Sources);
        Assert.True(result.AiHistoryId > 0);

        var history = await dbContext.AiSearchHistories
            .Include(x => x.Sources)
            .FirstAsync();

        Assert.False(history.IsSuccess);
        Assert.Null(history.AiAnswer);
        Assert.Equal("AI回答が空でした。", history.ErrorMessage);
        Assert.Single(history.Sources);

        var source = history.Sources.First();
        Assert.Equal(1, source.FaqId);
    }
}
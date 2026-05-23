using FaqApp.Api.Data;
using FaqApp.Api.Dtos.Ai;
using FaqApp.Api.Entities;
using FaqApp.Api.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FaqApp.Api.Tests.Services;

public class AiSearchHistoryServiceTests
{
    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task GetListAsync_ReturnsHistories()
    {
        // Arrange
        await using var dbContext = CreateDbContext();

        dbContext.AiSearchHistories.Add(new AiSearchHistory
        {
            Id = 1,
            Question = "ログインできない",
            SearchKeywords = "ログイン パスワード",
            AiAnswer = "パスワード再設定を確認してください。",
            IsSuccess = true,
            ErrorMessage = null,
            IsHelpful = null,
            ExecutedAt = DateTime.UtcNow,
            Sources = new List<AiSearchHistorySource>
            {
                new AiSearchHistorySource
                {
                    Id = 1,
                    FaqId = 10,
                    FaqTitle = "ログインできない場合",
                    DisplayOrder = 1,
                    Score = 10
                }
            }
        });

        await dbContext.SaveChangesAsync();

        var service = new AiSearchHistoryService(dbContext);

        var query = new AiSearchHistoryQuery
        {
            Page = 1,
            PageSize = 20
        };

        // Act
        var result = await service.GetListAsync(query);

        // Assert
        Assert.Single(result);

        var item = result[0];
        Assert.Equal(1, item.Id);
        Assert.Equal("ログインできない", item.Question);
        Assert.Equal("パスワード再設定を確認してください。", item.AnswerPreview);
        Assert.True(item.IsSuccess);
        Assert.Equal(1, item.SourceCount);
    }

    [Fact]
    public async Task GetListAsync_WithKeyword_ReturnsMatchedHistories()
    {
        // Arrange
        await using var dbContext = CreateDbContext();

        dbContext.AiSearchHistories.AddRange(
            new AiSearchHistory
            {
                Id = 1,
                Question = "ログインできない",
                AiAnswer = "パスワード再設定を確認してください。",
                IsSuccess = true,
                ExecutedAt = DateTime.UtcNow
            },
            new AiSearchHistory
            {
                Id = 2,
                Question = "CSV取込エラー",
                AiAnswer = "文字コードを確認してください。",
                IsSuccess = true,
                ExecutedAt = DateTime.UtcNow.AddMinutes(-1)
            });

        await dbContext.SaveChangesAsync();

        var service = new AiSearchHistoryService(dbContext);

        var query = new AiSearchHistoryQuery
        {
            Keyword = "CSV",
            Page = 1,
            PageSize = 20
        };

        // Act
        var result = await service.GetListAsync(query);

        // Assert
        Assert.Single(result);
        Assert.Equal("CSV取込エラー", result[0].Question);
    }

    [Fact]
    public async Task GetListAsync_WithIsSuccess_ReturnsFilteredHistories()
    {
        // Arrange
        await using var dbContext = CreateDbContext();

        dbContext.AiSearchHistories.AddRange(
            new AiSearchHistory
            {
                Id = 1,
                Question = "成功した検索",
                AiAnswer = "回答あり",
                IsSuccess = true,
                ExecutedAt = DateTime.UtcNow
            },
            new AiSearchHistory
            {
                Id = 2,
                Question = "失敗した検索",
                AiAnswer = null,
                IsSuccess = false,
                ErrorMessage = "AI回答の生成に失敗しました。",
                ExecutedAt = DateTime.UtcNow.AddMinutes(-1)
            });

        await dbContext.SaveChangesAsync();

        var service = new AiSearchHistoryService(dbContext);

        var query = new AiSearchHistoryQuery
        {
            IsSuccess = false,
            Page = 1,
            PageSize = 20
        };

        // Act
        var result = await service.GetListAsync(query);

        // Assert
        Assert.Single(result);
        Assert.False(result[0].IsSuccess);
        Assert.Equal("失敗した検索", result[0].Question);
    }

    [Fact]
    public async Task GetDetailAsync_ExistingHistory_ReturnsDetail()
    {
        // Arrange
        await using var dbContext = CreateDbContext();

        dbContext.AiSearchHistories.Add(new AiSearchHistory
        {
            Id = 1,
            Question = "ログインできない",
            SearchKeywords = "ログイン パスワード",
            AiAnswer = "パスワード再設定を確認してください。",
            IsSuccess = true,
            ErrorMessage = null,
            IsHelpful = null,
            ExecutedAt = DateTime.UtcNow,
            Sources = new List<AiSearchHistorySource>
            {
                new AiSearchHistorySource
                {
                    Id = 1,
                    FaqId = 20,
                    FaqTitle = "ログインできない場合",
                    DisplayOrder = 2,
                    Score = 5
                },
                new AiSearchHistorySource
                {
                    Id = 2,
                    FaqId = 10,
                    FaqTitle = "パスワード再設定",
                    DisplayOrder = 1,
                    Score = 10
                }
            }
        });

        await dbContext.SaveChangesAsync();

        var service = new AiSearchHistoryService(dbContext);

        // Act
        var result = await service.GetDetailAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result!.Id);
        Assert.Equal("ログインできない", result.Question);
        Assert.Equal("ログイン パスワード", result.SearchKeywords);
        Assert.Equal("パスワード再設定を確認してください。", result.AiAnswer);
        Assert.True(result.IsSuccess);

        Assert.Equal(2, result.Sources.Count);
        Assert.Equal(10, result.Sources[0].FaqId);
        Assert.Equal("/faqs/10", result.Sources[0].Url);
        Assert.Equal(20, result.Sources[1].FaqId);
        Assert.Equal("/faqs/20", result.Sources[1].Url);
    }

    [Fact]
    public async Task GetDetailAsync_NotFound_ReturnsNull()
    {
        // Arrange
        await using var dbContext = CreateDbContext();

        var service = new AiSearchHistoryService(dbContext);

        // Act
        var result = await service.GetDetailAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateFeedbackAsync_ExistingHistory_UpdatesIsHelpful()
    {
        // Arrange
        await using var dbContext = CreateDbContext();

        dbContext.AiSearchHistories.Add(new AiSearchHistory
        {
            Id = 1,
            Question = "ログインできない",
            AiAnswer = "回答本文",
            IsSuccess = true,
            IsHelpful = null,
            ExecutedAt = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync();

        var service = new AiSearchHistoryService(dbContext);

        // Act
        await service.UpdateFeedbackAsync(1, true);

        // Assert
        var history = await dbContext.AiSearchHistories.FirstAsync(x => x.Id == 1);
        Assert.True(history.IsHelpful);
    }

    [Fact]
    public async Task UpdateFeedbackAsync_NotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        await using var dbContext = CreateDbContext();

        var service = new AiSearchHistoryService(dbContext);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.UpdateFeedbackAsync(999, true));

        Assert.Equal("AI検索履歴が見つかりません。", ex.Message);
    }
}
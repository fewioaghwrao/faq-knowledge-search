using System.Reflection;
using FaqKnowledgeSearch.Application.AiSearch.Feedback;
using FaqKnowledgeSearch.Application.AiSearch.History;
using FaqKnowledgeSearch.Domain.AiSearch;
using Moq;

namespace FaqKnowledgeSearch.Application.Tests.AiSearch.Feedback;

public sealed class AiSearchFeedbackServiceTests
{
    private readonly Mock<IAiSearchHistoryRepository> _repositoryMock;
    private readonly AiSearchFeedbackService _sut;

    public AiSearchFeedbackServiceTests()
    {
        _repositoryMock =
            new Mock<IAiSearchHistoryRepository>(
                MockBehavior.Strict);

        _sut =
            new AiSearchFeedbackService(
                _repositoryMock.Object);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task SetFeedbackAsync_HistoryIdIsZeroOrNegative_ReturnsNotFound(
        long historyId)
    {
        // Act
        var result =
            await _sut.SetFeedbackAsync(
                historyId,
                wasHelpful: true);

        // Assert
        Assert.Equal(
            AiSearchFeedbackResult.NotFound,
            result);

        _repositoryMock.Verify(
            x => x.GetByIdForUpdateAsync(
                It.IsAny<long>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _repositoryMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task SetFeedbackAsync_HistoryDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        const long historyId = 1;
        var cancellationToken = new CancellationToken();

        _repositoryMock
            .Setup(x => x.GetByIdForUpdateAsync(
                historyId,
                cancellationToken))
            .ReturnsAsync((AiSearchHistory?)null);

        // Act
        var result =
            await _sut.SetFeedbackAsync(
                historyId,
                wasHelpful: true,
                cancellationToken);

        // Assert
        Assert.Equal(
            AiSearchFeedbackResult.NotFound,
            result);

        _repositoryMock.Verify(
            x => x.GetByIdForUpdateAsync(
                historyId,
                cancellationToken),
            Times.Once);

        _repositoryMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task SetFeedbackAsync_HistoryIsFailure_ReturnsNotEligible()
    {
        // Arrange
        const long historyId = 1;

        var history =
            CreateHistory(
                isSuccess: false,
                usedExternalAi: true);

        _repositoryMock
            .Setup(x => x.GetByIdForUpdateAsync(
                historyId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(history);

        // Act
        var result =
            await _sut.SetFeedbackAsync(
                historyId,
                wasHelpful: true);

        // Assert
        Assert.Equal(
            AiSearchFeedbackResult.NotEligible,
            result);

        _repositoryMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task SetFeedbackAsync_ExternalAiWasNotUsed_ReturnsNotEligible()
    {
        // Arrange
        const long historyId = 1;

        var history =
            CreateHistory(
                isSuccess: true,
                usedExternalAi: false);

        _repositoryMock
            .Setup(x => x.GetByIdForUpdateAsync(
                historyId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(history);

        // Act
        var result =
            await _sut.SetFeedbackAsync(
                historyId,
                wasHelpful: true);

        // Assert
        Assert.Equal(
            AiSearchFeedbackResult.NotEligible,
            result);

        _repositoryMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task SetFeedbackAsync_EligibleHistory_ReturnsSuccessAndSavesChanges(
        bool wasHelpful)
    {
        // Arrange
        const long historyId = 1;
        var cancellationToken = new CancellationToken();

        var history =
            CreateHistory(
                isSuccess: true,
                usedExternalAi: true);

        _repositoryMock
            .Setup(x => x.GetByIdForUpdateAsync(
                historyId,
                cancellationToken))
            .ReturnsAsync(history);

        _repositoryMock
            .Setup(x => x.SaveChangesAsync(
                cancellationToken))
            .Returns(Task.CompletedTask);

        // Act
        var result =
            await _sut.SetFeedbackAsync(
                historyId,
                wasHelpful,
                cancellationToken);

        // Assert
        Assert.Equal(
            AiSearchFeedbackResult.Success,
            result);

        _repositoryMock.Verify(
            x => x.GetByIdForUpdateAsync(
                historyId,
                cancellationToken),
            Times.Once);

        _repositoryMock.Verify(
            x => x.SaveChangesAsync(
                cancellationToken),
            Times.Once);
    }

    private static AiSearchHistory CreateHistory(
        bool isSuccess,
        bool usedExternalAi)
    {
        var history =
            Activator.CreateInstance(
                typeof(AiSearchHistory),
                nonPublic: true)
            as AiSearchHistory;

        Assert.NotNull(history);

        SetProperty(
            history,
            nameof(AiSearchHistory.IsSuccess),
            isSuccess);

        SetProperty(
            history,
            nameof(AiSearchHistory.UsedExternalAi),
            usedExternalAi);

        return history;
    }

    private static void SetProperty<T>(
        object target,
        string propertyName,
        T value)
    {
        var property =
            target.GetType().GetProperty(
                propertyName,
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic);

        Assert.NotNull(property);

        property.SetValue(
            target,
            value);
    }
}
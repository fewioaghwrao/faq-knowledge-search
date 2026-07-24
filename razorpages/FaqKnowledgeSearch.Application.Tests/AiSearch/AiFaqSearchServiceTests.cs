using FaqKnowledgeSearch.Application.AiSearch;
using FaqKnowledgeSearch.Application.AiSearch.History;
using FaqKnowledgeSearch.Domain.AiSearch;
using Moq;

namespace FaqKnowledgeSearch.Application.Tests.AiSearch;

public sealed class AiFaqSearchServiceTests
{
    private const string ModelName = "test-model";
    private const int MaxContextFaqCount = 3;

    private readonly Mock<IAiFaqCandidateQuery> _candidateQueryMock;
    private readonly Mock<IAiAnswerGenerator> _answerGeneratorMock;
    private readonly Mock<IAiSearchHistoryRepository> _historyRepositoryMock;
    private readonly AiFaqSearchService _sut;

    public AiFaqSearchServiceTests()
    {
        _candidateQueryMock =
            new Mock<IAiFaqCandidateQuery>(
                MockBehavior.Strict);

        _answerGeneratorMock =
            new Mock<IAiAnswerGenerator>(
                MockBehavior.Strict);

        _historyRepositoryMock =
            new Mock<IAiSearchHistoryRepository>(
                MockBehavior.Strict);

        var options =
            new AiFaqSearchOptions
            {
                MaxContextFaqCount = MaxContextFaqCount,
                ModelName = ModelName
            };

        _sut =
            new AiFaqSearchService(
                _candidateQueryMock.Object,
                _answerGeneratorMock.Object,
                _historyRepositoryMock.Object,
                options);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\r\n")]
    [InlineData("\t")]
    public async Task SearchAsync_QuestionIsNullOrWhiteSpace_ThrowsArgumentException(
        string question)
    {
        // Act
        var exception =
            await Assert.ThrowsAsync<ArgumentException>(
                () => _sut.SearchAsync(question));

        // Assert
        Assert.Equal(
            "question",
            exception.ParamName);

        Assert.Contains(
            "質問・検索キーワードを入力してください。",
            exception.Message);

        _candidateQueryMock.Verify(
            x => x.SearchAsync(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _answerGeneratorMock.Verify(
            x => x.GenerateAsync(
                It.IsAny<string>(),
                It.IsAny<IReadOnlyList<AiFaqReference>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _historyRepositoryMock.Verify(
            x => x.AddAsync(
                It.IsAny<AiSearchHistory>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _historyRepositoryMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task SearchAsync_TrimmedQuestionExceeds500Characters_ThrowsArgumentException()
    {
        // Arrange
        var question =
            "  " +
            new string('あ', 501) +
            "  ";

        // Act
        var exception =
            await Assert.ThrowsAsync<ArgumentException>(
                () => _sut.SearchAsync(question));

        // Assert
        Assert.Equal(
            "question",
            exception.ParamName);

        Assert.Contains(
            "質問は500文字以内で入力してください。",
            exception.Message);

        _candidateQueryMock.Verify(
            x => x.SearchAsync(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task SearchAsync_QuestionIsExactly500Characters_SearchesNormally()
    {
        // Arrange
        var question =
            new string('あ', 500);

        var cancellationToken =
            CancellationToken.None;

        _candidateQueryMock
            .Setup(x => x.SearchAsync(
                question,
                MaxContextFaqCount,
                cancellationToken))
            .ReturnsAsync(
                Array.Empty<AiFaqReference>());

        SetupSuccessfulHistorySave(
            cancellationToken);

        // Act
        var result =
            await _sut.SearchAsync(
                question,
                cancellationToken);

        // Assert
        Assert.Equal(question, result.Question);

        _candidateQueryMock.Verify(
            x => x.SearchAsync(
                question,
                MaxContextFaqCount,
                cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task SearchAsync_QuestionHasLeadingAndTrailingSpaces_TrimsQuestion()
    {
        // Arrange
        const string originalQuestion =
            "  パスワードを変更したい  ";

        const string trimmedQuestion =
            "パスワードを変更したい";

        var cancellationToken =
            CancellationToken.None;

        _candidateQueryMock
            .Setup(x => x.SearchAsync(
                trimmedQuestion,
                MaxContextFaqCount,
                cancellationToken))
            .ReturnsAsync(
                Array.Empty<AiFaqReference>());

        SetupSuccessfulHistorySave(
            cancellationToken);

        // Act
        var result =
            await _sut.SearchAsync(
                originalQuestion,
                cancellationToken);

        // Assert
        Assert.Equal(
            trimmedQuestion,
            result.Question);

        _candidateQueryMock.Verify(
            x => x.SearchAsync(
                trimmedQuestion,
                MaxContextFaqCount,
                cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task SearchAsync_NoReferences_ReturnsGuidanceWithoutUsingExternalAi()
    {
        // Arrange
        const string question =
            "存在しないFAQ";

        const string expectedAnswer =
            "質問に関連する公開FAQが見つかりませんでした。"
            + "キーワードを短くするか、"
            + "通常のFAQ検索をお試しください。";

        var cancellationToken =
            CancellationToken.None;

        AiSearchHistory? addedHistory = null;

        _candidateQueryMock
            .Setup(x => x.SearchAsync(
                question,
                MaxContextFaqCount,
                cancellationToken))
            .ReturnsAsync(
                Array.Empty<AiFaqReference>());

        _historyRepositoryMock
            .Setup(x => x.AddAsync(
                It.IsAny<AiSearchHistory>(),
                cancellationToken))
            .Callback<AiSearchHistory, CancellationToken>(
                (history, _) =>
                    addedHistory = history)
            .Returns(Task.CompletedTask);

        _historyRepositoryMock
            .Setup(x => x.SaveChangesAsync(
                cancellationToken))
            .Returns(Task.CompletedTask);

        // Act
        var result =
            await _sut.SearchAsync(
                question,
                cancellationToken);

        // Assert
        Assert.Equal(question, result.Question);
        Assert.Equal(expectedAnswer, result.Answer);
        Assert.Empty(result.References);
        Assert.False(result.UsedExternalAi);

        Assert.NotNull(addedHistory);
        Assert.True(addedHistory!.IsSuccess);
        Assert.False(addedHistory.UsedExternalAi);

        _answerGeneratorMock.Verify(
            x => x.GenerateAsync(
                It.IsAny<string>(),
                It.IsAny<IReadOnlyList<AiFaqReference>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _historyRepositoryMock.Verify(
            x => x.AddAsync(
                It.IsAny<AiSearchHistory>(),
                cancellationToken),
            Times.Once);

        _historyRepositoryMock.Verify(
            x => x.SaveChangesAsync(
                cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task SearchAsync_ReferencesExist_ReturnsGeneratedAnswer()
    {
        // Arrange
        const string question =
            "パスワードの変更方法は？";

        const string generatedAnswer =
            "設定画面のアカウント設定から変更できます。";

        var cancellationToken =
            CancellationToken.None;

        IReadOnlyList<AiFaqReference> references =
        [
            CreateReference(
                id: 10,
                title: "パスワード変更手順",
                score: 100),

            CreateReference(
                id: 20,
                title: "アカウント設定",
                score: 80)
        ];

        AiSearchHistory? addedHistory = null;

        _candidateQueryMock
            .Setup(x => x.SearchAsync(
                question,
                MaxContextFaqCount,
                cancellationToken))
            .ReturnsAsync(references);

        _answerGeneratorMock
            .Setup(x => x.GenerateAsync(
                question,
                references,
                cancellationToken))
            .ReturnsAsync(generatedAnswer);

        _historyRepositoryMock
            .Setup(x => x.AddAsync(
                It.IsAny<AiSearchHistory>(),
                cancellationToken))
            .Callback<AiSearchHistory, CancellationToken>(
                (history, _) =>
                    addedHistory = history)
            .Returns(Task.CompletedTask);

        _historyRepositoryMock
            .Setup(x => x.SaveChangesAsync(
                cancellationToken))
            .Returns(Task.CompletedTask);

        // Act
        var result =
            await _sut.SearchAsync(
                question,
                cancellationToken);

        // Assert
        Assert.Equal(question, result.Question);
        Assert.Equal(generatedAnswer, result.Answer);
        Assert.Same(references, result.References);
        Assert.True(result.UsedExternalAi);

        Assert.NotNull(addedHistory);
        Assert.True(addedHistory!.IsSuccess);
        Assert.True(addedHistory.UsedExternalAi);

        _candidateQueryMock.Verify(
            x => x.SearchAsync(
                question,
                MaxContextFaqCount,
                cancellationToken),
            Times.Once);

        _answerGeneratorMock.Verify(
            x => x.GenerateAsync(
                question,
                references,
                cancellationToken),
            Times.Once);

        _historyRepositoryMock.Verify(
            x => x.AddAsync(
                It.IsAny<AiSearchHistory>(),
                cancellationToken),
            Times.Once);

        _historyRepositoryMock.Verify(
            x => x.SaveChangesAsync(
                cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task SearchAsync_RequestIsCancelled_RethrowsWithoutSavingFailureHistory()
    {
        // Arrange
        const string question =
            "キャンセルされる質問";

        IReadOnlyList<AiFaqReference> references =
        [
            CreateReference()
        ];

        using var cancellationTokenSource =
            new CancellationTokenSource();

        cancellationTokenSource.Cancel();

        var cancellationToken =
            cancellationTokenSource.Token;

        _candidateQueryMock
            .Setup(x => x.SearchAsync(
                question,
                MaxContextFaqCount,
                cancellationToken))
            .ReturnsAsync(references);

        _answerGeneratorMock
            .Setup(x => x.GenerateAsync(
                question,
                references,
                cancellationToken))
            .ThrowsAsync(
                new OperationCanceledException(
                    cancellationToken));

        // Act
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _sut.SearchAsync(
                question,
                cancellationToken));

        // Assert
        _historyRepositoryMock.Verify(
            x => x.AddAsync(
                It.IsAny<AiSearchHistory>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _historyRepositoryMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task SearchAsync_AnswerGeneratorThrowsKnownException_SavesFailureAndRethrowsSameException()
    {
        // Arrange
        const string question =
            "AI回答生成に失敗する質問";

        var cancellationToken =
            CancellationToken.None;

        IReadOnlyList<AiFaqReference> references =
        [
            CreateReference()
        ];

        var expectedException =
            new AiAnswerGenerationException(
                "外部AIサービスとの通信に失敗しました。");

        AiSearchHistory? addedHistory = null;

        _candidateQueryMock
            .Setup(x => x.SearchAsync(
                question,
                MaxContextFaqCount,
                cancellationToken))
            .ReturnsAsync(references);

        _answerGeneratorMock
            .Setup(x => x.GenerateAsync(
                question,
                references,
                cancellationToken))
            .ThrowsAsync(expectedException);

        SetupFailureHistorySave(
            cancellationToken,
            history => addedHistory = history);

        // Act
        var actualException =
            await Assert.ThrowsAsync<AiAnswerGenerationException>(
                () => _sut.SearchAsync(
                    question,
                    cancellationToken));

        // Assert
        Assert.Same(
            expectedException,
            actualException);

        Assert.NotNull(addedHistory);
        Assert.False(addedHistory!.IsSuccess);
        Assert.True(addedHistory.UsedExternalAi);

        _historyRepositoryMock.Verify(
            x => x.AddAsync(
                It.IsAny<AiSearchHistory>(),
                cancellationToken),
            Times.Once);

        _historyRepositoryMock.Verify(
            x => x.SaveChangesAsync(
                cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task SearchAsync_AnswerGeneratorThrowsUnexpectedException_SavesFailureAndWrapsException()
    {
        // Arrange
        const string question =
            "予期しないエラーになる質問";

        const string expectedMessage =
            "AI回答の生成中に予期しないエラーが発生しました。";

        var cancellationToken =
            CancellationToken.None;

        IReadOnlyList<AiFaqReference> references =
        [
            CreateReference()
        ];

        var originalException =
            new InvalidOperationException(
                "想定外の内部エラー");

        AiSearchHistory? addedHistory = null;

        _candidateQueryMock
            .Setup(x => x.SearchAsync(
                question,
                MaxContextFaqCount,
                cancellationToken))
            .ReturnsAsync(references);

        _answerGeneratorMock
            .Setup(x => x.GenerateAsync(
                question,
                references,
                cancellationToken))
            .ThrowsAsync(originalException);

        SetupFailureHistorySave(
            cancellationToken,
            history => addedHistory = history);

        // Act
        var exception =
            await Assert.ThrowsAsync<AiAnswerGenerationException>(
                () => _sut.SearchAsync(
                    question,
                    cancellationToken));

        // Assert
        Assert.Equal(
            expectedMessage,
            exception.Message);

        Assert.Same(
            originalException,
            exception.InnerException);

        Assert.NotNull(addedHistory);
        Assert.False(addedHistory!.IsSuccess);
        Assert.True(addedHistory.UsedExternalAi);

        _historyRepositoryMock.Verify(
            x => x.AddAsync(
                It.IsAny<AiSearchHistory>(),
                cancellationToken),
            Times.Once);

        _historyRepositoryMock.Verify(
            x => x.SaveChangesAsync(
                cancellationToken),
            Times.Once);
    }

    private void SetupSuccessfulHistorySave(
        CancellationToken cancellationToken)
    {
        _historyRepositoryMock
            .Setup(x => x.AddAsync(
                It.IsAny<AiSearchHistory>(),
                cancellationToken))
            .Returns(Task.CompletedTask);

        _historyRepositoryMock
            .Setup(x => x.SaveChangesAsync(
                cancellationToken))
            .Returns(Task.CompletedTask);
    }

    private void SetupFailureHistorySave(
        CancellationToken cancellationToken,
        Action<AiSearchHistory> capture)
    {
        _historyRepositoryMock
            .Setup(x => x.AddAsync(
                It.IsAny<AiSearchHistory>(),
                cancellationToken))
            .Callback<AiSearchHistory, CancellationToken>(
                (history, _) =>
                    capture(history))
            .Returns(Task.CompletedTask);

        _historyRepositoryMock
            .Setup(x => x.SaveChangesAsync(
                cancellationToken))
            .Returns(Task.CompletedTask);
    }

    private static AiFaqReference CreateReference(
        int id = 1,
        string title = "テストFAQ",
        int score = 100)
    {
        return new AiFaqReference(
            Id: id,
            Title: title,
            Body: "テスト用のFAQ本文です。",
            CategoryName: "テストカテゴリ",
            Tags: ["テスト"],
            Score: score);
    }
}

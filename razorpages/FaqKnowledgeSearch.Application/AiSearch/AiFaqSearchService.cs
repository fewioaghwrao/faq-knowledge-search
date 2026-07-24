using FaqKnowledgeSearch.Application.AiSearch.History;
using FaqKnowledgeSearch.Domain.AiSearch;

namespace FaqKnowledgeSearch.Application.AiSearch;

public sealed class AiFaqSearchService(
    IAiFaqCandidateQuery candidateQuery,
    IAiAnswerGenerator answerGenerator,
    IAiSearchHistoryRepository historyRepository,
    AiFaqSearchOptions options)
    : IAiFaqSearchService
{
    public async Task<AiFaqSearchResult> SearchAsync(
        string question,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(question))
        {
            throw new ArgumentException(
                "質問・検索キーワードを入力してください。",
                nameof(question));
        }

        question = question.Trim();

        if (question.Length > 500)
        {
            throw new ArgumentException(
                "質問は500文字以内で入力してください。",
                nameof(question));
        }

        var references =
            await candidateQuery.SearchAsync(
                question,
                options.MaxContextFaqCount,
                cancellationToken);

        if (references.Count == 0)
        {
            const string noReferenceAnswer =
                "質問に関連する公開FAQが見つかりませんでした。"
                + "キーワードを短くするか、"
                + "通常のFAQ検索をお試しください。";

            var history =
                AiSearchHistory.CreateSuccess(
                    question,
                    noReferenceAnswer,
                    options.ModelName,
                    usedExternalAi: false);

            await SaveHistoryAsync(
                history,
                references,
                cancellationToken);

            return new AiFaqSearchResult(
                history.Id,
                question,
                noReferenceAnswer,
                references,
                UsedExternalAi: false);
        }

        string answer;

        try
        {
            answer =
                await answerGenerator.GenerateAsync(
                    question,
                    references,
                    cancellationToken);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            // 利用者によるキャンセルは失敗履歴にしない
            throw;
        }
        catch (AiAnswerGenerationException exception)
        {
            await SaveFailureHistoryAsync(
                question,
                exception.Message,
                references,
                cancellationToken);

            throw;
        }
        catch (Exception exception)
        {
            const string errorMessage =
                "AI回答の生成中に予期しないエラーが発生しました。";

            await SaveFailureHistoryAsync(
                question,
                errorMessage,
                references,
                cancellationToken);

            throw new AiAnswerGenerationException(
                errorMessage,
                exception);
        }

        var successHistory =
            AiSearchHistory.CreateSuccess(
                question,
                answer,
                options.ModelName,
                usedExternalAi: true);

        await SaveHistoryAsync(
            successHistory,
            references,
            cancellationToken);

        return new AiFaqSearchResult(
            successHistory.Id,
            question,
            answer,
            references,
            UsedExternalAi: true);
    }

    private async Task SaveFailureHistoryAsync(
        string question,
        string errorMessage,
        IReadOnlyList<AiFaqReference> references,
        CancellationToken cancellationToken)
    {
        // DB列の上限を超えないようにする
        var storedErrorMessage =
            errorMessage.Length <= 2_000
                ? errorMessage
                : errorMessage[..2_000];

        var history =
            AiSearchHistory.CreateFailure(
                question,
                storedErrorMessage,
                options.ModelName,
                usedExternalAi: true);

        await SaveHistoryAsync(
            history,
            references,
            cancellationToken);
    }

    private async Task SaveHistoryAsync(
        AiSearchHistory history,
        IReadOnlyList<AiFaqReference> references,
        CancellationToken cancellationToken)
    {
        for (var index = 0;
             index < references.Count;
             index++)
        {
            var reference = references[index];

            history.AddReference(
                reference.Id,
                reference.Title,
                reference.CategoryName,
                displayOrder: index + 1,
                reference.Score);
        }

        await historyRepository.AddAsync(
            history,
            cancellationToken);

        await historyRepository.SaveChangesAsync(
            cancellationToken);
    }
}
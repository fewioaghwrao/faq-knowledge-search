namespace FaqKnowledgeSearch.Application.AiSearch;

public interface IAiAnswerGenerator
{
    Task<string> GenerateAsync(
        string question,
        IReadOnlyList<AiFaqReference> references,
        CancellationToken cancellationToken = default);
}
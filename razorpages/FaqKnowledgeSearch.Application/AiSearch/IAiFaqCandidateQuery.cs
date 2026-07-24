namespace FaqKnowledgeSearch.Application.AiSearch;

public interface IAiFaqCandidateQuery
{
    Task<IReadOnlyList<AiFaqReference>> SearchAsync(
        string question,
        int maxResults,
        CancellationToken cancellationToken = default);
}
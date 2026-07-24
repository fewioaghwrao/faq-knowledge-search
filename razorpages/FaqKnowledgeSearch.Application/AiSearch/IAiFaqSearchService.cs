namespace FaqKnowledgeSearch.Application.AiSearch;

public interface IAiFaqSearchService
{
    Task<AiFaqSearchResult> SearchAsync(
        string question,
        CancellationToken cancellationToken = default);
}
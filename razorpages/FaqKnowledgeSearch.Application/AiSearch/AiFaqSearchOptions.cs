namespace FaqKnowledgeSearch.Application.AiSearch;

public sealed class AiFaqSearchOptions
{
    public int MaxContextFaqCount { get; init; } = 5;

    public string ModelName { get; init; } = "unknown";
}
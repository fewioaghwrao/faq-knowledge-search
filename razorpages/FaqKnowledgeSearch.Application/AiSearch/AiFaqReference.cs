namespace FaqKnowledgeSearch.Application.AiSearch;

public sealed record AiFaqReference(
    int Id,
    string Title,
    string Body,
    string CategoryName,
    IReadOnlyList<string> Tags,
    int Score)
{
    public string BodyPreview =>
        Body.Length <= 140
            ? Body
            : Body[..140] + "…";
}
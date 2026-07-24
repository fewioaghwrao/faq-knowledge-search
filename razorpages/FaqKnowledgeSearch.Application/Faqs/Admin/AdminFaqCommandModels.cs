namespace FaqKnowledgeSearch.Application.Faqs.Admin;

public sealed record AdminFaqCommand(
    string Title,
    string Body,
    int CategoryId,
    IReadOnlyCollection<int> TagIds,
    bool IsPublished);

public sealed record AdminFaqOption(
    int Id,
    string Name);

public sealed record AdminFaqFormOptions(
    IReadOnlyList<AdminFaqOption> Categories,
    IReadOnlyList<AdminFaqOption> Tags);

public sealed record AdminFaqEditData(
    int Id,
    string Title,
    string Body,
    int CategoryId,
    IReadOnlyList<int> TagIds,
    bool IsPublished);
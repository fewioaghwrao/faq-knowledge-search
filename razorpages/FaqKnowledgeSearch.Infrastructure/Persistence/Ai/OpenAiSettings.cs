namespace FaqKnowledgeSearch.Infrastructure.Ai;

public sealed class OpenAiSettings
{
    public string Provider { get; init; } = "OpenAI";

    public string Model { get; init; } = "gpt-5.4-mini";

    public string ApiKey { get; init; } = string.Empty;

    public int TimeoutSeconds { get; init; } = 30;

    public int MaxOutputTokens { get; init; } = 800;
}
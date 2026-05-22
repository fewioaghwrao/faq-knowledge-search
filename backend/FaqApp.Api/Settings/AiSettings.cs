namespace FaqApp.Api.Settings;

public class AiSettings
{
    public string Provider { get; set; } = "OpenAI";
    public string Model { get; set; } = "gpt-5.4-mini";
    public string ApiKey { get; set; } = string.Empty;
    public string Endpoint { get; set; } = "https://api.openai.com/v1/responses";
    public int TimeoutSeconds { get; set; } = 30;
    public int MaxContextFaqCount { get; set; } = 5;
}
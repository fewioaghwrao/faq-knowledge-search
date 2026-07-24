namespace FaqKnowledgeSearch.Application.AiSearch;

public sealed class AiAnswerGenerationException
    : Exception
{
    public AiAnswerGenerationException(
        string message)
        : base(message)
    {
    }

    public AiAnswerGenerationException(
        string message,
        Exception innerException)
        : base(message, innerException)
    {
    }
}
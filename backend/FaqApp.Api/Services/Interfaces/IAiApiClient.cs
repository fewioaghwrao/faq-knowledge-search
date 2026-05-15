namespace FaqApp.Api.Services.Interfaces;

public interface IAiApiClient
{
    Task<string> GenerateAnswerAsync(
        string question,
        IEnumerable<string> faqContexts,
        CancellationToken cancellationToken = default);
}

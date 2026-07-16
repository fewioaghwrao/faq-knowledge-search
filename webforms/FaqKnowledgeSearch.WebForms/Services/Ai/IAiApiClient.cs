using System.Threading.Tasks;

namespace FaqKnowledgeSearch.WebForms.Services.Ai
{
    public interface IAiApiClient
    {
        Task<string> GenerateAnswerAsync(
            string question,
            string faqContext);
    }
}
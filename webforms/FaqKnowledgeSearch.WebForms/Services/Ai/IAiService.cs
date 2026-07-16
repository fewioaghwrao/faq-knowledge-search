using System.Threading.Tasks;
using FaqKnowledgeSearch.WebForms.Dtos.Ai;

namespace FaqKnowledgeSearch.WebForms.Services.Ai
{
    public interface IAiService
    {
        Task<AiSearchResponse> SearchAsync(string question);
    }
}
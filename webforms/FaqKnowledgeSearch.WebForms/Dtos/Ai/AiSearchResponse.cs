using System.Collections.Generic;

namespace FaqKnowledgeSearch.WebForms.Dtos.Ai
{
    public class AiSearchResponse
    {
        public AiSearchResponse()
        {
            Sources = new List<AiSourceDto>();
        }

        public string Answer { get; set; }

        public string Disclaimer { get; set; }

        public IList<AiSourceDto> Sources { get; set; }

        public string Message { get; set; }

        public long AiHistoryId { get; set; }
    }
}
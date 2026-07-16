using System.Collections.Generic;

namespace FaqKnowledgeSearch.WebForms.Dtos
{
    public sealed class FaqEditDto
    {
        public FaqEditDto()
        {
            SelectedTagIds = new List<int>();
        }

        public long Id { get; set; }

        public long CategoryId { get; set; }

        public string Question { get; set; }

        public string Answer { get; set; }

        public bool IsPublished { get; set; }

        /// <summary>
        /// FAQへ設定するタグID一覧です。
        /// </summary>
        public IList<int> SelectedTagIds { get; set; }
    }
}
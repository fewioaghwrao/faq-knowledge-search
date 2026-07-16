using System;

namespace FaqKnowledgeSearch.WebForms.Dtos
{
    public class FaqListItemDto
    {
        public long Id { get; set; }

        public string CategoryName { get; set; }

        public string Question { get; set; }

        public string Answer { get; set; }

        public bool IsPublished { get; set; }

        public bool IsDeleted { get; set; }

        public int ViewCount { get; set; }

        public DateTime UpdatedAt { get; set; }

        public string TagNames { get; set; }
    }
}
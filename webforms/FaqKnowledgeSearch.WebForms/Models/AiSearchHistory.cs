using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FaqKnowledgeSearch.WebForms.Models
{
    [Table("ai_search_histories")]
    public class AiSearchHistory
    {
        public AiSearchHistory()
        {
            Sources =
                new HashSet<AiSearchHistorySource>();
        }

        [Key]
        [Column("id")]
        [DatabaseGenerated(
            DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [StringLength(500)]
        [Column("question")]
        public string Question { get; set; }

        [StringLength(500)]
        [Column("search_keywords")]
        public string SearchKeywords { get; set; }

        [Column("ai_answer")]
        public string AiAnswer { get; set; }

        [Column("is_success")]
        public bool IsSuccess { get; set; }

        [StringLength(1000)]
        [Column("error_message")]
        public string ErrorMessage { get; set; }

        [Column("executed_at")]
        public DateTime ExecutedAt { get; set; }

        public virtual ICollection<AiSearchHistorySource>
            Sources
        { get; set; }
    }
}
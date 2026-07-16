using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FaqKnowledgeSearch.WebForms.Models
{
    [Table("ai_search_feedbacks")]
    public class AiSearchFeedback
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(
            DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [Column("ai_search_history_id")]
        public long AiSearchHistoryId { get; set; }

        [Required]
        [Column("is_helpful")]
        public bool IsHelpful { get; set; }

        [StringLength(1000)]
        [Column("comment")]
        public string Comment { get; set; }

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Required]
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [ForeignKey("AiSearchHistoryId")]
        public virtual AiSearchHistory
            AiSearchHistory
        { get; set; }
    }
}
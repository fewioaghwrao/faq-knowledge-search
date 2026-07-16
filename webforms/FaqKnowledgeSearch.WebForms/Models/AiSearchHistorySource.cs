using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FaqKnowledgeSearch.WebForms.Models
{
    [Table("ai_search_history_sources")]
    public class AiSearchHistorySource
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(
            DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Column("ai_search_history_id")]
        public long AiSearchHistoryId { get; set; }

        [Column("faq_id")]
        public long FaqId { get; set; }

        [Required]
        [StringLength(500)]
        [Column("faq_question")]
        public string FaqQuestion { get; set; }

        [Column("faq_answer")]
        public string FaqAnswer { get; set; }

        [StringLength(100)]
        [Column("category_name")]
        public string CategoryName { get; set; }

        [Column("display_order")]
        public int DisplayOrder { get; set; }

        [ForeignKey("AiSearchHistoryId")]
        public virtual AiSearchHistory
            AiSearchHistory
        { get; set; }
    }
}
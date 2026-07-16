using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FaqKnowledgeSearch.WebForms.Models
{
    [Table("faqs")]
    public class Faq
    {
        public Faq()
        {
            Tags = new HashSet<Tag>();
        }

        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Column("category_id")]
        public long CategoryId { get; set; }

        [Required]
        [MaxLength(500)]
        [Column("question")]
        public string Question { get; set; }

        [Required]
        [Column("answer")]
        public string Answer { get; set; }

        [Column("is_published")]
        public bool IsPublished { get; set; }

        [Column("is_deleted")]
        public bool IsDeleted { get; set; }

        [Column("view_count")]
        public int ViewCount { get; set; }

        [Column("created_at")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime UpdatedAt { get; set; }

        [ForeignKey(nameof(CategoryId))]
        public virtual Category Category { get; set; }

        /// <summary>
        /// FAQに設定されているタグです。
        /// </summary>
        public virtual ICollection<Tag> Tags { get; set; }
    }
}
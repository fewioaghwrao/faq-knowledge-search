using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FaqKnowledgeSearch.WebForms.Models
{
    [Table("tags")]
    public class Tag
    {
        public Tag()
        {
            Faqs = new HashSet<Faq>();
        }

        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("name")]
        public string Name { get; set; }

        [Column("display_order")]
        public int DisplayOrder { get; set; }

        /// <summary>
        /// このタグが設定されているFAQです。
        /// </summary>
        public virtual ICollection<Faq> Faqs { get; set; }
    }
}
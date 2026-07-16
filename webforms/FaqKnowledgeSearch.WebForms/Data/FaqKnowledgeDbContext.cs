using System.Data.Entity;
using FaqKnowledgeSearch.WebForms.Models;
using MySql.Data.EntityFramework;

namespace FaqKnowledgeSearch.WebForms.Data
{
    [DbConfigurationType(typeof(MySqlEFConfiguration))]
    public sealed class FaqKnowledgeDbContext : DbContext
    {
        static FaqKnowledgeDbContext()
        {
            // DB構造はdatabase/*.sqlで管理する。
            // EFによるDBの自動作成・自動変更は行わない。
            Database.SetInitializer<FaqKnowledgeDbContext>(null);
        }

        public FaqKnowledgeDbContext()
            : base("name=FaqKnowledgeDb")
        {
        }

        public DbSet<Category> Categories { get; set; }

        public DbSet<Faq> Faqs { get; set; }

        public DbSet<Tag> Tags { get; set; }

        public DbSet<AiSearchHistory>
    AiSearchHistories
        { get; set; }

        public DbSet<AiSearchHistorySource>
            AiSearchHistorySources
        { get; set; }

        public DbSet<AiSearchFeedback>
    AiSearchFeedbacks
        { get; set; }

        protected override void OnModelCreating(
            DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Faq>()
                .HasRequired(x => x.Category)
                .WithMany(x => x.Faqs)
                .HasForeignKey(x => x.CategoryId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Faq>()
                .HasMany(x => x.Tags)
                .WithMany(x => x.Faqs)
                .Map(mapping =>
                {
                    mapping.ToTable("faq_tags");
                    mapping.MapLeftKey("faq_id");
                    mapping.MapRightKey("tag_id");
                });
            modelBuilder.Entity<AiSearchHistory>()
    .HasMany(x => x.Sources)
    .WithRequired(x => x.AiSearchHistory)
    .HasForeignKey(x => x.AiSearchHistoryId)
    .WillCascadeOnDelete(true);
        }


    }
}
using System.Data.Entity;
using Microsoft.AspNet.Identity.EntityFramework;
using MySql.Data.EntityFramework;

namespace FaqKnowledgeSearch.WebForms.Identity
{
    /// <summary>
    /// ASP.NET Identity用のデータベースコンテキストです。
    /// </summary>
    [DbConfigurationType(typeof(MySqlEFConfiguration))]
    public class ApplicationIdentityDbContext
        : IdentityDbContext<ApplicationUser>
    {
        public ApplicationIdentityDbContext()
            : base("FaqKnowledgeDb", throwIfV1Schema: false)
        {
        }

        public static ApplicationIdentityDbContext Create()
        {
            return new ApplicationIdentityDbContext();
        }
    }
}
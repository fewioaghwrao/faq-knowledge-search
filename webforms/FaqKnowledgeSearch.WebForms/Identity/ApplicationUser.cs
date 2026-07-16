using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace FaqKnowledgeSearch.WebForms.Identity
{
    /// <summary>
    /// アプリケーションのログインユーザーを表します。
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        public ApplicationUser()
        {
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// 管理画面などに表示するユーザー名です。
        /// </summary>
        [Required]
        [StringLength(100)]
        public string DisplayName { get; set; }

        /// <summary>
        /// ユーザーが有効かどうかを表します。
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// ユーザーの作成日時です。
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 認証Cookieへ格納するユーザーIdentityを生成します。
        /// </summary>
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(
            UserManager<ApplicationUser> manager)
        {
            return await manager.CreateIdentityAsync(
                this,
                DefaultAuthenticationTypes.ApplicationCookie);
        }
    }
}
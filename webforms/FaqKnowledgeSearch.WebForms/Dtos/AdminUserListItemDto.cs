using System;

namespace FaqKnowledgeSearch.WebForms.Dtos
{
    /// <summary>
    /// 管理画面のユーザー一覧に表示するユーザー情報です。
    /// </summary>
    public class AdminUserListItemDto
    {
        /// <summary>
        /// ASP.NET IdentityのユーザーIDです。
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 画面に表示するユーザー名です。
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// メールアドレスです。
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// ユーザーに付与されているロール名です。
        /// </summary>
        public string RoleName { get; set; }

        /// <summary>
        /// ユーザーが有効かどうかを表します。
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// ユーザー作成日時です。
        /// DBにはUTCで保存されています。
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Adminロールを持つユーザーかどうかを表します。
        /// </summary>
        public bool IsAdmin
        {
            get
            {
                return string.Equals(
                    RoleName,
                    "Admin",
                    StringComparison.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// 有効・無効を変更可能かどうかを表します。
        /// Adminユーザーは変更できません。
        /// </summary>
        public bool CanChangeStatus
        {
            get
            {
                return !IsAdmin;
            }
        }
    }
}
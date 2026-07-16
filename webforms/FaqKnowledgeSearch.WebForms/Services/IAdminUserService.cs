using System.Threading.Tasks;
using FaqKnowledgeSearch.WebForms.Dtos;
using Microsoft.AspNet.Identity;

namespace FaqKnowledgeSearch.WebForms.Services
{
    /// <summary>
    /// 管理画面のユーザー管理処理を定義します。
    /// </summary>
    public interface IAdminUserService
    {
        /// <summary>
        /// ユーザー一覧をページ単位で取得します。
        /// </summary>
        /// <param name="pageNumber">
        /// 取得するページ番号。1から開始します。
        /// </param>
        /// <param name="pageSize">
        /// 1ページあたりの表示件数です。
        /// </param>
        AdminUserPageDto GetPage(
            int pageNumber,
            int pageSize);

        /// <summary>
        /// 指定ユーザーの有効・無効状態を変更します。
        /// </summary>
        /// <param name="userId">
        /// ASP.NET IdentityのユーザーIDです。
        /// </param>
        /// <param name="isActive">
        /// trueの場合は有効、falseの場合は無効にします。
        /// </param>
        Task<IdentityResult> SetActiveAsync(
            string userId,
            bool isActive);
    }
}
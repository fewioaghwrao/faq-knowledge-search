using System;
using System.Collections.Generic;

namespace FaqKnowledgeSearch.WebForms.Dtos
{
    /// <summary>
    /// ユーザー一覧のページング結果です。
    /// </summary>
    public class AdminUserPageDto
    {
        public AdminUserPageDto()
        {
            Items = new List<AdminUserListItemDto>();
        }

        /// <summary>
        /// 現在のページに表示するユーザー一覧です。
        /// </summary>
        public IList<AdminUserListItemDto> Items { get; set; }

        /// <summary>
        /// 検索対象となるユーザーの総件数です。
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 現在のページ番号です。1から開始します。
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// 1ページあたりの表示件数です。
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// 総ページ数です。
        /// </summary>
        public int TotalPages
        {
            get
            {
                if (PageSize <= 0)
                {
                    return 0;
                }

                return (int)Math.Ceiling(
                    TotalCount / (double)PageSize);
            }
        }

        /// <summary>
        /// 前のページが存在するかどうかを表します。
        /// </summary>
        public bool HasPreviousPage
        {
            get
            {
                return PageNumber > 1;
            }
        }

        /// <summary>
        /// 次のページが存在するかどうかを表します。
        /// </summary>
        public bool HasNextPage
        {
            get
            {
                return PageNumber < TotalPages;
            }
        }
    }
}
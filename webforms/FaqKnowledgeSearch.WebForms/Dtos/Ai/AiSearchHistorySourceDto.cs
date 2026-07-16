namespace FaqKnowledgeSearch.WebForms.Dtos.Ai
{
    public class AiSearchHistorySourceDto
    {
        public long FaqId { get; set; }

        public string FaqQuestion { get; set; }

        public string FaqAnswer { get; set; }

        public string CategoryName { get; set; }

        public int DisplayOrder { get; set; }

        /// <summary>
        /// 現在の公開FAQ詳細画面へのURLです。
        /// FAQが非公開・削除済みの場合はリンク先で
        /// 見つからない扱いになる可能性があります。
        /// </summary>
        public string Url { get; set; }
    }
}
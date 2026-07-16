using System;

namespace FaqKnowledgeSearch.WebForms.Dtos.Ai
{
    public class AiSearchHistoryListItemDto
    {
        public long Id { get; set; }

        public string Question { get; set; }

        /// <summary>
        /// 一覧画面に表示するAI回答の要約です。
        /// 失敗時や回答がない場合はnullになります。
        /// </summary>
        public string AnswerPreview { get; set; }

        public bool IsSuccess { get; set; }

        public string ErrorMessage { get; set; }

        public int SourceCount { get; set; }

        /// <summary>
        /// AI回答に対するフィードバックです。
        /// true: 役に立った
        /// false: 役に立たなかった
        /// null: 未評価
        /// </summary>
        public bool? IsHelpful { get; set; }

        /// <summary>
        /// DBにはUTCで保存された日時を設定します。
        /// 画面表示時に日本時間へ変換します。
        /// </summary>
        public DateTime ExecutedAt { get; set; }
    }
}
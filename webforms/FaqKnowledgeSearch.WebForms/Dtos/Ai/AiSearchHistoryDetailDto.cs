using System;
using System.Collections.Generic;

namespace FaqKnowledgeSearch.WebForms.Dtos.Ai
{
    public class AiSearchHistoryDetailDto
    {
        public AiSearchHistoryDetailDto()
        {
            Sources =
                new List<AiSearchHistorySourceDto>();
        }

        public long Id { get; set; }

        public string Question { get; set; }

        public string SearchKeywords { get; set; }

        public string AiAnswer { get; set; }

        public bool IsSuccess { get; set; }

        public string ErrorMessage { get; set; }

        /// <summary>
        /// DBにはUTCで保存された日時を設定します。
        /// </summary>
        public DateTime ExecutedAt { get; set; }

        public IList<AiSearchHistorySourceDto>
            Sources
        { get; set; }

        /// <summary>
        /// AI回答に対するフィードバックです。
        /// true: 役に立った
        /// false: 役に立たなかった
        /// null: 未評価
        /// </summary>
        public bool? IsHelpful { get; set; }
    }
}
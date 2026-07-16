using System;
using System.Web;
using System.Web.UI.WebControls;
using FaqKnowledgeSearch.WebForms.Services.Ai;

namespace FaqKnowledgeSearch.WebForms.Admin.AiHistories
{
    public partial class Index : AdminPageBase
    {
        private static readonly TimeZoneInfo JapanTimeZone =
            TimeZoneInfo.FindSystemTimeZoneById(
                "Tokyo Standard Time");

        private IAiSearchHistoryQueryService
            _historyQueryService;

        protected void Page_Init(
            object sender,
            EventArgs e)
        {
            _historyQueryService =
                new AiSearchHistoryQueryService();
        }

        protected void Page_Load(
            object sender,
            EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadHistories();
            }
        }

        protected void SearchButton_Click(
            object sender,
            EventArgs e)
        {
            HistoryGrid.PageIndex =
                0;

            LoadHistories();
        }

        protected void ResetButton_Click(
            object sender,
            EventArgs e)
        {
            KeywordTextBox.Text =
                string.Empty;

            StatusDropDownList.SelectedValue =
                string.Empty;

            HistoryGrid.PageIndex =
                0;

            LoadHistories();
        }

        protected void HistoryGrid_PageIndexChanging(
            object sender,
            GridViewPageEventArgs e)
        {
            HistoryGrid.PageIndex =
                e.NewPageIndex;

            LoadHistories();
        }

        private void LoadHistories()
        {
            HideMessage();

            try
            {
                var keyword =
                    (KeywordTextBox.Text ??
                     string.Empty).Trim();

                var isSuccess =
                    GetSelectedSuccessStatus();

                var histories =
                    _historyQueryService
                        .SearchHistories(
                            keyword,
                            isSuccess);

                HistoryGrid.DataSource =
                    histories;

                HistoryGrid.DataBind();

                ResultCountLiteral.Text =
                    histories.Count +
                    " 件";
            }
            catch (Exception ex)
            {
                Trace.Warn(
                    "AiSearchHistory",
                    "AI検索履歴一覧の取得に失敗しました。",
                    ex);

                HistoryGrid.DataSource =
                    null;

                HistoryGrid.DataBind();

                ResultCountLiteral.Text =
                    "0 件";

                ShowMessage(
                    "AI検索履歴を取得できませんでした。");
            }
        }

        private bool? GetSelectedSuccessStatus()
        {
            var selectedValue =
                StatusDropDownList.SelectedValue;

            if (string.Equals(
                    selectedValue,
                    "true",
                    StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (string.Equals(
                    selectedValue,
                    "false",
                    StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return null;
        }

        /// <summary>
        /// DBにUTCで保存された実行日時を
        /// 日本時間へ変換して表示します。
        /// </summary>
        public string FormatExecutedAt(
            object value)
        {
            if (value == null ||
                value == DBNull.Value)
            {
                return "-";
            }

            DateTime executedAt;

            if (!DateTime.TryParse(
                    Convert.ToString(value),
                    out executedAt))
            {
                return "-";
            }

            /*
             * MySQLから取得したDateTimeのKindが
             * Unspecifiedになる場合もUTCとして扱う。
             */
            var utcDateTime =
                executedAt.Kind == DateTimeKind.Utc
                    ? executedAt
                    : DateTime.SpecifyKind(
                        executedAt,
                        DateTimeKind.Utc);

            var japanDateTime =
                TimeZoneInfo.ConvertTimeFromUtc(
                    utcDateTime,
                    JapanTimeZone);

            return japanDateTime.ToString(
                "yyyy/MM/dd HH:mm:ss");
        }

        public string GetStatusText(
            object value)
        {
            return ConvertToBoolean(value)
                ? "成功"
                : "失敗";
        }

        public string GetStatusCss(
            object value)
        {
            return ConvertToBoolean(value)
                ? "badge text-bg-success"
                : "badge text-bg-danger";
        }

        public string GetFeedbackText(
    object value)
        {
            var isHelpful =
                ConvertToNullableBoolean(
                    value);

            if (!isHelpful.HasValue)
            {
                return "-";
            }

            return isHelpful.Value
                ? "👍"
                : "👎";
        }

        public string GetFeedbackTitle(
            object value)
        {
            var isHelpful =
                ConvertToNullableBoolean(
                    value);

            if (!isHelpful.HasValue)
            {
                return "未評価";
            }

            return isHelpful.Value
                ? "役に立った"
                : "役に立たなかった";
        }

        public string GetFeedbackCss(
            object value)
        {
            var isHelpful =
                ConvertToNullableBoolean(
                    value);

            if (!isHelpful.HasValue)
            {
                return "admin-feedback-badge " +
                       "admin-feedback-badge--none";
            }

            return isHelpful.Value
                ? "admin-feedback-badge " +
                  "admin-feedback-badge--helpful"
                : "admin-feedback-badge " +
                  "admin-feedback-badge--not-helpful";
        }

        private static bool? ConvertToNullableBoolean(
            object value)
        {
            if (value == null ||
                value == DBNull.Value)
            {
                return null;
            }

            if (value is bool)
            {
                return (bool)value;
            }

            bool result;

            if (bool.TryParse(
                    Convert.ToString(value),
                    out result))
            {
                return result;
            }

            /*
             * MySQLのTINYINT(1)などが
             * 0または1として渡された場合にも対応する。
             */
            int numericValue;

            if (int.TryParse(
                    Convert.ToString(value),
                    out numericValue))
            {
                if (numericValue == 1)
                {
                    return true;
                }

                if (numericValue == 0)
                {
                    return false;
                }
            }

            return null;
        }

        public string GetResultPreview(
            object answerPreview,
            object errorMessage)
        {
            var answer =
                NormalizeDisplayText(
                    answerPreview);

            if (!string.IsNullOrWhiteSpace(
                    answer))
            {
                return answer;
            }

            var error =
                NormalizeDisplayText(
                    errorMessage);

            if (!string.IsNullOrWhiteSpace(
                    error))
            {
                return error;
            }

            return "-";
        }

        private static bool ConvertToBoolean(
            object value)
        {
            if (value == null ||
                value == DBNull.Value)
            {
                return false;
            }

            bool result;

            return bool.TryParse(
                       Convert.ToString(value),
                       out result) &&
                   result;
        }

        private static string NormalizeDisplayText(
            object value)
        {
            if (value == null ||
                value == DBNull.Value)
            {
                return null;
            }

            var text =
                Convert.ToString(value);

            if (string.IsNullOrWhiteSpace(
                    text))
            {
                return null;
            }

            return text.Trim();
        }

        private void ShowMessage(
            string message)
        {
            MessageLiteral.Text =
                HttpUtility.HtmlEncode(
                    message ??
                    string.Empty);

            MessagePanel.Visible =
                true;
        }

        private void HideMessage()
        {
            MessageLiteral.Text =
                string.Empty;

            MessagePanel.Visible =
                false;
        }
    }
}
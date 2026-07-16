using System;
using System.Diagnostics;
using System.Web.UI.WebControls;
using FaqKnowledgeSearch.WebForms.Dtos;
using FaqKnowledgeSearch.WebForms.Services;
using System.Collections.Generic;
using System.Linq;


namespace FaqKnowledgeSearch.WebForms.Admin.Faqs
{
    public partial class Edit : AdminPageBase
    {
        private readonly IFaqService _faqService;

        public Edit()
        {
            _faqService = new FaqService();
        }

        protected void Page_Load(
            object sender,
            EventArgs e)
        {
            ErrorPanel.Visible = false;

            long? faqId;

            if (!TryGetFaqId(out faqId))
            {
                AdminHeaderControl.ActiveItem = "faq";

                ConfigureErrorMode(
                    "指定されたFAQ IDが正しくありません。");

                return;
            }

            /*
             * 新規登録画面では「新規登録」、
             * 編集画面では「FAQ管理」を選択状態にします。
             */
            AdminHeaderControl.ActiveItem =
                faqId.HasValue
                    ? "faq"
                    : "new";

            if (IsPostBack)
            {
                return;
            }

            if (faqId.HasValue)
            {
                ConfigureEditMode(faqId.Value);
            }
            else
            {
                ConfigureCreateMode();
            }
        }

        protected void SaveButton_Click(
            object sender,
            EventArgs e)
        {
            ErrorPanel.Visible = false;

            Page.Validate("FaqEdit");

            if (!Page.IsValid)
            {
                return;
            }

            long? faqId;

            if (!TryGetFaqId(out faqId))
            {
                ShowError(
                    "指定されたFAQ IDが正しくありません。");

                return;
            }

            long categoryId;

            if (!long.TryParse(
                CategoryDropDownList.SelectedValue,
                out categoryId))
            {
                ShowError(
                    "カテゴリを選択してください。");

                return;
            }

            var question =
                QuestionTextBox.Text.Trim();

            var answer =
                AnswerTextBox.Text.Trim();

            if (question.Length > 500)
            {
                ShowError(
                    "タイトルは500文字以内で入力してください。");

                return;
            }

            if (answer.Length > 4000)
            {
                ShowError(
                    "本文は4000文字以内で入力してください。");

                return;
            }

            var input =
                new FaqEditDto
                {
                    CategoryId = categoryId,
                    Question = question,
                    Answer = answer,
                    IsPublished =
                        IsPublishedCheckBox.Checked,

                    SelectedTagIds =
                        GetSelectedTagIds()
                };

            try
            {
                if (faqId.HasValue)
                {
                    UpdateFaq(
                        faqId.Value,
                        input);

                    return;
                }

                CreateFaq(input);
            }
            catch (ArgumentException ex)
            {
                ShowError(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                ShowError(ex.Message);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(
                    ex.ToString());

                ShowError(
                    "FAQの保存中にエラーが発生しました。");
            }
        }

        private void CreateFaq(
            FaqEditDto input)
        {
            _faqService.CreateFaq(input);

            RedirectToList("created");
        }

        private void UpdateFaq(
            long faqId,
            FaqEditDto input)
        {
            var updated =
                _faqService.UpdateFaq(
                    faqId,
                    input);

            if (!updated)
            {
                ShowError(
                    "編集対象のFAQが見つかりません。");

                return;
            }

            RedirectToList("updated");
        }

        private void ConfigureCreateMode()
        {
            Page.Title = "FAQ新規登録";

            PageBadgeLiteral.Text =
                "New FAQ";

            PageHeadingLiteral.Text =
                "FAQ新規登録";

            PageDescriptionLiteral.Text =
                "社内FAQ・手順書・障害対応メモを登録します。";

            SaveButton.Text =
                "登録する";

            FaqIdPanel.Visible = false;

            BindCategories(null);

            BindTags(null);

            IsPublishedCheckBox.Checked = false;
        }

        private void ConfigureEditMode(
            long faqId)
        {
            Page.Title = "FAQ編集";

            PageBadgeLiteral.Text =
                "Edit FAQ";

            PageHeadingLiteral.Text =
                "FAQ編集";

            PageDescriptionLiteral.Text =
                "登録済みFAQの内容・カテゴリ・タグ・公開状態を更新します。";

            SaveButton.Text =
                "更新する";

            var faq =
                _faqService.GetFaqForAdmin(faqId);

            if (faq == null)
            {
                ConfigureErrorMode(
                    "編集対象のFAQが見つかりません。");

                return;
            }

            BindCategories(faq.CategoryId);

            BindTags(faq.SelectedTagIds);

            CategoryDropDownList.SelectedValue =
                faq.CategoryId.ToString();

            QuestionTextBox.Text =
                faq.Question ?? string.Empty;

            AnswerTextBox.Text =
                faq.Answer ?? string.Empty;

            IsPublishedCheckBox.Checked =
                faq.IsPublished;

            FaqIdLiteral.Text =
                faqId.ToString();

            FaqIdPanel.Visible = true;
        }

        private void BindCategories(
            long? includeCategoryId)
        {
            var categories =
                _faqService.GetCategoryOptions(
                    includeCategoryId);

            CategoryDropDownList.DataSource =
                categories;

            CategoryDropDownList.DataTextField =
                "DisplayName";

            CategoryDropDownList.DataValueField =
                "Id";

            CategoryDropDownList.DataBind();

            CategoryDropDownList.Items.Insert(
                0,
                new ListItem(
                    "選択してください",
                    string.Empty));
        }

        private void BindTags(
    IEnumerable<int> selectedTagIds)
        {
            var tags =
                _faqService.GetTagOptions();

            TagCheckBoxList.DataSource =
                tags;

            TagCheckBoxList.DataTextField =
                "Name";

            TagCheckBoxList.DataValueField =
                "Id";

            TagCheckBoxList.DataBind();

            var selectedIds =
                new HashSet<int>(
                    selectedTagIds ??
                    Enumerable.Empty<int>());

            foreach (ListItem item
                in TagCheckBoxList.Items)
            {
                int tagId;

                if (!int.TryParse(
                    item.Value,
                    out tagId))
                {
                    continue;
                }

                item.Selected =
                    selectedIds.Contains(tagId);
            }

            var hasTags =
                TagCheckBoxList.Items.Count > 0;

            TagCheckBoxList.Visible =
                hasTags;

            TagEmptyPanel.Visible =
                !hasTags;
        }

        private IList<int> GetSelectedTagIds()
        {
            var selectedTagIds =
                new List<int>();

            foreach (ListItem item
                in TagCheckBoxList.Items)
            {
                if (!item.Selected)
                {
                    continue;
                }

                int tagId;

                if (!int.TryParse(
                    item.Value,
                    out tagId))
                {
                    continue;
                }

                if (tagId <= 0)
                {
                    continue;
                }

                selectedTagIds.Add(tagId);
            }

            return selectedTagIds
                .Distinct()
                .ToList();
        }

        private bool TryGetFaqId(
            out long? faqId)
        {
            faqId = null;

            var rawId =
                Request.QueryString["id"];

            if (string.IsNullOrWhiteSpace(rawId))
            {
                return true;
            }

            long parsedId;

            if (!long.TryParse(
                    rawId,
                    out parsedId) ||
                parsedId <= 0)
            {
                return false;
            }

            faqId = parsedId;

            return true;
        }

        private void ConfigureErrorMode(
            string message)
        {
            Page.Title =
                "FAQ編集エラー";

            PageBadgeLiteral.Text =
                "FAQ Error";

            PageHeadingLiteral.Text =
                "FAQ登録・編集";

            PageDescriptionLiteral.Text =
                "FAQの情報を読み込めませんでした。";

            ShowError(message);

            CategoryDropDownList.Enabled = false;
            QuestionTextBox.Enabled = false;
            AnswerTextBox.Enabled = false;
            IsPublishedCheckBox.Enabled = false;
            TagCheckBoxList.Enabled = false;

            SaveButton.Visible = false;
            FaqIdPanel.Visible = false;
        }

        private void ShowError(
            string message)
        {
            ErrorLiteral.Text =
                Server.HtmlEncode(message);

            ErrorPanel.Visible = true;
        }

        private void RedirectToList(
            string message)
        {
            Response.Redirect(
                "~/Admin/Faqs/Index.aspx?message=" +
                Server.UrlEncode(message),
                false);

            Context.ApplicationInstance
                .CompleteRequest();
        }
    }
}
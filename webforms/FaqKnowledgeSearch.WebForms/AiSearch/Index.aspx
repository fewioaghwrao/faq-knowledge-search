<%@ Page
    Title="AI FAQ検索"
    Language="C#"
    MasterPageFile="~/Site.Master"
    AutoEventWireup="true"
    CodeBehind="Index.aspx.cs"
    Inherits="FaqKnowledgeSearch.WebForms.AiSearch.Index"
    Async="true" %>

<asp:Content
    ID="MainContent"
    ContentPlaceHolderID="MainContent"
    runat="server">

    <main class="ai-search-page">

        <%-- ヒーローエリア --%>
        <section class="ai-search-hero">
            <span class="ai-search-hero__label">
                AI FAQ Search
            </span>

            <h1 class="ai-search-hero__title">
                FAQをもとにAI回答を生成
            </h1>

            <p class="ai-search-hero__description">
                質問・検索キーワードから関連FAQを検索し、
                上位のFAQをもとにAI回答と参照元FAQを表示します。
            </p>

            <div class="ai-search-hero__features">
                <span>FAQ検索連携</span>
                <span>参照元表示</span>
                <span>外部AI API連携</span>
            </div>
        </section>

        <%-- 入力エリア --%>
        <section class="ai-search-form-card">

            <asp:ValidationSummary
                ID="ValidationSummary1"
                runat="server"
                ValidationGroup="AiSearch"
                CssClass="ai-search-message ai-search-message--error"
                DisplayMode="BulletList" />

            <div class="ai-search-field">
                <asp:Label
                    ID="QuestionLabel"
                    runat="server"
                    AssociatedControlID="QuestionTextBox"
                    Text="質問・検索キーワード"
                    CssClass="ai-search-field__label" />

                <asp:TextBox
                    ID="QuestionTextBox"
                    runat="server"
                    ClientIDMode="Static"
                    TextMode="MultiLine"
                    Rows="5"
                    MaxLength="500"
                    CssClass="ai-search-textarea"
                    placeholder="例：ログインできない場合はどうすればよいですか？" />

                <asp:RequiredFieldValidator
                    ID="QuestionRequiredValidator"
                    runat="server"
                    ControlToValidate="QuestionTextBox"
                    ValidationGroup="AiSearch"
                    ErrorMessage="質問を入力してください。"
                    Display="None" />
            </div>

            <div class="ai-search-examples">
                <span class="ai-search-examples__label">
                    質問例
                </span>

                <div class="ai-search-examples__buttons">
<button
    type="button"
    class="ai-search-chip"
    onclick="setAiQuestion('ログインできない場合の初期対応手順')">
    ログインできない初期対応
</button>

<button
    type="button"
    class="ai-search-chip"
    onclick="setAiQuestion('CSV取込でエラー行が発生した場合の確認手順')">
    CSV取込エラー
</button>

<button
    type="button"
    class="ai-search-chip"
    onclick="setAiQuestion('PDF出力に失敗した場合の確認手順')">
    PDF出力失敗
</button>
            </div>

            <div class="ai-search-form-note">
                <span>
                    FAQに登録された内容をもとに回答します。
                </span>

                <span
                    id="QuestionCharacterCount"
                    class="ai-search-character-count">
                    0/500
                </span>
            </div>

            <div class="ai-search-form-actions">
                <a
                    runat="server"
                    href="~/Faqs/Index.aspx"
                    class="ai-search-standard-link">
                    通常のFAQ検索を見る →
                </a>

                <asp:Button
                    ID="SearchButton"
                    runat="server"
                    Text="AI検索する"
                    CssClass="ai-search-submit"
                    ValidationGroup="AiSearch"
                    OnClick="SearchButton_Click" />
            </div>

        </section>

        <%-- メッセージ --%>
        <asp:Panel
            ID="MessagePanel"
            runat="server"
            Visible="false"
            CssClass="ai-search-message ai-search-message--warning">

            <asp:Literal
                ID="MessageLiteral"
                runat="server" />
        </asp:Panel>

        <%-- 検索結果 --%>
        <asp:Panel
            ID="ResultPanel"
            runat="server"
            Visible="false"
            CssClass="ai-search-result">

            <section class="ai-result-card">
                <div class="ai-result-card__header">
                    <div>
                        <span class="ai-result-card__eyebrow">
                            AI Answer
                        </span>

                        <h2 class="ai-result-card__title">
                            AI回答
                        </h2>
                    </div>

                    <span class="ai-result-card__badge">
                        FAQベース
                    </span>
                </div>

                <div class="ai-answer-box">
                    <asp:Literal
                        ID="AnswerLiteral"
                        runat="server" />
                </div>

                <p class="ai-answer-disclaimer">
                    <asp:Literal
                        ID="DisclaimerLiteral"
                        runat="server" />
                </p>

                    <%-- ここからフィードバック機能 --%>
    <asp:HiddenField
        ID="HistoryIdHiddenField"
        runat="server" />

    <asp:Panel
        ID="FeedbackPanel"
        runat="server"
        Visible="false"
        CssClass="ai-feedback-card">

        <div class="ai-feedback-card__heading">
            <h3 class="ai-feedback-card__title">
                この回答は役に立ちましたか？
            </h3>

            <p class="ai-feedback-card__description">
                評価はAI検索結果の改善に利用します。
            </p>
        </div>

        <div class="ai-feedback-card__actions">

<asp:LinkButton
    ID="HelpfulButton"
    runat="server"
    CssClass="ai-feedback-button ai-feedback-button--helpful"
    CausesValidation="false"
    OnClick="HelpfulButton_Click">

    <span aria-hidden="true">👍</span>
    <span>役に立った</span>

</asp:LinkButton>

<asp:LinkButton
    ID="NotHelpfulButton"
    runat="server"
    CssClass="ai-feedback-button ai-feedback-button--not-helpful"
    CausesValidation="false"
    OnClick="NotHelpfulButton_Click">

    <span aria-hidden="true">👎</span>
    <span>役に立たなかった</span>

</asp:LinkButton>

        </div>

        <asp:Panel
            ID="FeedbackCompletePanel"
            runat="server"
            Visible="false"
            CssClass="ai-feedback-complete">

            <asp:Literal
                ID="FeedbackCompleteLiteral"
                runat="server"
                Mode="Encode" />

        </asp:Panel>

    </asp:Panel>
    <%-- ここまでフィードバック機能 --%>

            </section>

            <section class="ai-result-card">
                <div class="ai-result-card__header">
                    <div>
                        <span class="ai-result-card__eyebrow">
                            Sources
                        </span>

                        <h2 class="ai-result-card__title">
                            参照元FAQ
                        </h2>
                    </div>
                </div>

                <asp:Repeater
                    ID="SourceRepeater"
                    runat="server">

                    <HeaderTemplate>
                        <div class="ai-source-list">
                    </HeaderTemplate>

                    <ItemTemplate>
                        <a
                            class="ai-source-item"
                            href="<%# ResolveUrl(Eval("Url").ToString()) %>">

                            <span class="ai-source-item__icon">
                                Q
                            </span>

                            <span class="ai-source-item__title">
                                <%#: Eval("Title") %>
                            </span>

                            <span
                                class="ai-source-item__arrow"
                                aria-hidden="true">
                                →
                            </span>
                        </a>
                    </ItemTemplate>

                    <FooterTemplate>
                        </div>
                    </FooterTemplate>

                </asp:Repeater>
            </section>

        </asp:Panel>

    </main>

    <script>
        (function () {
            var questionTextBox =
                document.getElementById("QuestionTextBox");

            var characterCount =
                document.getElementById("QuestionCharacterCount");

            function updateCharacterCount() {
                if (!questionTextBox || !characterCount) {
                    return;
                }

                characterCount.textContent =
                    questionTextBox.value.length + "/500";
            }

            window.setAiQuestion = function (question) {
                if (!questionTextBox) {
                    return;
                }

                questionTextBox.value = question;
                questionTextBox.focus();

                updateCharacterCount();
            };

            if (questionTextBox) {
                questionTextBox.addEventListener(
                    "input",
                    updateCharacterCount);

                updateCharacterCount();
            }
        })();
    </script>

</asp:Content>
<%@ Page
    Title="FAQ登録・編集"
    Language="C#"
    MasterPageFile="~/Site.Master"
    AutoEventWireup="true"
    CodeBehind="Edit.aspx.cs"
    Inherits="FaqKnowledgeSearch.WebForms.Admin.Faqs.Edit" %>

<%@ Register
    Src="~/Admin/AdminHeader.ascx"
    TagPrefix="admin"
    TagName="AdminHeader" %>

<asp:Content
    ID="MainContent"
    ContentPlaceHolderID="MainContent"
    runat="server">

    <main class="admin-page">

        <admin:AdminHeader
            ID="AdminHeaderControl"
            runat="server" />

        <section class="admin-panel admin-edit-panel">

            <div class="admin-panel__header">

                <div>
                    <span class="admin-panel__badge">
                        <asp:Literal
                            ID="PageBadgeLiteral"
                            runat="server" />
                    </span>

                    <h2 class="admin-panel__title">
                        <asp:Literal
                            ID="PageHeadingLiteral"
                            runat="server" />
                    </h2>

                    <p class="admin-panel__description">
                        <asp:Literal
                            ID="PageDescriptionLiteral"
                            runat="server" />
                    </p>
                </div>

                <asp:HyperLink
                    ID="HeaderBackLink"
                    runat="server"
                    NavigateUrl="~/Admin/Faqs/Index.aspx"
                    CssClass="admin-secondary-button">
                    一覧へ戻る
                </asp:HyperLink>

            </div>

            <asp:Panel
                ID="ErrorPanel"
                runat="server"
                Visible="false"
                CssClass="admin-message admin-message--error"
                role="alert">

                <asp:Literal
                    ID="ErrorLiteral"
                    runat="server" />

            </asp:Panel>

            <asp:ValidationSummary
                ID="FaqValidationSummary"
                runat="server"
                ValidationGroup="FaqEdit"
                CssClass="admin-message admin-message--error"
                HeaderText="入力内容を確認してください。"
                DisplayMode="BulletList" />

            <div class="admin-form">

                <%-- タイトル --%>
                <div class="admin-form-field">

                    <div class="admin-form-field__heading">

                        <asp:Label
                            ID="QuestionLabel"
                            runat="server"
                            AssociatedControlID="QuestionTextBox"
                            CssClass="form-label"
                            Text="タイトル" />

                        <span class="admin-form-required">
                            必須
                        </span>

                        <span class="admin-form-counter">
                            <span id="questionCharacterCount">0</span>/500
                        </span>

                    </div>

                    <asp:TextBox
                        ID="QuestionTextBox"
                        runat="server"
                        ClientIDMode="Static"
                        CssClass="form-control"
                        MaxLength="500"
                        autocomplete="off"
                        placeholder="例：ログインできない場合の初期対応手順" />

                    <asp:RequiredFieldValidator
                        ID="QuestionRequiredValidator"
                        runat="server"
                        ControlToValidate="QuestionTextBox"
                        ValidationGroup="FaqEdit"
                        CssClass="admin-form-error"
                        ErrorMessage="タイトルを入力してください。"
                        Display="Dynamic" />

                </div>

                <%-- 本文 --%>
                <div class="admin-form-field">

                    <div class="admin-form-field__heading">

                        <asp:Label
                            ID="AnswerLabel"
                            runat="server"
                            AssociatedControlID="AnswerTextBox"
                            CssClass="form-label"
                            Text="本文" />

                        <span class="admin-form-required">
                            必須
                        </span>

                        <span class="admin-form-counter">
                            <span id="answerCharacterCount">0</span>文字
                        </span>

                    </div>

                    <asp:TextBox
                        ID="AnswerTextBox"
                        runat="server"
                        ClientIDMode="Static"
                        TextMode="MultiLine"
                        Rows="10"
                        MaxLength="4000"
                        CssClass="form-control admin-answer-input"
                        placeholder="対応手順、確認ポイント、参照元などを記載してください。" />

                    <p class="admin-form-help">
                        改行を含めて入力できます。
                    </p>

                    <asp:RequiredFieldValidator
                        ID="AnswerRequiredValidator"
                        runat="server"
                        ControlToValidate="AnswerTextBox"
                        ValidationGroup="FaqEdit"
                        CssClass="admin-form-error"
                        ErrorMessage="本文を入力してください。"
                        Display="Dynamic" />

                </div>

                <%-- カテゴリ --%>
                <div class="admin-form-row">

                    <div class="admin-form-field">

                        <div class="admin-form-field__heading">

                            <asp:Label
                                ID="CategoryLabel"
                                runat="server"
                                AssociatedControlID="CategoryDropDownList"
                                CssClass="form-label"
                                Text="カテゴリ" />

                            <span class="admin-form-required">
                                必須
                            </span>

                        </div>

                        <asp:DropDownList
                            ID="CategoryDropDownList"
                            runat="server"
                            CssClass="form-select" />

                        <asp:RequiredFieldValidator
                            ID="CategoryRequiredValidator"
                            runat="server"
                            ControlToValidate="CategoryDropDownList"
                            InitialValue=""
                            ValidationGroup="FaqEdit"
                            CssClass="admin-form-error"
                            ErrorMessage="カテゴリを選択してください。"
                            Display="Dynamic" />

                    </div>

<div class="admin-form-field">

    <div class="admin-form-field__heading">

        <asp:Label
            ID="TagLabel"
            runat="server"
            AssociatedControlID="TagCheckBoxList"
            CssClass="form-label"
            Text="タグ" />

        <span class="admin-form-optional">
            任意
        </span>

    </div>

    <div class="admin-tag-selector">

        <asp:CheckBoxList
            ID="TagCheckBoxList"
            runat="server"
            RepeatDirection="Horizontal"
            RepeatLayout="Flow"
            CssClass="admin-tag-check-list" />

        <asp:Panel
            ID="TagEmptyPanel"
            runat="server"
            Visible="false"
            CssClass="admin-tag-empty">

            利用できるタグが登録されていません。

        </asp:Panel>

    </div>

    <p class="admin-form-help">
        FAQに関連するタグを複数選択できます。
    </p>

</div>

                </div>

                <%-- 公開状態 --%>
                <div class="admin-publish-panel">

                    <div class="admin-publish-panel__content">

                        <strong class="admin-publish-panel__title">
                            公開状態
                        </strong>

                        <p class="admin-publish-panel__description">
                            ONにすると利用者向けFAQ一覧・検索結果に表示されます。
                        </p>

                    </div>

                    <div class="admin-publish-panel__control">

                        <asp:CheckBox
                            ID="IsPublishedCheckBox"
                            runat="server"
                            CssClass="admin-publish-checkbox" />

                        <asp:Label
                            ID="IsPublishedLabel"
                            runat="server"
                            AssociatedControlID="IsPublishedCheckBox"
                            CssClass="admin-publish-label"
                            Text="公開する" />

                    </div>

                </div>

                <%-- 下部操作 --%>
                <div class="admin-form-footer">

                    <asp:Panel
                        ID="FaqIdPanel"
                        runat="server"
                        Visible="false"
                        CssClass="admin-form-footer__id">

                        FAQ ID:
                        <strong>
                            #<asp:Literal
                                ID="FaqIdLiteral"
                                runat="server" />
                        </strong>

                    </asp:Panel>

                    <div class="admin-form-footer__actions">

                        <asp:HyperLink
                            ID="BackLink"
                            runat="server"
                            NavigateUrl="~/Admin/Faqs/Index.aspx"
                            CssClass="admin-secondary-button">
                            キャンセル
                        </asp:HyperLink>

                        <asp:Button
                            ID="SaveButton"
                            runat="server"
                            Text="保存"
                            CssClass="admin-primary-button"
                            ValidationGroup="FaqEdit"
                            OnClick="SaveButton_Click" />

                    </div>

                </div>

            </div>

        </section>

    </main>

    <script>
        (function () {
            "use strict";

            function updateCharacterCount(
                input,
                counter) {

                if (!input || !counter) {
                    return;
                }

                counter.textContent =
                    input.value.length.toString();
            }

            function initializeCharacterCounter(
                inputId,
                counterId) {

                const input =
                    document.getElementById(inputId);

                const counter =
                    document.getElementById(counterId);

                if (!input || !counter) {
                    return;
                }

                updateCharacterCount(
                    input,
                    counter);

                input.addEventListener(
                    "input",
                    function () {
                        updateCharacterCount(
                            input,
                            counter);
                    });
            }

            function initializeFaqEditPage() {
                initializeCharacterCounter(
                    "QuestionTextBox",
                    "questionCharacterCount");

                initializeCharacterCounter(
                    "AnswerTextBox",
                    "answerCharacterCount");
            }

            if (document.readyState === "loading") {
                document.addEventListener(
                    "DOMContentLoaded",
                    initializeFaqEditPage);
            }
            else {
                initializeFaqEditPage();
            }
        }());
    </script>

</asp:Content>

<%@ Page
    Title="ページが見つかりません"
    Language="C#"
    MasterPageFile="~/Error/Error.Master"
    AutoEventWireup="true"
    CodeBehind="404.aspx.cs"
    Inherits="FaqKnowledgeSearch.WebForms.ErrorPages.NotFound" %>

<asp:Content
    ID="MainContent"
    ContentPlaceHolderID="MainContent"
    runat="server">

    <main class="error-page">
        <section class="error-card">
            <span class="error-card__code">
                404
            </span>

            <h1 class="error-card__title">
                ページが見つかりません
            </h1>

            <p class="error-card__description">
                URLが変更されたか、ページが削除された可能性があります。
            </p>

            <div class="error-card__actions">
                <a
                    runat="server"
                    href="~/Default.aspx"
                    class="error-card__primary-button">
                    トップページへ戻る
                </a>

                <button
                    type="button"
                    class="error-card__secondary-button"
                    onclick="history.back();">
                    前のページへ戻る
                </button>
            </div>
        </section>
    </main>

</asp:Content>
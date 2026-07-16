<%@ Page
    Title="エラーが発生しました"
    Language="C#"
    MasterPageFile="~/Error/Error.Master"
    AutoEventWireup="true"
    CodeBehind="500.aspx.cs"
    Inherits="FaqKnowledgeSearch.WebForms.ErrorPages.InternalServerError" %>

<asp:Content
    ID="MainContent"
    ContentPlaceHolderID="MainContent"
    runat="server">

    <main class="error-page">
        <section class="error-card">
            <span class="error-card__code error-card__code--error">
                500
            </span>

            <h1 class="error-card__title">
                エラーが発生しました
            </h1>

            <p class="error-card__description">
                処理を完了できませんでした。
                時間をおいてから、もう一度お試しください。
            </p>

            <p class="error-card__support-text">
                問題が続く場合は、管理者へお問い合わせください。
            </p>

            <div class="error-card__actions">
                <a
                    runat="server"
                    href="~/Default.aspx"
                    class="error-card__primary-button">
                    トップページへ戻る
                </a>
            </div>
        </section>
    </main>

</asp:Content>
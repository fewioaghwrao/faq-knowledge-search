<%@ Page
    Title="ユーザー管理"
    Language="C#"
    MasterPageFile="~/Site.Master"
    AutoEventWireup="true"
    Async="true"
    CodeBehind="Index.aspx.cs"
    Inherits="FaqKnowledgeSearch.WebForms.Admin.Users.Index" %>

<%@ Register
    Src="~/Admin/AdminHeader.ascx"
    TagPrefix="uc"
    TagName="AdminHeader" %>

<asp:Content
    ID="BodyContent"
    ContentPlaceHolderID="MainContent"
    runat="server">

    <uc:AdminHeader
        ID="AdminHeader1"
        runat="server"
        ActiveItem="users" />

    <main class="admin-content admin-users-page">

        <asp:Panel
            ID="SuccessMessagePanel"
            runat="server"
            CssClass="admin-message admin-message--success"
            Visible="false">

            <asp:Label
                ID="SuccessMessageLabel"
                runat="server" />
        </asp:Panel>

        <asp:Panel
            ID="ErrorMessagePanel"
            runat="server"
            CssClass="admin-message admin-message--error"
            Visible="false">

            <asp:Label
                ID="ErrorMessageLabel"
                runat="server" />
        </asp:Panel>

        <section class="admin-section">

            <header class="admin-section__header">
                <div>
                    <span class="admin-section__eyebrow">
                        USER MANAGEMENT
                    </span>

                    <h2 class="admin-section__title">
                        登録ユーザー一覧
                    </h2>

                    <p class="admin-section__description">
                        システムへ登録されているユーザーと利用状態を管理します。
                    </p>
                </div>

                <div class="admin-user-count">
                    全
                    <asp:Label
                        ID="TotalCountLabel"
                        runat="server"
                        Text="0" />
                    件
                </div>
            </header>

            <asp:Panel
                ID="EmptyPanel"
                runat="server"
                CssClass="admin-empty-state"
                Visible="false">

                登録されているユーザーはありません。
            </asp:Panel>

            <asp:Repeater
                ID="UsersRepeater"
                runat="server">

                <HeaderTemplate>
                    <div class="admin-table-wrapper">
                        <table class="admin-table admin-user-table">
                            <thead>
                                <tr>
                                    <th scope="col">表示名</th>
                                    <th scope="col">メールアドレス</th>
                                    <th scope="col">ロール</th>
                                    <th scope="col">状態</th>
                                    <th scope="col">作成日時</th>
                                    <th scope="col">操作</th>
                                </tr>
                            </thead>

                            <tbody>
                </HeaderTemplate>

                <ItemTemplate>
                    <tr>
                        <td data-label="表示名">
                            <strong class="admin-user-name">
                                <%#: Eval("DisplayName") %>
                            </strong>
                        </td>

                        <td data-label="メールアドレス">
                            <span class="admin-user-email">
                                <%#: Eval("Email") %>
                            </span>
                        </td>

                        <td data-label="ロール">
                            <span class='<%# GetRoleCssClass(
                                Convert.ToString(Eval("RoleName"))) %>'>
                                <%#: Eval("RoleName") %>
                            </span>
                        </td>

                        <td data-label="状態">
                            <span class='<%# GetStatusCssClass(
                                Convert.ToBoolean(Eval("IsActive"))) %>'>
                                <%#: GetStatusText(
                                    Convert.ToBoolean(Eval("IsActive"))) %>
                            </span>
                        </td>

                        <td data-label="作成日時">
                            <%#: FormatCreatedAt(
                                Convert.ToDateTime(Eval("CreatedAt"))) %>
                        </td>

                        <td data-label="操作">
                            <asp:LinkButton
                                ID="ToggleStatusButton"
                                runat="server"
                                CssClass='<%# GetActionCssClass(
                                    Convert.ToBoolean(Eval("IsActive"))) %>'
                                CommandName='<%# GetCommandName(
                                    Convert.ToBoolean(Eval("IsActive"))) %>'
                                CommandArgument='<%# Eval("Id") %>'
                                OnCommand="ToggleStatusButton_Command"
                                OnClientClick='<%# GetConfirmationMessage(
                                    Convert.ToBoolean(Eval("IsActive"))) %>'
                                CausesValidation="false"
                                Visible='<%# Convert.ToBoolean(
                                    Eval("CanChangeStatus")) %>'>

                                <%#: GetActionText(
                                    Convert.ToBoolean(Eval("IsActive"))) %>
                            </asp:LinkButton>

                            <span
                                runat="server"
                                class="admin-action-disabled"
                                visible='<%# !Convert.ToBoolean(
                                    Eval("CanChangeStatus")) %>'
                                title="Adminユーザーの状態は変更できません">
                                変更不可
                            </span>
                        </td>
                    </tr>
                </ItemTemplate>

                <FooterTemplate>
                            </tbody>
                        </table>
                    </div>
                </FooterTemplate>

            </asp:Repeater>

            <asp:Panel
                ID="PagerPanel"
                runat="server"
                CssClass="admin-pager"
                Visible="false">

                <asp:HyperLink
                    ID="PreviousPageLink"
                    runat="server"
                    CssClass="admin-pager__button"
                    Text="前へ" />

                <asp:Label
                    ID="PageInfoLabel"
                    runat="server"
                    CssClass="admin-pager__info" />

                <asp:HyperLink
                    ID="NextPageLink"
                    runat="server"
                    CssClass="admin-pager__button"
                    Text="次へ" />

            </asp:Panel>

            <p class="admin-users-note">
                Adminユーザーの有効・無効は変更できません。
                ロール変更機能は現在未実装です。
            </p>

        </section>
    </main>

</asp:Content>

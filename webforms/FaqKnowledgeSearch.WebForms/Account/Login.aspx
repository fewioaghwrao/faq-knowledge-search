<%@ Page Title="管理者ログイン" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="FaqKnowledgeSearch.WebForms.Account.Login" Async="true" %>

<asp:Content
    ID="BodyContent"
    ContentPlaceHolderID="MainContent"
    runat="server">

    <main class="login-page">
        <section class="login-card" aria-labelledby="loginTitle">

            <div class="login-card__header">
                <div class="login-card__icon" aria-hidden="true">
                    A
                </div>

                <div>
                    <h1 id="loginTitle" class="login-card__title">
                        管理者ログイン
                    </h1>

                    <p class="login-card__description">
                        FAQの登録・編集を行う管理画面にログインします。
                    </p>
                </div>
            </div>

            <asp:ValidationSummary
                ID="LoginValidationSummary"
                runat="server"
                ValidationGroup="Login"
                CssClass="login-alert"
                HeaderText="入力内容を確認してください。"
                DisplayMode="BulletList" />

            <asp:Panel
                ID="LoginErrorPanel"
                runat="server"
                CssClass="login-alert"
                Visible="false"
                role="alert">

                <asp:Literal
                    ID="LoginErrorMessage"
                    runat="server" />

            </asp:Panel>

            <div class="login-field">
                <asp:Label
                    ID="EmailLabel"
                    runat="server"
                    AssociatedControlID="EmailTextBox"
                    CssClass="login-field__label"
                    Text="メールアドレス" />

                <asp:TextBox
                    ID="EmailTextBox"
                    runat="server"
                    TextMode="Email"
                    CssClass="login-field__input"
                    MaxLength="256"
                    autocomplete="username"
                    placeholder="メールアドレスを入力" />

                <asp:RequiredFieldValidator
                    ID="EmailRequiredValidator"
                    runat="server"
                    ControlToValidate="EmailTextBox"
                    ValidationGroup="Login"
                    CssClass="login-field__error"
                    ErrorMessage="メールアドレスを入力してください。"
                    Display="Dynamic" />
            </div>

            <div class="login-field">
                <asp:Label
                    ID="PasswordLabel"
                    runat="server"
                    AssociatedControlID="PasswordTextBox"
                    CssClass="login-field__label"
                    Text="パスワード" />

                <div class="login-password">
                    <asp:TextBox
                        ID="PasswordTextBox"
                        runat="server"
                        ClientIDMode="Static"
                        TextMode="Password"
                        CssClass="login-field__input login-password__input"
                        MaxLength="128"
                        autocomplete="current-password"
                        placeholder="パスワードを入力" />

                    <button
                        type="button"
                        class="login-password__toggle"
                        aria-label="パスワードを表示"
                        aria-pressed="false"
                        onclick="toggleLoginPassword(this)">
                        表示
                    </button>
                </div>

                <asp:RequiredFieldValidator
                    ID="PasswordRequiredValidator"
                    runat="server"
                    ControlToValidate="PasswordTextBox"
                    ValidationGroup="Login"
                    CssClass="login-field__error"
                    ErrorMessage="パスワードを入力してください。"
                    Display="Dynamic" />
            </div>

            <div class="login-options">
                <asp:CheckBox
                    ID="RememberMeCheckBox"
                    runat="server"
                    CssClass="login-remember"
                    Text="ログイン状態を保持する" />
            </div>

            <asp:Button
                ID="LoginButton"
                runat="server"
                Text="ログイン"
                CssClass="login-submit"
                ValidationGroup="Login"
                OnClick="LoginButton_Click" />

        </section>
    </main>

    <script>
        function toggleLoginPassword(button) {
            const passwordInput =
                document.getElementById("PasswordTextBox");

            if (!passwordInput) {
                return;
            }

            const showPassword =
                passwordInput.type === "password";

            passwordInput.type =
                showPassword ? "text" : "password";

            button.textContent =
                showPassword ? "非表示" : "表示";

            button.setAttribute(
                "aria-pressed",
                showPassword ? "true" : "false");

            button.setAttribute(
                "aria-label",
                showPassword
                    ? "パスワードを非表示"
                    : "パスワードを表示");
        }
    </script>

</asp:Content>
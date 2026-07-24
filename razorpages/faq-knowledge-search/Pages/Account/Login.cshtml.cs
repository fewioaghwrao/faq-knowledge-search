using System.ComponentModel.DataAnnotations;
using FaqKnowledgeSearch.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FaqKnowledgeSearch.Razor.Pages.Account;

[AllowAnonymous]
public sealed class LoginModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public LoginModel(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ReturnUrl { get; private set; }

    public IActionResult OnGet(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToPage("/Admin/Index");
        }

        ReturnUrl = returnUrl;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(
        string? returnUrl = null)
    {
        ReturnUrl = returnUrl;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var email = Input.Email.Trim();

        var user =
            await _userManager.FindByEmailAsync(email);

        // ユーザーの存在や権限を画面上で区別しない
        if (user is null
            || !user.IsActive
            || !await _userManager.IsInRoleAsync(
                user,
                AppRoles.Admin))
        {
            AddLoginError();

            return Page();
        }

        var result =
            await _signInManager.PasswordSignInAsync(
                user,
                Input.Password,
                isPersistent: false,
                lockoutOnFailure: true);

        if (result.Succeeded)
        {
            var destination =
                Url.IsLocalUrl(returnUrl)
                    ? returnUrl
                    : Url.Page("/Admin/Index");

            return LocalRedirect(destination!);
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(
                string.Empty,
                "ログイン試行回数が上限に達しました。"
                + "時間を置いてから再度お試しください。");

            return Page();
        }

        AddLoginError();

        return Page();
    }

    private void AddLoginError()
    {
        ModelState.AddModelError(
            string.Empty,
            "メールアドレスまたはパスワードが正しくありません。");
    }

    public sealed class InputModel
    {
        [Required(
            ErrorMessage = "メールアドレスを入力してください。")]
        [EmailAddress(
            ErrorMessage = "正しいメールアドレス形式で入力してください。")]
        [Display(Name = "メールアドレス")]
        public string Email { get; set; } = string.Empty;

        [Required(
            ErrorMessage = "パスワードを入力してください。")]
        [DataType(DataType.Password)]
        [Display(Name = "パスワード")]
        public string Password { get; set; } = string.Empty;
    }
}
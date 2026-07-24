using System.Security.Claims;
using FaqKnowledgeSearch.Application.Common.Models;
using FaqKnowledgeSearch.Application.Users.Admin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FaqKnowledgeSearch.Razor.Pages.Admin.Users;

public sealed class IndexModel(
    IAdminUserService adminUserService)
    : PageModel
{
    private const int DefaultPageSize = 5;

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    public string CurrentUserId { get; private set; }
        = string.Empty;

    public PagedResult<AdminUserListItem> Result
    { get; private set; }
        = new(
            Items: [],
            TotalCount: 0,
            Page: 1,
            PageSize: DefaultPageSize);

    public int DisplayTotalPages =>
        Math.Max(Result.TotalPages, 1);

    public async Task OnGetAsync(
        CancellationToken cancellationToken)
    {
        CurrentUserId =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier)
            ?? string.Empty;

        PageNumber = Math.Max(PageNumber, 1);

        Result = await adminUserService.SearchAsync(
            PageNumber,
            DefaultPageSize,
            cancellationToken);

        if (Result.TotalPages > 0 &&
            PageNumber > Result.TotalPages)
        {
            PageNumber = Result.TotalPages;

            Result = await adminUserService.SearchAsync(
                PageNumber,
                DefaultPageSize,
                cancellationToken);
        }
    }

    public async Task<IActionResult> OnPostToggleActiveAsync(
        string id,
        bool isActive,
        int pageNumber,
        CancellationToken cancellationToken)
    {
        var currentUserId =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return Challenge();
        }

        var result =
            await adminUserService.SetActiveAsync(
                id,
                isActive,
                currentUserId,
                cancellationToken);

        switch (result)
        {
            case AdminUserStatusChangeResult.Success:
                TempData["SuccessMessage"] =
                    isActive
                        ? "ユーザーを有効化しました。"
                        : "ユーザーを無効化しました。";
                break;

            case AdminUserStatusChangeResult.NotFound:
                TempData["ErrorMessage"] =
                    "対象のユーザーが見つかりませんでした。";
                break;

            case AdminUserStatusChangeResult.CannotChangeAdmin:
                TempData["ErrorMessage"] =
                    "管理者ユーザーの状態は変更できません。";
                break;

            case AdminUserStatusChangeResult
                .CannotChangeCurrentUser:
                TempData["ErrorMessage"] =
                    "ログイン中のユーザー自身は無効化できません。";
                break;

            default:
                TempData["ErrorMessage"] =
                    "ユーザー状態の変更に失敗しました。";
                break;
        }

        return RedirectToPage(
            "/Admin/Users/Index",
            new
            {
                pageNumber = Math.Max(pageNumber, 1)
            });
    }
}

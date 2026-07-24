using FaqKnowledgeSearch.Application.Common.Models;
using FaqKnowledgeSearch.Application.Faqs.Admin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FaqKnowledgeSearch.Razor.Pages.Admin.Faqs;

public sealed class IndexModel(
    IAdminFaqQuery adminFaqQuery,
    IAdminFaqService adminFaqService)
    : PageModel
{
    private const int DefaultPageSize = 5;

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    public PagedResult<AdminFaqListItem> Result { get; private set; }
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
        PageNumber = Math.Max(PageNumber, 1);

        Result = await adminFaqQuery.SearchAsync(
            PageNumber,
            DefaultPageSize,
            cancellationToken);

        if (Result.TotalPages > 0 &&
            PageNumber > Result.TotalPages)
        {
            PageNumber = Result.TotalPages;

            Result = await adminFaqQuery.SearchAsync(
                PageNumber,
                DefaultPageSize,
                cancellationToken);
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(
        int id,
        int pageNumber,
        CancellationToken cancellationToken)
    {
        var deleted = await adminFaqService.DeleteAsync(
            id,
            cancellationToken);

        if (!deleted)
        {
            TempData["ErrorMessage"] =
                "削除対象のFAQが見つかりませんでした。";

            return RedirectToPage(
                "/Admin/Faqs/Index",
                new
                {
                    pageNumber = Math.Max(pageNumber, 1)
                });
        }

        TempData["SuccessMessage"] =
            $"FAQ ID #{id} を削除しました。";

        return RedirectToPage(
            "/Admin/Faqs/Index",
            new
            {
                pageNumber = Math.Max(pageNumber, 1)
            });
    }
}
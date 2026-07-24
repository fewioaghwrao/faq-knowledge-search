using FaqKnowledgeSearch.Application.AiSearch.History.Admin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FaqKnowledgeSearch.Razor.Pages.Admin.AiSearchHistories;

public sealed class DetailsModel(
    IAdminAiSearchHistoryQuery historyQuery)
    : PageModel
{
    [BindProperty(SupportsGet = true)]
    public long Id { get; set; }

    public AdminAiSearchHistoryDetail History
    { get; private set; } = null!;

    public async Task<IActionResult> OnGetAsync(
        CancellationToken cancellationToken)
    {
        if (Id <= 0)
        {
            return NotFound();
        }

        var history = await historyQuery.GetDetailAsync(
            Id,
            cancellationToken);

        if (history is null)
        {
            return NotFound();
        }

        History = history;

        return Page();
    }
}
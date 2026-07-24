using FaqKnowledgeSearch.Application.AiSearch.History.Admin;
using FaqKnowledgeSearch.Application.Common.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FaqKnowledgeSearch.Razor.Pages.Admin.AiSearchHistories;

public sealed class IndexModel(
    IAdminAiSearchHistoryQuery historyQuery)
    : PageModel
{
    private const int DefaultPageSize = 10;

    [BindProperty(SupportsGet = true)]
    public string? Keyword { get; set; }

    [BindProperty(SupportsGet = true)]
    public AdminAiSearchStatusFilter Status { get; set; }
        = AdminAiSearchStatusFilter.All;

    [BindProperty(SupportsGet = true)]
    public AdminAiSearchFeedbackFilter Feedback { get; set; }
        = AdminAiSearchFeedbackFilter.All;

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    public PagedResult<AdminAiSearchHistoryListItem> Result
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
        PageNumber = Math.Max(PageNumber, 1);

        Result = await SearchAsync(
            PageNumber,
            cancellationToken);

        if (Result.TotalPages > 0
            && PageNumber > Result.TotalPages)
        {
            PageNumber = Result.TotalPages;

            Result = await SearchAsync(
                PageNumber,
                cancellationToken);
        }
    }

    private Task<PagedResult<AdminAiSearchHistoryListItem>>
        SearchAsync(
            int page,
            CancellationToken cancellationToken)
    {
        return historyQuery.SearchAsync(
            new AdminAiSearchHistorySearchCondition
            {
                Keyword = Keyword,
                Status = Status,
                Feedback = Feedback,
                Page = page,
                PageSize = DefaultPageSize
            },
            cancellationToken);
    }
}
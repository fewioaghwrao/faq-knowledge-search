using FaqKnowledgeSearch.Application.Common.Models;
using FaqKnowledgeSearch.Application.Faqs.Public;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FaqKnowledgeSearch.Razor.Pages.Faqs;

public sealed class IndexModel(
    IPublicFaqService publicFaqService)
    : PageModel
{
    private const int DefaultPageSize = 10;

    [BindProperty(SupportsGet = true)]
    public string? Keyword { get; set; }

    [BindProperty(SupportsGet = true)]
    public FaqSortOrder SortOrder { get; set; }
        = FaqSortOrder.Relevance;

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    public PagedResult<PublicFaqSearchItem> SearchResult
    {
        get;
        private set;
    } = new(
        Items: [],
        TotalCount: 0,
        Page: 1,
        PageSize: DefaultPageSize);

    public async Task OnGetAsync()
    {
        var condition = new PublicFaqSearchCondition
        {
            Keyword = Keyword,
            SortOrder = SortOrder,
            Page = PageNumber,
            PageSize = DefaultPageSize
        };

        SearchResult = await publicFaqService.SearchAsync(
            condition,
            HttpContext.RequestAborted);

        // 0や負数を指定された場合、
        // Service側で補正されたページ番号を画面にも反映する。
        PageNumber = SearchResult.Page;
    }

    public string GetSortOrderLabel()
    {
        return SortOrder switch
        {
            FaqSortOrder.Newest => "新着順",
            FaqSortOrder.MostViewed => "閲覧数順",
            _ => "関連度順"
        };
    }
}

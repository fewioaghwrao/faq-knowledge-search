using FaqKnowledgeSearch.Application.Faqs.Admin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FaqKnowledgeSearch.Razor.Pages.Admin.Faqs;

public sealed class CreateModel(
    IAdminFaqService adminFaqService)
    : PageModel
{
    [BindProperty]
    public FaqFormInput Input { get; set; } = new();

    public IReadOnlyList<AdminFaqOption> CategoryOptions
    { get; private set; } = [];

    public IReadOnlyList<AdminFaqOption> TagOptions
    { get; private set; } = [];

    public async Task OnGetAsync(
        CancellationToken cancellationToken)
    {
        await LoadOptionsAsync(cancellationToken);
    }

    public async Task<IActionResult> OnPostAsync(
        CancellationToken cancellationToken)
    {
        if (!Input.TryParseTagIds(out var tagIds))
        {
            ModelState.AddModelError(
                "Input.TagIds",
                "タグIDは「1,2」のように数値をカンマ区切りで入力してください。");
        }

        if (!ModelState.IsValid)
        {
            await LoadOptionsAsync(cancellationToken);
            return Page();
        }

        try
        {
            await adminFaqService.CreateAsync(
                new AdminFaqCommand(
                    Input.Title,
                    Input.Body,
                    Input.CategoryId,
                    tagIds,
                    Input.IsPublished),
                cancellationToken);
        }
        catch (ArgumentException exception)
        {
            ModelState.AddModelError(
                string.Empty,
                exception.Message);

            await LoadOptionsAsync(cancellationToken);

            return Page();
        }

        TempData["SuccessMessage"] =
            "FAQを登録しました。";

        return RedirectToPage("/Admin/Faqs/Index");
    }

    private async Task LoadOptionsAsync(
        CancellationToken cancellationToken)
    {
        var options =
            await adminFaqService.GetFormOptionsAsync(
                cancellationToken);

        CategoryOptions = options.Categories;
        TagOptions = options.Tags;
    }
}
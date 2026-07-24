using FaqKnowledgeSearch.Application.Faqs.Admin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FaqKnowledgeSearch.Razor.Pages.Admin.Faqs;

public sealed class EditModel(
    IAdminFaqService adminFaqService)
    : PageModel
{
    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    [BindProperty]
    public FaqFormInput Input { get; set; } = new();

    public IReadOnlyList<AdminFaqOption> CategoryOptions
    { get; private set; } = [];

    public IReadOnlyList<AdminFaqOption> TagOptions
    { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(
        CancellationToken cancellationToken)
    {
        var faq = await adminFaqService.GetEditDataAsync(
            Id,
            cancellationToken);

        if (faq is null)
        {
            return NotFound();
        }

        Input = new FaqFormInput
        {
            Title = faq.Title,
            Body = faq.Body,
            CategoryId = faq.CategoryId,
            TagIds = string.Join(",", faq.TagIds),
            IsPublished = faq.IsPublished
        };

        await LoadOptionsAsync(cancellationToken);

        return Page();
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
            var updated = await adminFaqService.UpdateAsync(
                Id,
                new AdminFaqCommand(
                    Input.Title,
                    Input.Body,
                    Input.CategoryId,
                    tagIds,
                    Input.IsPublished),
                cancellationToken);

            if (!updated)
            {
                return NotFound();
            }
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
            "FAQを更新しました。";

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
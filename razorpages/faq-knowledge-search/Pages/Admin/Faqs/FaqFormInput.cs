using System.ComponentModel.DataAnnotations;

namespace FaqKnowledgeSearch.Razor.Pages.Admin.Faqs;

public sealed class FaqFormInput
{
    [Required(ErrorMessage = "タイトルを入力してください。")]
    [StringLength(
        100,
        ErrorMessage = "タイトルは100文字以内で入力してください。")]
    [Display(Name = "タイトル")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "本文を入力してください。")]
    [Display(Name = "本文")]
    public string Body { get; set; } = string.Empty;

    [Range(
        1,
        int.MaxValue,
        ErrorMessage = "カテゴリを選択してください。")]
    [Display(Name = "カテゴリ")]
    public int CategoryId { get; set; }

    [Display(Name = "タグID")]
    public string TagIds { get; set; } = string.Empty;

    [Display(Name = "公開状態")]
    public bool IsPublished { get; set; } = true;

    public bool TryParseTagIds(
        out IReadOnlyList<int> tagIds)
    {
        tagIds = [];

        if (string.IsNullOrWhiteSpace(TagIds))
        {
            return true;
        }

        var values = TagIds.Split(
            new[] { ',', '、', ';' },
            StringSplitOptions.RemoveEmptyEntries |
            StringSplitOptions.TrimEntries);

        var parsedIds = new List<int>();

        foreach (var value in values)
        {
            if (!int.TryParse(value, out var tagId) ||
                tagId <= 0)
            {
                return false;
            }

            if (!parsedIds.Contains(tagId))
            {
                parsedIds.Add(tagId);
            }
        }

        tagIds = parsedIds;

        return true;
    }
}
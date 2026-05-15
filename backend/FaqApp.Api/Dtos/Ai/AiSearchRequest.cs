using System.ComponentModel.DataAnnotations;

namespace FaqApp.Api.Dtos.Ai;

public class AiSearchRequest
{
    [Required(ErrorMessage = "質問文は必須です。")]
    [MaxLength(500, ErrorMessage = "質問文は500文字以内で入力してください。")]
    public string Question { get; set; } = string.Empty;
}
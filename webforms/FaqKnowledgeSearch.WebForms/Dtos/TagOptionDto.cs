namespace FaqKnowledgeSearch.WebForms.Dtos
{
    /// <summary>
    /// FAQ編集画面のタグ選択肢です。
    /// </summary>
    public sealed class TagOptionDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int DisplayOrder { get; set; }
    }
}
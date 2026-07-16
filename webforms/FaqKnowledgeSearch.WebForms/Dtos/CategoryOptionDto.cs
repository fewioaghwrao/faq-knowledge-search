namespace FaqKnowledgeSearch.WebForms.Dtos
{
    public sealed class CategoryOptionDto
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public bool IsActive { get; set; }

        public string DisplayName
        {
            get
            {
                return IsActive
                    ? Name
                    : Name + "（無効）";
            }
        }
    }
}
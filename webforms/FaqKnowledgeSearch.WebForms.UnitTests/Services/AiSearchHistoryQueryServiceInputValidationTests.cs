using FaqKnowledgeSearch.WebForms.Services.Ai;
using Xunit;

namespace FaqKnowledgeSearch.WebForms.UnitTests.Services.Ai
{
    [Trait("Category", "Unit")]
    public sealed class AiSearchHistoryQueryServiceInputValidationTests
    {
        private readonly AiSearchHistoryQueryService _sut =
            new AiSearchHistoryQueryService();

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(long.MinValue)]
        public void GetHistoryById_Idが0以下の場合_Nullを返す(
            long id)
        {
            // Act
            var result =
                _sut.GetHistoryById(id);

            // Assert
            Assert.Null(result);
        }
    }
}
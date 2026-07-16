using System;
using System.Threading.Tasks;
using FaqKnowledgeSearch.WebForms.Services.Ai;
using Xunit;

namespace FaqKnowledgeSearch.WebForms.UnitTests.Services.Ai
{
    [Trait("Category", "Unit")]
    public sealed class AiSearchFeedbackServiceInputValidationTests
    {
        private readonly AiSearchFeedbackService _sut;

        public AiSearchFeedbackServiceInputValidationTests()
        {
            _sut = new AiSearchFeedbackService();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(long.MinValue)]
        public async Task SaveAsync_履歴Idが0以下の場合_ArgumentOutOfRangeExceptionを投げる(
            long aiSearchHistoryId)
        {
            // Act
            var exception =
                await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                    () => _sut.SaveAsync(
                        aiSearchHistoryId,
                        true,
                        "役に立ちました。"));

            // Assert
            Assert.Equal(
                "aiSearchHistoryId",
                exception.ParamName);

            Assert.Contains(
                "AI検索履歴IDが正しくありません。",
                exception.Message);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task SaveAsync_履歴Idが不正の場合_評価値にかかわらず例外を投げる(
            bool isHelpful)
        {
            // Act
            var exception =
                await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                    () => _sut.SaveAsync(
                        0,
                        isHelpful,
                        null));

            // Assert
            Assert.Equal(
                "aiSearchHistoryId",
                exception.ParamName);
        }
    }
}
using System;
using System.Threading.Tasks;
using FaqKnowledgeSearch.WebForms.Services.Ai;
using Xunit;

namespace FaqKnowledgeSearch.WebForms.UnitTests.Services.Ai
{
    [Trait("Category", "Unit")]
    public sealed class AiSearchHistoryServiceInputValidationTests
    {
        private readonly AiSearchHistoryService _sut =
            new AiSearchHistoryService();

        #region SaveSuccessAsync

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t")]
        [InlineData("\r\n")]
        public async Task SaveSuccessAsync_質問が空の場合_ArgumentExceptionを投げる(
            string question)
        {
            // Act
            var exception =
                await Assert.ThrowsAsync<ArgumentException>(
                    () => _sut.SaveSuccessAsync(
                        question,
                        "AI回答",
                        null));

            // Assert
            Assert.Equal(
                "question",
                exception.ParamName);

            Assert.StartsWith(
                "履歴へ保存する質問文がありません。",
                exception.Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t")]
        [InlineData("\r\n")]
        public async Task SaveSuccessAsync_回答が空の場合_ArgumentExceptionを投げる(
            string answer)
        {
            // Act
            var exception =
                await Assert.ThrowsAsync<ArgumentException>(
                    () => _sut.SaveSuccessAsync(
                        "パスワードの変更方法を教えてください。",
                        answer,
                        null));

            // Assert
            Assert.Equal(
                "answer",
                exception.ParamName);

            Assert.StartsWith(
                "成功履歴にはAI回答が必要です。",
                exception.Message);
        }

        [Fact]
        public async Task SaveSuccessAsync_質問と回答が空の場合_質問の例外を先に投げる()
        {
            // Act
            var exception =
                await Assert.ThrowsAsync<ArgumentException>(
                    () => _sut.SaveSuccessAsync(
                        null,
                        null,
                        null));

            // Assert
            Assert.Equal(
                "question",
                exception.ParamName);
        }

        #endregion

        #region SaveFailureAsync

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t")]
        [InlineData("\r\n")]
        public async Task SaveFailureAsync_質問が空の場合_ArgumentExceptionを投げる(
            string question)
        {
            // Act
            var exception =
                await Assert.ThrowsAsync<ArgumentException>(
                    () => _sut.SaveFailureAsync(
                        question,
                        "AI検索に失敗しました。",
                        null));

            // Assert
            Assert.Equal(
                "question",
                exception.ParamName);

            Assert.StartsWith(
                "履歴へ保存する質問文がありません。",
                exception.Message);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task SaveFailureAsync_質問が空の場合_Sourcesの有無に関係なく例外を投げる(
            bool useEmptySources)
        {
            // Arrange
            var sources = useEmptySources
                ? new FaqKnowledgeSearch.WebForms.Dtos.FaqListItemDto[0]
                : null;

            // Act
            var exception =
                await Assert.ThrowsAsync<ArgumentException>(
                    () => _sut.SaveFailureAsync(
                        " ",
                        null,
                        sources));

            // Assert
            Assert.Equal(
                "question",
                exception.ParamName);
        }

        #endregion
    }
}
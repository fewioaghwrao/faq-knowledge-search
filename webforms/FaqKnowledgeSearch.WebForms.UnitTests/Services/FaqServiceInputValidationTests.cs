using System;
using FaqKnowledgeSearch.WebForms.Dtos;
using FaqKnowledgeSearch.WebForms.Services;
using Xunit;

namespace FaqKnowledgeSearch.WebForms.Tests.Services
{
    [Trait("Category", "Unit")]
    public sealed class FaqServiceInputValidationTests
    {
        private readonly FaqService _sut;

        public FaqServiceInputValidationTests()
        {
            _sut = new FaqService();
        }

        #region GetPublishedFaqById

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void GetPublishedFaqById_Idが0以下の場合_Nullを返す(
            long id)
        {
            // Act
            var result =
                _sut.GetPublishedFaqById(id);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region GetFaqForAdmin

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void GetFaqForAdmin_Idが0以下の場合_Nullを返す(
            long id)
        {
            // Act
            var result =
                _sut.GetFaqForAdmin(id);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region DeleteFaq

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void DeleteFaq_Idが0以下の場合_Falseを返す(
            long id)
        {
            // Act
            var result =
                _sut.DeleteFaq(id);

            // Assert
            Assert.False(result);
        }

        #endregion

        #region UpdateFaq：IDチェック

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void UpdateFaq_Idが0以下の場合_InputがNullでもFalseを返す(
            long id)
        {
            // Act
            var result =
                _sut.UpdateFaq(id, null);

            // Assert
            Assert.False(result);
        }

        #endregion

        #region CreateFaq：入力検証

        [Fact]
        public void CreateFaq_InputがNullの場合_ArgumentNullExceptionを投げる()
        {
            // Act
            var exception =
                Assert.Throws<ArgumentNullException>(
                    () => _sut.CreateFaq(null));

            // Assert
            Assert.Equal(
                "input",
                exception.ParamName);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void CreateFaq_カテゴリIdが0以下の場合_ArgumentExceptionを投げる(
            long categoryId)
        {
            // Arrange
            var input = CreateValidInput();
            input.CategoryId = categoryId;

            // Act
            var exception =
                Assert.Throws<ArgumentException>(
                    () => _sut.CreateFaq(input));

            // Assert
            Assert.Equal(
                "カテゴリを選択してください。",
                exception.Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void CreateFaq_質問が空の場合_ArgumentExceptionを投げる(
            string question)
        {
            // Arrange
            var input = CreateValidInput();
            input.Question = question;

            // Act
            var exception =
                Assert.Throws<ArgumentException>(
                    () => _sut.CreateFaq(input));

            // Assert
            Assert.Equal(
                "質問を入力してください。",
                exception.Message);
        }

        [Fact]
        public void CreateFaq_質問が501文字の場合_ArgumentExceptionを投げる()
        {
            // Arrange
            var input = CreateValidInput();
            input.Question = new string('あ', 501);

            // Act
            var exception =
                Assert.Throws<ArgumentException>(
                    () => _sut.CreateFaq(input));

            // Assert
            Assert.Equal(
                "質問は500文字以内で入力してください。",
                exception.Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void CreateFaq_回答が空の場合_ArgumentExceptionを投げる(
            string answer)
        {
            // Arrange
            var input = CreateValidInput();
            input.Answer = answer;

            // Act
            var exception =
                Assert.Throws<ArgumentException>(
                    () => _sut.CreateFaq(input));

            // Assert
            Assert.Equal(
                "回答を入力してください。",
                exception.Message);
        }

        #endregion

        #region UpdateFaq：入力検証

        [Fact]
        public void UpdateFaq_InputがNullの場合_ArgumentNullExceptionを投げる()
        {
            // Act
            var exception =
                Assert.Throws<ArgumentNullException>(
                    () => _sut.UpdateFaq(1, null));

            // Assert
            Assert.Equal(
                "input",
                exception.ParamName);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void UpdateFaq_カテゴリIdが0以下の場合_ArgumentExceptionを投げる(
            long categoryId)
        {
            // Arrange
            var input = CreateValidInput();
            input.CategoryId = categoryId;

            // Act
            var exception =
                Assert.Throws<ArgumentException>(
                    () => _sut.UpdateFaq(1, input));

            // Assert
            Assert.Equal(
                "カテゴリを選択してください。",
                exception.Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void UpdateFaq_質問が空の場合_ArgumentExceptionを投げる(
            string question)
        {
            // Arrange
            var input = CreateValidInput();
            input.Question = question;

            // Act
            var exception =
                Assert.Throws<ArgumentException>(
                    () => _sut.UpdateFaq(1, input));

            // Assert
            Assert.Equal(
                "質問を入力してください。",
                exception.Message);
        }

        [Fact]
        public void UpdateFaq_質問が501文字の場合_ArgumentExceptionを投げる()
        {
            // Arrange
            var input = CreateValidInput();
            input.Question = new string('あ', 501);

            // Act
            var exception =
                Assert.Throws<ArgumentException>(
                    () => _sut.UpdateFaq(1, input));

            // Assert
            Assert.Equal(
                "質問は500文字以内で入力してください。",
                exception.Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void UpdateFaq_回答が空の場合_ArgumentExceptionを投げる(
            string answer)
        {
            // Arrange
            var input = CreateValidInput();
            input.Answer = answer;

            // Act
            var exception =
                Assert.Throws<ArgumentException>(
                    () => _sut.UpdateFaq(1, input));

            // Assert
            Assert.Equal(
                "回答を入力してください。",
                exception.Message);
        }

        #endregion

        private static FaqEditDto CreateValidInput()
        {
            return new FaqEditDto
            {
                CategoryId = 1,
                Question = "GitHub Actionsの使い方を教えてください。",
                Answer = "リポジトリのActions画面から確認できます。",
                IsPublished = true
            };
        }
    }
}
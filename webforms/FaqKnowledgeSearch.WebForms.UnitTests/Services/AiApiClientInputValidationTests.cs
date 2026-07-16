using System;
using System.Threading.Tasks;
using FaqKnowledgeSearch.WebForms.Services.Ai;
using FaqKnowledgeSearch.WebForms.Settings;
using Xunit;

namespace FaqKnowledgeSearch.WebForms.UnitTests.Services.Ai
{
    [Trait("Category", "Unit")]
    public sealed class AiApiClientInputValidationTests
    {
        [Fact]
        public void Constructor_SettingsがNullの場合_ArgumentNullExceptionを投げる()
        {
            var exception =
                Assert.Throws<ArgumentNullException>(
                    () => new AiApiClient(null));

            Assert.Equal(
                "settings",
                exception.ParamName);
        }

        [Fact]
        public async Task GenerateAnswerAsync_ApiKeyが未設定の場合_InvalidOperationExceptionを投げる()
        {
            var settings = CreateValidSettings();
            settings.ApiKey = null;

            var sut = new AiApiClient(settings);

            var exception =
                await Assert.ThrowsAsync<InvalidOperationException>(
                    () => sut.GenerateAnswerAsync(
                        "パスワードを変更する方法は？",
                        "パスワードは設定画面から変更できます。"));

            Assert.Equal(
                "AI APIキーが設定されていません。",
                exception.Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GenerateAnswerAsync_ApiKeyが空の場合_InvalidOperationExceptionを投げる(
            string apiKey)
        {
            var settings = CreateValidSettings();
            settings.ApiKey = apiKey;

            var sut = new AiApiClient(settings);

            var exception =
                await Assert.ThrowsAsync<InvalidOperationException>(
                    () => sut.GenerateAnswerAsync(
                        "質問",
                        "FAQコンテキスト"));

            Assert.Equal(
                "AI APIキーが設定されていません。",
                exception.Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GenerateAnswerAsync_Endpointが空の場合_InvalidOperationExceptionを投げる(
            string endpoint)
        {
            var settings = CreateValidSettings();
            settings.Endpoint = endpoint;

            var sut = new AiApiClient(settings);

            var exception =
                await Assert.ThrowsAsync<InvalidOperationException>(
                    () => sut.GenerateAnswerAsync(
                        "質問",
                        "FAQコンテキスト"));

            Assert.Equal(
                "AI APIエンドポイントが設定されていません。",
                exception.Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GenerateAnswerAsync_Modelが空の場合_InvalidOperationExceptionを投げる(
            string model)
        {
            var settings = CreateValidSettings();
            settings.Model = model;

            var sut = new AiApiClient(settings);

            var exception =
                await Assert.ThrowsAsync<InvalidOperationException>(
                    () => sut.GenerateAnswerAsync(
                        "質問",
                        "FAQコンテキスト"));

            Assert.Equal(
                "AIモデルが設定されていません。",
                exception.Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GenerateAnswerAsync_質問が空の場合_ArgumentExceptionを投げる(
            string question)
        {
            var sut =
                new AiApiClient(
                    CreateValidSettings());

            var exception =
                await Assert.ThrowsAsync<ArgumentException>(
                    () => sut.GenerateAnswerAsync(
                        question,
                        "FAQコンテキスト"));

            Assert.Equal(
                "question",
                exception.ParamName);

            Assert.StartsWith(
                "質問文が入力されていません。",
                exception.Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GenerateAnswerAsync_FaqContextが空の場合_ArgumentExceptionを投げる(
            string faqContext)
        {
            var sut =
                new AiApiClient(
                    CreateValidSettings());

            var exception =
                await Assert.ThrowsAsync<ArgumentException>(
                    () => sut.GenerateAnswerAsync(
                        "質問",
                        faqContext));

            Assert.Equal(
                "faqContext",
                exception.ParamName);

            Assert.StartsWith(
                "FAQコンテキストが設定されていません。",
                exception.Message);
        }

        [Fact]
        public async Task GenerateAnswerAsync_設定と質問の両方が不正の場合_設定エラーを先に返す()
        {
            var settings = CreateValidSettings();
            settings.ApiKey = null;

            var sut = new AiApiClient(settings);

            var exception =
                await Assert.ThrowsAsync<InvalidOperationException>(
                    () => sut.GenerateAnswerAsync(
                        null,
                        null));

            Assert.Equal(
                "AI APIキーが設定されていません。",
                exception.Message);
        }

        private static AiSettings CreateValidSettings()
        {
            return new AiSettings
            {
                ApiKey = "unit-test-api-key",
                Endpoint =
                    "https://example.invalid/v1/responses",
                Model = "unit-test-model"
            };
        }
    }
}
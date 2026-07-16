using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FaqKnowledgeSearch.WebForms.Dtos;
using FaqKnowledgeSearch.WebForms.Services;
using FaqKnowledgeSearch.WebForms.Services.Ai;
using FaqKnowledgeSearch.WebForms.Settings;
using Moq;
using Xunit;

namespace FaqKnowledgeSearch.WebForms.UnitTests.Services.Ai
{
    [Trait("Category", "Unit")]
    public sealed class AiServiceTests
    {
        private readonly Mock<IFaqService> _faqService;
        private readonly Mock<IAiApiClient> _aiApiClient;
        private readonly Mock<IAiSearchHistoryService>
            _historyService;

        public AiServiceTests()
        {
            _faqService =
                new Mock<IFaqService>();

            _aiApiClient =
                new Mock<IAiApiClient>();

            _historyService =
                new Mock<IAiSearchHistoryService>();
        }

        #region Constructor

        [Fact]
        public void Constructor_FaqServiceがNullの場合_ArgumentNullException()
        {
            var exception =
                Assert.Throws<ArgumentNullException>(
                    () => new AiService(
                        null,
                        _aiApiClient.Object,
                        _historyService.Object,
                        CreateSettings()));

            Assert.Equal(
                "faqService",
                exception.ParamName);
        }

        [Fact]
        public void Constructor_AiApiClientがNullの場合_ArgumentNullException()
        {
            var exception =
                Assert.Throws<ArgumentNullException>(
                    () => new AiService(
                        _faqService.Object,
                        null,
                        _historyService.Object,
                        CreateSettings()));

            Assert.Equal(
                "aiApiClient",
                exception.ParamName);
        }

        [Fact]
        public void Constructor_HistoryServiceがNullの場合_ArgumentNullException()
        {
            var exception =
                Assert.Throws<ArgumentNullException>(
                    () => new AiService(
                        _faqService.Object,
                        _aiApiClient.Object,
                        null,
                        CreateSettings()));

            Assert.Equal(
                "historyService",
                exception.ParamName);
        }

        [Fact]
        public void Constructor_SettingsがNullの場合_ArgumentNullException()
        {
            var exception =
                Assert.Throws<ArgumentNullException>(
                    () => new AiService(
                        _faqService.Object,
                        _aiApiClient.Object,
                        _historyService.Object,
                        null));

            Assert.Equal(
                "settings",
                exception.ParamName);
        }

        #endregion

        #region Input validation

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t")]
        [InlineData("\r\n")]
        public async Task SearchAsync_質問が空の場合_入力メッセージを返す(
            string question)
        {
            var sut = CreateSut();

            var result =
                await sut.SearchAsync(question);

            Assert.Null(result.Answer);
            Assert.Null(result.Disclaimer);
            Assert.Equal(
                "質問文を入力してください。",
                result.Message);
            Assert.Equal(0, result.AiHistoryId);
            Assert.Empty(result.Sources);

            _faqService.Verify(
                x => x.SearchPublishedFaqs(
                    It.IsAny<string>()),
                Times.Never);

            _aiApiClient.Verify(
                x => x.GenerateAnswerAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>()),
                Times.Never);

            _historyService.Verify(
                x => x.SaveSuccessAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<
                        IReadOnlyList<FaqListItemDto>>()),
                Times.Never);

            _historyService.Verify(
                x => x.SaveFailureAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<
                        IReadOnlyList<FaqListItemDto>>()),
                Times.Never);
        }

        [Fact]
        public async Task SearchAsync_質問が501文字の場合_文字数エラーを返す()
        {
            var sut = CreateSut();

            var result =
                await sut.SearchAsync(
                    new string('あ', 501));

            Assert.Null(result.Answer);
            Assert.Equal(
                "質問文は500文字以内で入力してください。",
                result.Message);
            Assert.Equal(0, result.AiHistoryId);
            Assert.Empty(result.Sources);

            _faqService.Verify(
                x => x.SearchPublishedFaqs(
                    It.IsAny<string>()),
                Times.Never);
        }

        #endregion

        #region No FAQ

        [Fact]
        public async Task SearchAsync_関連Faqがない場合_失敗履歴を保存して案内を返す()
        {
            _faqService
                .Setup(x =>
                    x.SearchPublishedFaqs(
                        "パスワード変更"))
                .Returns(
                    new List<FaqListItemDto>());

            _historyService
                .Setup(x =>
                    x.SaveFailureAsync(
                        "パスワード変更",
                        "該当するFAQが見つかりませんでした。",
                        It.Is<
                            IReadOnlyList<FaqListItemDto>>(
                            sources =>
                                sources.Count == 0)))
                .ReturnsAsync(101);

            var sut = CreateSut();

            var result =
                await sut.SearchAsync(
                    "  パスワード変更  ");

            Assert.Null(result.Answer);
            Assert.Null(result.Disclaimer);
            Assert.Equal(
                "該当するFAQが見つかりませんでした。" +
                "キーワードを変えて検索してください。",
                result.Message);
            Assert.Equal(101, result.AiHistoryId);
            Assert.Empty(result.Sources);

            _faqService.Verify(
                x => x.SearchPublishedFaqs(
                    "パスワード変更"),
                Times.Once);

            _aiApiClient.Verify(
                x => x.GenerateAnswerAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>()),
                Times.Never);

            _historyService.Verify(
                x => x.SaveFailureAsync(
                    "パスワード変更",
                    "該当するFAQが見つかりませんでした。",
                    It.Is<
                        IReadOnlyList<FaqListItemDto>>(
                        sources =>
                            sources.Count == 0)),
                Times.Once);
        }

        [Fact]
        public async Task SearchAsync_関連Faqなしで履歴保存に失敗しても案内を返す()
        {
            _faqService
                .Setup(x =>
                    x.SearchPublishedFaqs(
                        It.IsAny<string>()))
                .Returns(
                    new List<FaqListItemDto>());

            _historyService
                .Setup(x =>
                    x.SaveFailureAsync(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<
                            IReadOnlyList<FaqListItemDto>>()))
                .ThrowsAsync(
                    new InvalidOperationException(
                        "DBエラー"));

            var sut = CreateSut();

            var result =
                await sut.SearchAsync("質問");

            Assert.Equal(
                "該当するFAQが見つかりませんでした。" +
                "キーワードを変えて検索してください。",
                result.Message);

            Assert.Equal(
                0,
                result.AiHistoryId);
        }

        #endregion

        #region Success

        [Fact]
        public async Task SearchAsync_Ai回答生成に成功した場合_回答と参照元を返す()
        {
            var faqs =
                new List<FaqListItemDto>
                {
                    CreateFaq(
                        10,
                        "アカウント",
                        "パスワード変更方法",
                        "設定画面から変更できます。",
                        "認証,パスワード")
                };

            _faqService
                .Setup(x =>
                    x.SearchPublishedFaqs(
                        "パスワードを変更したい"))
                .Returns(faqs);

            _aiApiClient
                .Setup(x =>
                    x.GenerateAnswerAsync(
                        "パスワードを変更したい",
                        It.IsAny<string>()))
                .ReturnsAsync(
                    "設定画面から変更できます。");

            _historyService
                .Setup(x =>
                    x.SaveSuccessAsync(
                        "パスワードを変更したい",
                        "設定画面から変更できます。",
                        It.Is<
                            IReadOnlyList<FaqListItemDto>>(
                            sources =>
                                sources.Count == 1 &&
                                sources[0].Id == 10)))
                .ReturnsAsync(200);

            var sut = CreateSut();

            var result =
                await sut.SearchAsync(
                    "  パスワードを変更したい  ");

            Assert.Equal(
                "設定画面から変更できます。",
                result.Answer);

            Assert.Equal(
                "この回答はFAQをもとに生成されています。" +
                "必ず参照元を確認してください。",
                result.Disclaimer);

            Assert.Null(result.Message);
            Assert.Equal(200, result.AiHistoryId);

            var source =
                Assert.Single(result.Sources);

            Assert.Equal(10, source.FaqId);
            Assert.Equal(
                "パスワード変更方法",
                source.Title);
            Assert.Equal(
                "~/Faqs/Detail.aspx?id=10",
                source.Url);

            _faqService.Verify(
                x => x.SearchPublishedFaqs(
                    "パスワードを変更したい"),
                Times.Once);

            _historyService.Verify(
                x => x.SaveSuccessAsync(
                    "パスワードを変更したい",
                    "設定画面から変更できます。",
                    It.IsAny<
                        IReadOnlyList<FaqListItemDto>>()),
                Times.Once);
        }

        [Fact]
        public async Task SearchAsync_成功履歴保存に失敗してもAi回答を返す()
        {
            var faqs =
                new List<FaqListItemDto>
                {
                    CreateFaq(
                        1,
                        "カテゴリ",
                        "質問",
                        "回答",
                        null)
                };

            _faqService
                .Setup(x =>
                    x.SearchPublishedFaqs(
                        It.IsAny<string>()))
                .Returns(faqs);

            _aiApiClient
                .Setup(x =>
                    x.GenerateAnswerAsync(
                        It.IsAny<string>(),
                        It.IsAny<string>()))
                .ReturnsAsync("AI回答");

            _historyService
                .Setup(x =>
                    x.SaveSuccessAsync(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<
                            IReadOnlyList<FaqListItemDto>>()))
                .ThrowsAsync(
                    new InvalidOperationException(
                        "DBエラー"));

            var sut = CreateSut();

            var result =
                await sut.SearchAsync("質問");

            Assert.Equal(
                "AI回答",
                result.Answer);
            Assert.Null(result.Message);
            Assert.Equal(0, result.AiHistoryId);
            Assert.Single(result.Sources);
        }

        #endregion

        #region API failure

        [Fact]
        public async Task SearchAsync_AiApiが例外の場合_失敗履歴と参照元を返す()
        {
            var faqs =
                new List<FaqListItemDto>
                {
                    CreateFaq(
                        20,
                        "申請",
                        "申請方法",
                        "申請画面から登録します。",
                        "申請")
                };

            _faqService
                .Setup(x =>
                    x.SearchPublishedFaqs(
                        "申請方法"))
                .Returns(faqs);

            _aiApiClient
                .Setup(x =>
                    x.GenerateAnswerAsync(
                        "申請方法",
                        It.IsAny<string>()))
                .ThrowsAsync(
                    new InvalidOperationException(
                        "AI APIエラー"));

            _historyService
                .Setup(x =>
                    x.SaveFailureAsync(
                        "申請方法",
                        "AI回答の生成に失敗しました。",
                        It.Is<
                            IReadOnlyList<FaqListItemDto>>(
                            sources =>
                                sources.Count == 1 &&
                                sources[0].Id == 20)))
                .ReturnsAsync(300);

            var sut = CreateSut();

            var result =
                await sut.SearchAsync(
                    "申請方法");

            Assert.Null(result.Answer);
            Assert.Null(result.Disclaimer);

            Assert.Equal(
                "AI回答の生成に失敗しました。" +
                "参照元FAQをご確認ください。",
                result.Message);

            Assert.Equal(300, result.AiHistoryId);

            var source =
                Assert.Single(result.Sources);

            Assert.Equal(20, source.FaqId);

            _historyService.Verify(
                x => x.SaveFailureAsync(
                    "申請方法",
                    "AI回答の生成に失敗しました。",
                    It.IsAny<
                        IReadOnlyList<FaqListItemDto>>()),
                Times.Once);
        }

        #endregion

        #region Maximum context count

        [Theory]
        [InlineData(2, 2)]
        [InlineData(0, 5)]
        [InlineData(-1, 5)]
        public async Task SearchAsync_MaxContextFaqCountに従ってFaq数を制限する(
            int configuredMaximumCount,
            int expectedCount)
        {
            var faqs =
                Enumerable.Range(1, 6)
                    .Select(index =>
                        CreateFaq(
                            index,
                            "カテゴリ" + index,
                            "質問" + index,
                            "回答" + index,
                            null))
                    .ToList();

            _faqService
                .Setup(x =>
                    x.SearchPublishedFaqs(
                        It.IsAny<string>()))
                .Returns(faqs);

            _aiApiClient
                .Setup(x =>
                    x.GenerateAnswerAsync(
                        It.IsAny<string>(),
                        It.IsAny<string>()))
                .ReturnsAsync("AI回答");

            _historyService
                .Setup(x =>
                    x.SaveSuccessAsync(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<
                            IReadOnlyList<FaqListItemDto>>()))
                .ReturnsAsync(1);

            var settings =
                new AiSettings
                {
                    MaxContextFaqCount =
                        configuredMaximumCount
                };

            var sut =
                CreateSut(settings);

            var result =
                await sut.SearchAsync("質問");

            Assert.Equal(
                expectedCount,
                result.Sources.Count);

            Assert.Equal(
                Enumerable.Range(
                    1,
                    expectedCount)
                    .Select(x => (long)x),
                result.Sources.Select(
                    x => x.FaqId));

            _historyService.Verify(
                x => x.SaveSuccessAsync(
                    "質問",
                    "AI回答",
                    It.Is<
                        IReadOnlyList<FaqListItemDto>>(
                        sources =>
                            sources.Count ==
                            expectedCount)),
                Times.Once);
        }

        #endregion

        #region FAQ context

        [Fact]
        public async Task SearchAsync_Faq項目が空の場合_既定値でContextを生成する()
        {
            var faqs =
                new List<FaqListItemDto>
                {
                    new FaqListItemDto
                    {
                        Id = 50,
                        CategoryName = " ",
                        Question = null,
                        Answer = "",
                        TagNames = "\t"
                    }
                };

            string capturedContext = null;

            _faqService
                .Setup(x =>
                    x.SearchPublishedFaqs(
                        It.IsAny<string>()))
                .Returns(faqs);

            _aiApiClient
                .Setup(x =>
                    x.GenerateAnswerAsync(
                        "質問",
                        It.IsAny<string>()))
                .Callback<string, string>(
                    (question, faqContext) =>
                        capturedContext =
                            faqContext)
                .ReturnsAsync("AI回答");

            _historyService
                .Setup(x =>
                    x.SaveSuccessAsync(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<
                            IReadOnlyList<FaqListItemDto>>()))
                .ReturnsAsync(1);

            var sut = CreateSut();

            await sut.SearchAsync("質問");

            Assert.NotNull(capturedContext);
            Assert.Contains(
                "[FAQ1]",
                capturedContext);
            Assert.Contains(
                "ID: 50",
                capturedContext);
            Assert.Contains(
                "カテゴリ: 未分類",
                capturedContext);
            Assert.Contains(
                "タグ: なし",
                capturedContext);
            Assert.Contains(
                "質問が登録されていません。",
                capturedContext);
            Assert.Contains(
                "回答が登録されていません。",
                capturedContext);
        }

        #endregion

        private AiService CreateSut(
            AiSettings settings = null)
        {
            return new AiService(
                _faqService.Object,
                _aiApiClient.Object,
                _historyService.Object,
                settings ?? CreateSettings());
        }

        private static AiSettings CreateSettings()
        {
            return new AiSettings
            {
                MaxContextFaqCount = 5
            };
        }

        private static FaqListItemDto CreateFaq(
            long id,
            string categoryName,
            string question,
            string answer,
            string tagNames)
        {
            return new FaqListItemDto
            {
                Id = id,
                CategoryName = categoryName,
                Question = question,
                Answer = answer,
                TagNames = tagNames
            };
        }
    }
}
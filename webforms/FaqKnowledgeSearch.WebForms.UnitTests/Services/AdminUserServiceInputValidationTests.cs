using System.Threading.Tasks;
using FaqKnowledgeSearch.WebForms.Services;
using Xunit;

namespace FaqKnowledgeSearch.WebForms.UnitTests.Services
{
    [Trait("Category", "Unit")]
    public sealed class AdminUserServiceInputValidationTests
    {
        private readonly AdminUserService _sut;

        public AdminUserServiceInputValidationTests()
        {
            _sut = new AdminUserService();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task SetActiveAsync_ユーザーIdが未指定の場合_Failedを返す(
            string userId)
        {
            // Act
            var result =
                await _sut.SetActiveAsync(
                    userId,
                    true);

            // Assert
            Assert.False(result.Succeeded);

            Assert.Contains(
                "ユーザーIDが指定されていません。",
                result.Errors);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("\t")]
        public async Task SetActiveAsync_無効化要求でもユーザーIdが未指定の場合_Failedを返す(
            string userId)
        {
            // Act
            var result =
                await _sut.SetActiveAsync(
                    userId,
                    false);

            // Assert
            Assert.False(result.Succeeded);

            Assert.Contains(
                "ユーザーIDが指定されていません。",
                result.Errors);
        }
    }
}
using FaqKnowledgeSearch.Infrastructure.Identity;
using FaqKnowledgeSearch.Razor.Pages.Account;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

namespace FaqKnowledgeSearch.Razor.Tests.Pages.Account;

public sealed class LogoutModelTests
{
    [Fact]
    public async Task OnPostAsync_SignsOutAndRedirectsToIndex()
    {
        // Arrange
        var userManager = CreateUserManager();
        var signInManager = CreateSignInManager(userManager);

        signInManager
            .Setup(x => x.SignOutAsync())
            .Returns(Task.CompletedTask);

        var sut = new LogoutModel(signInManager.Object);

        // Act
        var result = await sut.OnPostAsync();

        // Assert
        var redirectResult =
            Assert.IsType<RedirectToPageResult>(result);

        Assert.Equal("/Index", redirectResult.PageName);

        signInManager.Verify(
            x => x.SignOutAsync(),
            Times.Once);
    }

    private static Mock<UserManager<ApplicationUser>>
        CreateUserManager()
    {
        var userStore =
            new Mock<IUserStore<ApplicationUser>>();

        return new Mock<UserManager<ApplicationUser>>(
            userStore.Object,
            Options.Create(new IdentityOptions()),
            new PasswordHasher<ApplicationUser>(),
            Array.Empty<IUserValidator<ApplicationUser>>(),
            Array.Empty<IPasswordValidator<ApplicationUser>>(),
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            null!,
            NullLogger<UserManager<ApplicationUser>>.Instance);
    }

    private static Mock<SignInManager<ApplicationUser>>
        CreateSignInManager(
            Mock<UserManager<ApplicationUser>> userManager)
    {
        return new Mock<SignInManager<ApplicationUser>>(
            userManager.Object,
            new Mock<IHttpContextAccessor>().Object,
            new Mock<
                IUserClaimsPrincipalFactory<ApplicationUser>>().Object,
            Options.Create(new IdentityOptions()),
            NullLogger<SignInManager<ApplicationUser>>.Instance,
            new Mock<IAuthenticationSchemeProvider>().Object,
            new Mock<IUserConfirmation<ApplicationUser>>().Object);
    }
}
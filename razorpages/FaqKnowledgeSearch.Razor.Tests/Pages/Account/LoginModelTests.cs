using FaqKnowledgeSearch.Infrastructure.Identity;
using FaqKnowledgeSearch.Razor.Pages.Account;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

namespace FaqKnowledgeSearch.Razor.Tests.Pages.Account;

public sealed class LoginModelTests
{
    [Fact]
    public async Task OnPostAsync_WhenUserDoesNotExist_ReturnsGenericError()
    {
        // Arrange
        var userManager = CreateUserManager();

        userManager
            .Setup(x => x.FindByEmailAsync("admin@example.com"))
            .ReturnsAsync((ApplicationUser?)null);

        var signInManager =
            CreateSignInManager(userManager);

        var sut = new LoginModel(
            userManager.Object,
            signInManager.Object)
        {
            Input = new LoginModel.InputModel
            {
                Email = " admin@example.com ",
                Password = "Password123!"
            }
        };

        // Act
        var result = await sut.OnPostAsync();

        // Assert
        Assert.IsType<PageResult>(result);

        var errors = sut.ModelState[string.Empty]!
            .Errors
            .Select(x => x.ErrorMessage);

        Assert.Contains(
            "メールアドレスまたはパスワードが正しくありません。",
            errors);

        userManager.Verify(
            x => x.FindByEmailAsync("admin@example.com"),
            Times.Once);

        signInManager.Verify(
            x => x.PasswordSignInAsync(
                It.IsAny<ApplicationUser>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<bool>()),
            Times.Never);
    }

    private static Mock<UserManager<ApplicationUser>>
        CreateUserManager()
    {
        var store =
            new Mock<IUserStore<ApplicationUser>>();

        return new Mock<UserManager<ApplicationUser>>(
            store.Object,
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
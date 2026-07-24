using FaqKnowledgeSearch.Application.Users.Admin;
using FaqKnowledgeSearch.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MockQueryable;
using MockQueryable.Moq;
using Moq;
using Xunit;

namespace FaqKnowledgeSearch.Infrastructure.Tests.Identity;

public sealed class AdminUserServiceTests
{
    private const string AdminRoleName = "Admin";

    // =========================================================
    // SearchAsync
    // =========================================================

    [Theory]
    [InlineData(0, 0, 1, 1)]
    [InlineData(-1, -10, 1, 1)]
    [InlineData(1, 50, 1, 50)]
    [InlineData(2, 51, 2, 50)]
    [InlineData(3, 100, 3, 50)]
    public async Task SearchAsync_PageValuesAreOutOfRange_NormalizesValues(
        int pageNumber,
        int pageSize,
        int expectedPageNumber,
        int expectedPageSize)
    {
        // Arrange
        var userManagerMock =
            CreateUserManagerMock();

        var users =
            Array.Empty<ApplicationUser>()
                .BuildMock();

        userManagerMock
            .SetupGet(manager => manager.Users)
            .Returns(users);

        var sut =
            new AdminUserService(
                userManagerMock.Object);

        // Act
        var result =
            await sut.SearchAsync(
                pageNumber,
                pageSize);

        // Assert
        Assert.Equal(
            expectedPageNumber,
            result.Page);

        Assert.Equal(
            expectedPageSize,
            result.PageSize);

        Assert.Equal(0, result.TotalCount);
        Assert.Empty(result.Items);

        userManagerMock.VerifyGet(
            manager => manager.Users,
            Times.Once);

        userManagerMock.Verify(
            manager => manager.GetRolesAsync(
                It.IsAny<ApplicationUser>()),
            Times.Never);
    }

    [Fact]
    public async Task SearchAsync_UsersExist_OrdersByCreatedAtDescendingThenEmailAscending()
    {
        // Arrange
        var sameCreatedAt =
            new DateTime(
                2026,
                7,
                20,
                0,
                0,
                0,
                DateTimeKind.Utc);

        var users = new[]
        {
            CreateUser(
                id: "user-old",
                userName: "old-user",
                email: "old@example.com",
                isActive: true,
                createdAt:
                    new DateTime(
                        2026,
                        7,
                        10,
                        0,
                        0,
                        0,
                        DateTimeKind.Utc)),

            CreateUser(
                id: "user-b",
                userName: "user-b",
                email: "b@example.com",
                isActive: true,
                createdAt: sameCreatedAt),

            CreateUser(
                id: "user-a",
                userName: "user-a",
                email: "a@example.com",
                isActive: true,
                createdAt: sameCreatedAt)
        };

        var userManagerMock =
            CreateUserManagerMock();

        userManagerMock
            .SetupGet(manager => manager.Users)
            .Returns(users.BuildMock());

        userManagerMock
            .Setup(manager => manager.GetRolesAsync(
                It.IsAny<ApplicationUser>()))
            .ReturnsAsync(
                new List<string>());

        var sut =
            new AdminUserService(
                userManagerMock.Object);

        // Act
        var result =
            await sut.SearchAsync(
                pageNumber: 1,
                pageSize: 10);

        // Assert
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(3, result.Items.Count);

        Assert.Equal(
            "user-a",
            GetPropertyValue<string>(
                result.Items[0],
                "Id",
                "UserId"));

        Assert.Equal(
            "user-b",
            GetPropertyValue<string>(
                result.Items[1],
                "Id",
                "UserId"));

        Assert.Equal(
            "user-old",
            GetPropertyValue<string>(
                result.Items[2],
                "Id",
                "UserId"));

        userManagerMock.Verify(
            manager => manager.GetRolesAsync(
                It.IsAny<ApplicationUser>()),
            Times.Exactly(3));
    }

    [Fact]
    public async Task SearchAsync_SecondPage_ReturnsCorrectUsersAndTotalCount()
    {
        // Arrange
        var baseDate =
            new DateTime(
                2026,
                7,
                1,
                0,
                0,
                0,
                DateTimeKind.Utc);

        var users =
            Enumerable.Range(1, 5)
                .Select(index =>
                    CreateUser(
                        id: $"user-{index}",
                        userName: $"user-{index}",
                        email: $"user-{index}@example.com",
                        isActive: true,
                        createdAt:
                            baseDate.AddDays(index)))
                .ToArray();

        var userManagerMock =
            CreateUserManagerMock();

        userManagerMock
            .SetupGet(manager => manager.Users)
            .Returns(users.BuildMock());

        userManagerMock
            .Setup(manager => manager.GetRolesAsync(
                It.IsAny<ApplicationUser>()))
            .ReturnsAsync(
                new List<string>());

        var sut =
            new AdminUserService(
                userManagerMock.Object);

        // Act
        var result =
            await sut.SearchAsync(
                pageNumber: 2,
                pageSize: 2);

        // Assert
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(2, result.Page);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(2, result.Items.Count);

        // CreatedAt降順：
        // user-5, user-4, user-3, user-2, user-1
        // 2ページ目は user-3, user-2
        Assert.Equal(
            "user-3",
            GetPropertyValue<string>(
                result.Items[0],
                "Id",
                "UserId"));

        Assert.Equal(
            "user-2",
            GetPropertyValue<string>(
                result.Items[1],
                "Id",
                "UserId"));

        // ページに含まれる2ユーザーだけロールを取得する
        userManagerMock.Verify(
            manager => manager.GetRolesAsync(
                It.IsAny<ApplicationUser>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task SearchAsync_UserHasAdminRole_ReturnsAdminInformation()
    {
        // Arrange
        var user =
            CreateUser(
                id: "admin-user",
                userName: "admin-name",
                email: "admin@example.com",
                isActive: true,
                createdAt: DateTime.UtcNow);

        var userManagerMock =
            CreateUserManagerMock();

        userManagerMock
            .SetupGet(manager => manager.Users)
            .Returns(
                new[] { user }.BuildMock());

        // 大文字・小文字を無視する実装を確認する
        userManagerMock
            .Setup(manager => manager.GetRolesAsync(user))
            .ReturnsAsync(
                new List<string>
                {
                    "admin"
                });

        var sut =
            new AdminUserService(
                userManagerMock.Object);

        // Act
        var result =
            await sut.SearchAsync(
                pageNumber: 1,
                pageSize: 10);

        // Assert
        var item =
            Assert.Single(result.Items);

        Assert.Equal(
            AdminRoleName,
            GetPropertyValue<string>(
                item,
                "Role",
                "RoleName"));

        Assert.True(
            GetPropertyValue<bool>(
                item,
                "IsAdmin"));

        Assert.True(
            GetPropertyValue<bool>(
                item,
                "IsActive"));

        Assert.Equal(
            "admin-name",
            GetPropertyValue<string>(
                item,
                "DisplayName",
                "UserName",
                "Name"));

        Assert.Equal(
            "admin@example.com",
            GetPropertyValue<string>(
                item,
                "Email"));
    }

    [Fact]
    public async Task SearchAsync_UserDoesNotHaveAdminRole_ReturnsUserRole()
    {
        // Arrange
        var user =
            CreateUser(
                id: "normal-user",
                userName: "normal-name",
                email: "normal@example.com",
                isActive: false,
                createdAt: DateTime.UtcNow);

        var userManagerMock =
            CreateUserManagerMock();

        userManagerMock
            .SetupGet(manager => manager.Users)
            .Returns(
                new[] { user }.BuildMock());

        userManagerMock
            .Setup(manager => manager.GetRolesAsync(user))
            .ReturnsAsync(
                new List<string>
                {
                    "User"
                });

        var sut =
            new AdminUserService(
                userManagerMock.Object);

        // Act
        var result =
            await sut.SearchAsync(
                pageNumber: 1,
                pageSize: 10);

        // Assert
        var item =
            Assert.Single(result.Items);

        Assert.Equal(
            "User",
            GetPropertyValue<string>(
                item,
                "Role",
                "RoleName"));

        Assert.False(
            GetPropertyValue<bool>(
                item,
                "IsAdmin"));

        Assert.False(
            GetPropertyValue<bool>(
                item,
                "IsActive"));
    }

    [Fact]
    public async Task SearchAsync_UserNameIsNull_UsesEmailAsDisplayName()
    {
        // Arrange
        var user =
            CreateUser(
                id: "user-1",
                userName: null,
                email: "fallback@example.com",
                isActive: true,
                createdAt: DateTime.UtcNow);

        var userManagerMock =
            CreateUserManagerMock();

        userManagerMock
            .SetupGet(manager => manager.Users)
            .Returns(
                new[] { user }.BuildMock());

        userManagerMock
            .Setup(manager => manager.GetRolesAsync(user))
            .ReturnsAsync(
                new List<string>());

        var sut =
            new AdminUserService(
                userManagerMock.Object);

        // Act
        var result =
            await sut.SearchAsync(
                pageNumber: 1,
                pageSize: 10);

        // Assert
        var item =
            Assert.Single(result.Items);

        Assert.Equal(
            "fallback@example.com",
            GetPropertyValue<string>(
                item,
                "DisplayName",
                "UserName",
                "Name"));

        Assert.Equal(
            "fallback@example.com",
            GetPropertyValue<string>(
                item,
                "Email"));
    }

    [Fact]
    public async Task SearchAsync_UserNameAndEmailAreNull_UsesDefaultLabels()
    {
        // Arrange
        var user =
            CreateUser(
                id: "user-1",
                userName: null,
                email: null,
                isActive: true,
                createdAt: DateTime.UtcNow);

        var userManagerMock =
            CreateUserManagerMock();

        userManagerMock
            .SetupGet(manager => manager.Users)
            .Returns(
                new[] { user }.BuildMock());

        userManagerMock
            .Setup(manager => manager.GetRolesAsync(user))
            .ReturnsAsync(
                new List<string>());

        var sut =
            new AdminUserService(
                userManagerMock.Object);

        // Act
        var result =
            await sut.SearchAsync(
                pageNumber: 1,
                pageSize: 10);

        // Assert
        var item =
            Assert.Single(result.Items);

        Assert.Equal(
            "名称未設定",
            GetPropertyValue<string>(
                item,
                "DisplayName",
                "UserName",
                "Name"));

        Assert.Equal(
            "メール未設定",
            GetPropertyValue<string>(
                item,
                "Email"));
    }

    // =========================================================
    // SetActiveAsync
    // =========================================================

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    public async Task SetActiveAsync_UserIdIsNullOrWhiteSpace_ReturnsNotFound(
        string? userId)
    {
        // Arrange
        var userManagerMock =
            CreateUserManagerMock();

        var sut =
            new AdminUserService(
                userManagerMock.Object);

        // Act
        var result =
            await sut.SetActiveAsync(
                userId!,
                isActive: false,
                currentUserId: "current-user");

        // Assert
        Assert.Equal(
            AdminUserStatusChangeResult.NotFound,
            result);

        userManagerMock.Verify(
            manager => manager.FindByIdAsync(
                It.IsAny<string>()),
            Times.Never);

        userManagerMock.Verify(
            manager => manager.UpdateAsync(
                It.IsAny<ApplicationUser>()),
            Times.Never);
    }

    [Fact]
    public async Task SetActiveAsync_UserDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        const string userId =
            "missing-user";

        var userManagerMock =
            CreateUserManagerMock();

        userManagerMock
            .Setup(manager => manager.FindByIdAsync(userId))
            .ReturnsAsync((ApplicationUser?)null);

        var sut =
            new AdminUserService(
                userManagerMock.Object);

        // Act
        var result =
            await sut.SetActiveAsync(
                userId,
                isActive: false,
                currentUserId: "current-user");

        // Assert
        Assert.Equal(
            AdminUserStatusChangeResult.NotFound,
            result);

        userManagerMock.Verify(
            manager => manager.IsInRoleAsync(
                It.IsAny<ApplicationUser>(),
                It.IsAny<string>()),
            Times.Never);

        userManagerMock.Verify(
            manager => manager.UpdateAsync(
                It.IsAny<ApplicationUser>()),
            Times.Never);
    }

    [Fact]
    public async Task SetActiveAsync_TargetIsCurrentUser_ReturnsCannotChangeCurrentUser()
    {
        // Arrange
        const string userId =
            "current-user";

        var user =
            CreateUser(
                id: userId,
                userName: "current",
                email: "current@example.com",
                isActive: true,
                createdAt: DateTime.UtcNow);

        var userManagerMock =
            CreateUserManagerMock();

        userManagerMock
            .Setup(manager => manager.FindByIdAsync(userId))
            .ReturnsAsync(user);

        var sut =
            new AdminUserService(
                userManagerMock.Object);

        // Act
        var result =
            await sut.SetActiveAsync(
                userId,
                isActive: false,
                currentUserId: userId);

        // Assert
        Assert.Equal(
            AdminUserStatusChangeResult.CannotChangeCurrentUser,
            result);

        Assert.True(user.IsActive);

        userManagerMock.Verify(
            manager => manager.IsInRoleAsync(
                It.IsAny<ApplicationUser>(),
                It.IsAny<string>()),
            Times.Never);

        userManagerMock.Verify(
            manager => manager.UpdateAsync(
                It.IsAny<ApplicationUser>()),
            Times.Never);
    }

    [Fact]
    public async Task SetActiveAsync_TargetIsAdmin_ReturnsCannotChangeAdmin()
    {
        // Arrange
        const string userId =
            "admin-user";

        var user =
            CreateUser(
                id: userId,
                userName: "admin",
                email: "admin@example.com",
                isActive: true,
                createdAt: DateTime.UtcNow);

        var userManagerMock =
            CreateUserManagerMock();

        userManagerMock
            .Setup(manager => manager.FindByIdAsync(userId))
            .ReturnsAsync(user);

        userManagerMock
            .Setup(manager => manager.IsInRoleAsync(
                user,
                AdminRoleName))
            .ReturnsAsync(true);

        var sut =
            new AdminUserService(
                userManagerMock.Object);

        // Act
        var result =
            await sut.SetActiveAsync(
                userId,
                isActive: false,
                currentUserId: "current-user");

        // Assert
        Assert.Equal(
            AdminUserStatusChangeResult.CannotChangeAdmin,
            result);

        Assert.True(user.IsActive);

        userManagerMock.Verify(
            manager => manager.UpdateAsync(
                It.IsAny<ApplicationUser>()),
            Times.Never);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task SetActiveAsync_StatusIsAlreadyRequestedValue_ReturnsSuccessWithoutUpdating(
        bool isActive)
    {
        // Arrange
        const string userId =
            "normal-user";

        var user =
            CreateUser(
                id: userId,
                userName: "normal",
                email: "normal@example.com",
                isActive: isActive,
                createdAt: DateTime.UtcNow);

        var userManagerMock =
            CreateUserManagerMock();

        userManagerMock
            .Setup(manager => manager.FindByIdAsync(userId))
            .ReturnsAsync(user);

        userManagerMock
            .Setup(manager => manager.IsInRoleAsync(
                user,
                AdminRoleName))
            .ReturnsAsync(false);

        var sut =
            new AdminUserService(
                userManagerMock.Object);

        // Act
        var result =
            await sut.SetActiveAsync(
                userId,
                isActive,
                currentUserId: "current-user");

        // Assert
        Assert.Equal(
            AdminUserStatusChangeResult.Success,
            result);

        Assert.Equal(
            isActive,
            user.IsActive);

        userManagerMock.Verify(
            manager => manager.UpdateAsync(
                It.IsAny<ApplicationUser>()),
            Times.Never);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task SetActiveAsync_UpdateSucceeds_ChangesStatusAndReturnsSuccess(
        bool currentStatus,
        bool requestedStatus)
    {
        // Arrange
        const string userId =
            "normal-user";

        var user =
            CreateUser(
                id: userId,
                userName: "normal",
                email: "normal@example.com",
                isActive: currentStatus,
                createdAt: DateTime.UtcNow);

        var userManagerMock =
            CreateUserManagerMock();

        userManagerMock
            .Setup(manager => manager.FindByIdAsync(userId))
            .ReturnsAsync(user);

        userManagerMock
            .Setup(manager => manager.IsInRoleAsync(
                user,
                AdminRoleName))
            .ReturnsAsync(false);

        userManagerMock
            .Setup(manager => manager.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        var sut =
            new AdminUserService(
                userManagerMock.Object);

        // Act
        var result =
            await sut.SetActiveAsync(
                userId,
                requestedStatus,
                currentUserId: "current-user");

        // Assert
        Assert.Equal(
            AdminUserStatusChangeResult.Success,
            result);

        Assert.Equal(
            requestedStatus,
            user.IsActive);

        userManagerMock.Verify(
            manager => manager.UpdateAsync(
                It.Is<ApplicationUser>(
                    updatedUser =>
                        updatedUser.Id == userId &&
                        updatedUser.IsActive ==
                        requestedStatus)),
            Times.Once);
    }

    [Fact]
    public async Task SetActiveAsync_UpdateFails_ReturnsFailed()
    {
        // Arrange
        const string userId =
            "normal-user";

        var user =
            CreateUser(
                id: userId,
                userName: "normal",
                email: "normal@example.com",
                isActive: true,
                createdAt: DateTime.UtcNow);

        var failedIdentityResult =
            IdentityResult.Failed(
                new IdentityError
                {
                    Code = "UpdateFailed",
                    Description =
                        "ユーザー状態の更新に失敗しました。"
                });

        var userManagerMock =
            CreateUserManagerMock();

        userManagerMock
            .Setup(manager => manager.FindByIdAsync(userId))
            .ReturnsAsync(user);

        userManagerMock
            .Setup(manager => manager.IsInRoleAsync(
                user,
                AdminRoleName))
            .ReturnsAsync(false);

        userManagerMock
            .Setup(manager => manager.UpdateAsync(user))
            .ReturnsAsync(failedIdentityResult);

        var sut =
            new AdminUserService(
                userManagerMock.Object);

        // Act
        var result =
            await sut.SetActiveAsync(
                userId,
                isActive: false,
                currentUserId: "current-user");

        // Assert
        Assert.Equal(
            AdminUserStatusChangeResult.Failed,
            result);

        userManagerMock.Verify(
            manager => manager.UpdateAsync(user),
            Times.Once);
    }

    // =========================================================
    // Test helpers
    // =========================================================

    private static Mock<UserManager<ApplicationUser>>
        CreateUserManagerMock()
    {
        var userStoreMock =
            new Mock<IUserStore<ApplicationUser>>();

        var options =
            Options.Create(
                new IdentityOptions());

        var passwordHasherMock =
            new Mock<IPasswordHasher<ApplicationUser>>();

        var userValidators =
            Array.Empty<IUserValidator<ApplicationUser>>();

        var passwordValidators =
            Array.Empty<IPasswordValidator<ApplicationUser>>();

        var keyNormalizer =
            new UpperInvariantLookupNormalizer();

        var errorDescriber =
            new IdentityErrorDescriber();

        var serviceProviderMock =
            new Mock<IServiceProvider>();

        var loggerMock =
            new Mock<
                ILogger<UserManager<ApplicationUser>>>();

        return new Mock<UserManager<ApplicationUser>>(
            userStoreMock.Object,
            options,
            passwordHasherMock.Object,
            userValidators,
            passwordValidators,
            keyNormalizer,
            errorDescriber,
            serviceProviderMock.Object,
            loggerMock.Object);
    }

    private static ApplicationUser CreateUser(
        string id,
        string? userName,
        string? email,
        bool isActive,
        DateTime createdAt)
    {
        return new ApplicationUser
        {
            Id = id,
            UserName = userName,
            Email = email,
            IsActive = isActive,
            CreatedAt = createdAt
        };
    }

    private static T GetPropertyValue<T>(
        object target,
        params string[] candidatePropertyNames)
    {
        var targetType =
            target.GetType();

        foreach (var propertyName
                 in candidatePropertyNames)
        {
            var property =
                targetType.GetProperty(
                    propertyName);

            if (property is null)
            {
                continue;
            }

            var value =
                property.GetValue(target);

            if (value is T typedValue)
            {
                return typedValue;
            }
        }

        throw new InvalidOperationException(
            $"{targetType.Name}に対象プロパティがありません。"
            + $"候補: "
            + $"{string.Join(", ", candidatePropertyNames)}");
    }
}
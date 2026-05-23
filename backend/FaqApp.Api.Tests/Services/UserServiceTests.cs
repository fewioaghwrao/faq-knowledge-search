using System.Linq.Expressions;
using FaqApp.Api.Entities;
using FaqApp.Api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Xunit;

namespace FaqApp.Api.Tests.Services;

public class UserServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;

    public UserServiceTests()
    {
        _userManagerMock = CreateUserManagerMock();
    }

    [Fact]
    public async Task GetUsersAsync_ReturnsUsersOrderedByCreatedAtDescending()
    {
        // Arrange
        var olderUser = new ApplicationUser
        {
            Id = "user-1",
            Email = "user@faq-app.local",
            DisplayName = "一般ユーザー",
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        var newerUser = new ApplicationUser
        {
            Id = "admin-1",
            Email = "admin@faq-app.local",
            DisplayName = "管理者",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var users = new List<ApplicationUser>
        {
            olderUser,
            newerUser
        }.AsQueryable();

        _userManagerMock
            .Setup(x => x.Users)
            .Returns(new TestAsyncEnumerable<ApplicationUser>(users));

        _userManagerMock
            .Setup(x => x.GetRolesAsync(newerUser))
            .ReturnsAsync(new List<string> { "Admin" });

        _userManagerMock
            .Setup(x => x.GetRolesAsync(olderUser))
            .ReturnsAsync(new List<string> { "User" });

        var service = new UserService(_userManagerMock.Object);

        // Act
        var result = await service.GetUsersAsync();

        // Assert
        Assert.Equal(2, result.Count);

        Assert.Equal("admin-1", result[0].Id);
        Assert.Equal("admin@faq-app.local", result[0].Email);
        Assert.Equal("管理者", result[0].DisplayName);
        Assert.Equal("Admin", result[0].Role);
        Assert.True(result[0].IsActive);

        Assert.Equal("user-1", result[1].Id);
        Assert.Equal("User", result[1].Role);

        _userManagerMock.Verify(x => x.GetRolesAsync(newerUser), Times.Once);
        _userManagerMock.Verify(x => x.GetRolesAsync(olderUser), Times.Once);
    }

    [Fact]
    public async Task UpdateRoleAsync_UserNotFound_ReturnsFalse()
    {
        // Arrange
        var userId = "not-found-user";

        _userManagerMock
            .Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync((ApplicationUser?)null);

        var service = new UserService(_userManagerMock.Object);

        // Act
        var result = await service.UpdateRoleAsync(userId, "Admin");

        // Assert
        Assert.False(result);

        _userManagerMock.Verify(x => x.FindByIdAsync(userId), Times.Once);
        _userManagerMock.Verify(
            x => x.GetRolesAsync(It.IsAny<ApplicationUser>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateRoleAsync_UserHasCurrentRole_RemovesAndAddsNewRole()
    {
        // Arrange
        var userId = "user-1";
        var user = new ApplicationUser
        {
            Id = userId,
            Email = "user@faq-app.local",
            DisplayName = "一般ユーザー",
            IsActive = true
        };

        _userManagerMock
            .Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "User" });

        _userManagerMock
            .Setup(x => x.RemoveFromRolesAsync(user, It.Is<IEnumerable<string>>(r => r.Contains("User"))))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock
            .Setup(x => x.AddToRoleAsync(user, "Admin"))
            .ReturnsAsync(IdentityResult.Success);

        var service = new UserService(_userManagerMock.Object);

        // Act
        var result = await service.UpdateRoleAsync(userId, "Admin");

        // Assert
        Assert.True(result);

        _userManagerMock.Verify(x => x.FindByIdAsync(userId), Times.Once);
        _userManagerMock.Verify(x => x.GetRolesAsync(user), Times.Once);
        _userManagerMock.Verify(
            x => x.RemoveFromRolesAsync(user, It.Is<IEnumerable<string>>(r => r.Contains("User"))),
            Times.Once);
        _userManagerMock.Verify(x => x.AddToRoleAsync(user, "Admin"), Times.Once);
    }

    [Fact]
    public async Task UpdateRoleAsync_RemoveCurrentRoleFailed_ReturnsFalse()
    {
        // Arrange
        var userId = "user-1";
        var user = new ApplicationUser
        {
            Id = userId,
            Email = "user@faq-app.local",
            DisplayName = "一般ユーザー",
            IsActive = true
        };

        _userManagerMock
            .Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "User" });

        _userManagerMock
            .Setup(x => x.RemoveFromRolesAsync(user, It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(IdentityResult.Failed());

        var service = new UserService(_userManagerMock.Object);

        // Act
        var result = await service.UpdateRoleAsync(userId, "Admin");

        // Assert
        Assert.False(result);

        _userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task UpdateRoleAsync_UserHasNoCurrentRole_AddsNewRole()
    {
        // Arrange
        var userId = "user-1";
        var user = new ApplicationUser
        {
            Id = userId,
            Email = "user@faq-app.local",
            DisplayName = "一般ユーザー",
            IsActive = true
        };

        _userManagerMock
            .Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string>());

        _userManagerMock
            .Setup(x => x.AddToRoleAsync(user, "Admin"))
            .ReturnsAsync(IdentityResult.Success);

        var service = new UserService(_userManagerMock.Object);

        // Act
        var result = await service.UpdateRoleAsync(userId, "Admin");

        // Assert
        Assert.True(result);

        _userManagerMock.Verify(
            x => x.RemoveFromRolesAsync(It.IsAny<ApplicationUser>(), It.IsAny<IEnumerable<string>>()),
            Times.Never);
        _userManagerMock.Verify(x => x.AddToRoleAsync(user, "Admin"), Times.Once);
    }

    [Fact]
    public async Task UpdateRoleAsync_AddRoleFailed_ReturnsFalse()
    {
        // Arrange
        var userId = "user-1";
        var user = new ApplicationUser
        {
            Id = userId,
            Email = "user@faq-app.local",
            DisplayName = "一般ユーザー",
            IsActive = true
        };

        _userManagerMock
            .Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string>());

        _userManagerMock
            .Setup(x => x.AddToRoleAsync(user, "Admin"))
            .ReturnsAsync(IdentityResult.Failed());

        var service = new UserService(_userManagerMock.Object);

        // Act
        var result = await service.UpdateRoleAsync(userId, "Admin");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task UpdateStatusAsync_UserNotFound_ReturnsFalse()
    {
        // Arrange
        var userId = "not-found-user";

        _userManagerMock
            .Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync((ApplicationUser?)null);

        var service = new UserService(_userManagerMock.Object);

        // Act
        var result = await service.UpdateStatusAsync(userId, false);

        // Assert
        Assert.False(result);

        _userManagerMock.Verify(x => x.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Fact]
    public async Task UpdateStatusAsync_UpdateSuccess_ReturnsTrueAndChangesIsActive()
    {
        // Arrange
        var userId = "user-1";
        var user = new ApplicationUser
        {
            Id = userId,
            Email = "user@faq-app.local",
            DisplayName = "一般ユーザー",
            IsActive = true
        };

        _userManagerMock
            .Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(x => x.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        var service = new UserService(_userManagerMock.Object);

        // Act
        var result = await service.UpdateStatusAsync(userId, false);

        // Assert
        Assert.True(result);
        Assert.False(user.IsActive);

        _userManagerMock.Verify(x => x.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task UpdateStatusAsync_UpdateFailed_ReturnsFalse()
    {
        // Arrange
        var userId = "user-1";
        var user = new ApplicationUser
        {
            Id = userId,
            Email = "user@faq-app.local",
            DisplayName = "一般ユーザー",
            IsActive = true
        };

        _userManagerMock
            .Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(x => x.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Failed());

        var service = new UserService(_userManagerMock.Object);

        // Act
        var result = await service.UpdateStatusAsync(userId, false);

        // Assert
        Assert.False(result);
        Assert.False(user.IsActive);
    }

    private static Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();

        return new Mock<UserManager<ApplicationUser>>(
            store.Object,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null);
    }

    private sealed class TestAsyncEnumerable<T> :
        EnumerableQuery<T>,
        IAsyncEnumerable<T>,
        IQueryable<T>
    {
        public TestAsyncEnumerable(IEnumerable<T> enumerable)
            : base(enumerable)
        {
        }

        public TestAsyncEnumerable(Expression expression)
            : base(expression)
        {
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
        }

        IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
    }

    private sealed class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;

        public TestAsyncEnumerator(IEnumerator<T> inner)
        {
            _inner = inner;
        }

        public T Current => _inner.Current;

        public ValueTask DisposeAsync()
        {
            _inner.Dispose();
            return ValueTask.CompletedTask;
        }

        public ValueTask<bool> MoveNextAsync()
        {
            return new ValueTask<bool>(_inner.MoveNext());
        }
    }

    private sealed class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
    {
        private readonly IQueryProvider _inner;

        public TestAsyncQueryProvider(IQueryProvider inner)
        {
            _inner = inner;
        }

        public IQueryable CreateQuery(Expression expression)
        {
            return new TestAsyncEnumerable<TEntity>(expression);
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new TestAsyncEnumerable<TElement>(expression);
        }

        public object? Execute(Expression expression)
        {
            return _inner.Execute(expression);
        }

        public TResult Execute<TResult>(Expression expression)
        {
            return _inner.Execute<TResult>(expression);
        }

        public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
        {
            var expectedResultType = typeof(TResult).GetGenericArguments()[0];

            var executionResult = typeof(IQueryProvider)
                .GetMethod(
                    name: nameof(IQueryProvider.Execute),
                    genericParameterCount: 1,
                    types: new[] { typeof(Expression) })!
                .MakeGenericMethod(expectedResultType)
                .Invoke(this, new object[] { expression });

            return (TResult)typeof(Task)
                .GetMethod(nameof(Task.FromResult))!
                .MakeGenericMethod(expectedResultType)
                .Invoke(null, new[] { executionResult })!;
        }
    }
}
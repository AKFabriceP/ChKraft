using ChronoKraft.Domain.Enums;
using ChronoKraft.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace ChronoKraft.Tests.Unit;
[Trait("Category", "Unit")]
public class UserAccountServiceTests
{
    private static Mock<UserManager<ApplicationUser>> CreateMockUserManager()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        return new Mock<UserManager<ApplicationUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
    }

    [Fact]
    public async Task CreateUserAccount_When_UserManager_Fails_Then_Returns_Failure()
    {
        // Prepare
        var mockUserManager = CreateMockUserManager();
        var failedResult = IdentityResult.Failed(new IdentityError { Description = "Email already used." });
        mockUserManager.Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(failedResult);
        var service = new UserAccountService(mockUserManager.Object, null!, NullLogger<UserAccountService>.Instance);

        // Act
        var (result, user) = await service.CreateUserAccountAsync("test@chronokraft.local", "pass", UserRole.WORKER);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Null(user);
        mockUserManager.Verify(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task CreateUserAccountAsync_When_UserManager_Succeeds_Then_Assigns_Role()
    {
        var mockUserManager = CreateMockUserManager();
        mockUserManager
            .Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        mockUserManager
            .Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        var service = new UserAccountService(
            mockUserManager.Object, null!, NullLogger<UserAccountService>.Instance);

        var (result, user) = await service.CreateUserAccountAsync("test@chronokraft.local", "pass", UserRole.ADMIN);

        Assert.True(result.Succeeded);
        Assert.NotNull(user);
        mockUserManager.Verify(
            m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), UserRole.ADMIN.ToString()), Times.Once);
    }
}
using ConfigurationManager.Core.Constants;
using ConfigurationManager.Core.Services;
using ConfigurationManager.Db.Models;
using ConfigurationManager.Db.Repositories;
using FluentAssertions;
using Moq;

namespace ConfigurationManager.Tests.Services;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IConfigurationRepository> _configurationRepositoryMock;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _configurationRepositoryMock = new Mock<IConfigurationRepository>();
        _userService = new UserService(
            _userRepositoryMock.Object,
            _configurationRepositoryMock.Object);
    }

    private User GetTestUser(Guid? id = null)
    {
        return new User
        {
            Id = id ?? Guid.NewGuid(),
            Username = "testuser",
            Email = "test@example.com",
            CreatedAt = DateTime.UtcNow
        };
    }

    [Fact]
    public async Task CreateUserAsync_WithValidData_ShouldCreateUserAndDefaultConfig()
    {
        var username = "testuser";
        var email = "test@example.com";
        var createdUser = new User
        {
            Id = Guid.NewGuid(),
            Username = username,
            Email = email,
            CreatedAt = DateTime.UtcNow
        };

        _userRepositoryMock
            .Setup(x => x.ExistsAsync(email))
            .ReturnsAsync(false);

        _userRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync(createdUser);

        _configurationRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Configuration>()))
            .ReturnsAsync(new Configuration());

        var result = await _userService.CreateUserAsync(username, email);

        result.Should().NotBeNull();
        result.Username.Should().Be(username);
        result.Email.Should().Be(email);

        _userRepositoryMock.Verify(x => x.ExistsAsync(email), Times.Once);
        _userRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<User>()), Times.Once);

        _configurationRepositoryMock.Verify(x => x.CreateAsync(It.Is<Configuration>(c =>
            c.Name == DefaultSettings.DefaultConfigurationName &&
            c.UserId == createdUser.Id &&
            c.IsActive == true &&
            c.Versions.Count == 1 &&
            c.Versions.First().VersionNumber == 1 &&
            c.Versions.First().IsActive == true
        )), Times.Once);
    }

    [Fact]
    public async Task CreateUserAsync_WithExistingEmail_ShouldThrowException()
    {
        var username = "testuser";
        var email = "existing@example.com";

        _userRepositoryMock
            .Setup(x => x.ExistsAsync(email))
            .ReturnsAsync(true);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _userService.CreateUserAsync(username, email));

        exception.Message.Should().Be("User with this email already exists");

        _userRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<User>()), Times.Never);
        _configurationRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Configuration>()), Times.Never);
    }

    [Fact]
    public async Task GetUserByIdAsync_WithExistingId_ShouldReturnUser()
    {
        var userId = Guid.NewGuid();
        var expectedUser = GetTestUser(userId);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(expectedUser);

        var result = await _userService.GetUserByIdAsync(userId);

        result.Should().NotBeNull();
        result?.Id.Should().Be(userId);
        result?.Username.Should().Be(expectedUser.Username);
        result?.Email.Should().Be(expectedUser.Email);
    }

    [Fact]
    public async Task GetUserByIdAsync_WithNonExistingId_ShouldReturnNull()
    {
        var nonExistingId = Guid.NewGuid();

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(nonExistingId))
            .ReturnsAsync((User?)null);

        var result = await _userService.GetUserByIdAsync(nonExistingId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllUsersAsync_ShouldReturnAllUsers()
    {
        var users = new List<User>
        {
            GetTestUser(),
            GetTestUser(),
            GetTestUser()
        };

        _userRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(users);

        var result = await _userService.GetAllUsersAsync();

        result.Should().HaveCount(3);
        result.Should().BeEquivalentTo(users);
    }

    [Fact]
    public async Task GetUserByEmailAsync_WithExistingEmail_ShouldReturnUser()
    {
        var email = "test@example.com";
        var expectedUser = GetTestUser();
        expectedUser.Email = email;

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(email))
            .ReturnsAsync(expectedUser);

        var result = await _userService.GetUserByEmailAsync(email);

        result.Should().NotBeNull();
        result?.Email.Should().Be(email);
    }

    [Fact]
    public async Task UpdateUserAsync_WithValidData_ShouldUpdateUser()
    {
        var userId = Guid.NewGuid();
        var existingUser = GetTestUser(userId);
        var newUsername = "newname";
        var newEmail = "new@example.com";

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(existingUser);

        _userRepositoryMock
            .Setup(x => x.ExistsAsync(newEmail))
            .ReturnsAsync(false);

        var updatedUser = new User
        {
            Id = userId,
            Username = newUsername,
            Email = newEmail
        };

        _userRepositoryMock
            .Setup(x => x.UpdateAsync(userId, It.IsAny<User>()))
            .ReturnsAsync(updatedUser);

        var result = await _userService.UpdateUserAsync(userId, newUsername, newEmail);

        result.Should().NotBeNull();
        result?.Username.Should().Be(newUsername);
        result?.Email.Should().Be(newEmail);
    }

    [Fact]
    public async Task UpdateUserAsync_WithExistingEmail_ShouldThrowException()
    {
        var userId = Guid.NewGuid();
        var existingUser = GetTestUser(userId);
        var existingEmail = "taken@example.com";

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(existingUser);

        _userRepositoryMock
            .Setup(x => x.ExistsAsync(existingEmail))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _userService.UpdateUserAsync(userId, null, existingEmail));
    }

    [Fact]
    public async Task UpdateUserAsync_WithNonExistingUser_ShouldReturnNull()
    {
        var nonExistingId = Guid.NewGuid();

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(nonExistingId))
            .ReturnsAsync((User?)null);

        var result = await _userService.UpdateUserAsync(nonExistingId, "name", "email@test.com");

        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteUserAsync_WithExistingId_ShouldReturnTrue()
    {
        var userId = Guid.NewGuid();

        _userRepositoryMock
            .Setup(x => x.DeleteAsync(userId))
            .ReturnsAsync(true);

        var result = await _userService.DeleteUserAsync(userId);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteUserAsync_WithNonExistingId_ShouldReturnFalse()
    {
        var nonExistingId = Guid.NewGuid();

        _userRepositoryMock
            .Setup(x => x.DeleteAsync(nonExistingId))
            .ReturnsAsync(false);

        var result = await _userService.DeleteUserAsync(nonExistingId);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetUserWithConfigurationsAsync_ShouldReturnUserWithConfigs()
    {
        var userId = Guid.NewGuid();
        var user = GetTestUser(userId);
        var configs = new List<Configuration>
        {
            new Configuration
            {
                Id = Guid.NewGuid(),
                Name = "Config1",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Versions = new List<ConfigurationVersion>
                {
                    new ConfigurationVersion { VersionNumber = 1, IsActive = true }
                }
            },
            new Configuration
            {
                Id = Guid.NewGuid(),
                Name = "Config2",
                IsActive = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Versions = new List<ConfigurationVersion>
                {
                    new ConfigurationVersion { VersionNumber = 1, IsActive = false },
                    new ConfigurationVersion { VersionNumber = 2, IsActive = true }
                }
            }
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);

        _configurationRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync(configs);

        var result = await _userService.GetUserWithConfigurationsAsync(userId);

        result.Should().NotBeNull();
        result?.Username.Should().Be(user.Username);
        result?.Configurations.Should().HaveCount(2);

        var configList = result?.Configurations.ToList();
        configList?[0].Name.Should().Be("Config1");
        configList?[0].VersionsCount.Should().Be(1);
        configList?[1].Name.Should().Be("Config2");
        configList?[1].VersionsCount.Should().Be(2);
    }
}
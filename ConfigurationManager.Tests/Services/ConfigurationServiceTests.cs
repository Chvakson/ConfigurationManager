using ConfigurationManager.Core.Infrastructure.Notification;
using ConfigurationManager.Core.Models;
using ConfigurationManager.Core.Services;
using ConfigurationManager.Db.Models;
using ConfigurationManager.Db.Repositories;
using FluentAssertions;
using Moq;

namespace ConfigurationManager.Tests.Services;

public class ConfigurationServiceTests
{
    private readonly Mock<IConfigurationRepository> _configRepoMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<INotificationService> _notificationMock;
    private readonly ConfigurationService _configService;

    public ConfigurationServiceTests()
    {
        _configRepoMock = new Mock<IConfigurationRepository>();
        _userRepoMock = new Mock<IUserRepository>();
        _notificationMock = new Mock<INotificationService>();
        _configService = new ConfigurationService(
            _configRepoMock.Object,
            _userRepoMock.Object,
            _notificationMock.Object);
    }

    private User GetTestUser()
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Email = "test@example.com",
            CreatedAt = DateTime.UtcNow
        };
    }

    private Configuration GetTestConfiguration(Guid userId, int versionsCount = 1)
    {
        var config = new Configuration
        {
            Id = Guid.NewGuid(),
            Name = "Test Config",
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = false,
            Versions = new List<ConfigurationVersion>()
        };

        for (int i = 1; i <= versionsCount; i++)
        {
            config.Versions.Add(new ConfigurationVersion
            {
                Id = Guid.NewGuid(),
                VersionNumber = i,
                SettingsJson = $"{{\"theme\":\"dark{i}\"}}",
                CreatedAt = DateTime.UtcNow.AddMinutes(i),
                IsActive = i == versionsCount
            });
        }

        return config;
    }

    private string GetValidSettingsJson()
    {
        return "{\"theme\":{\"theme\":\"dark\",\"colors\":{\"background\":\"#1e1e1e\"}}}";
    }

    [Fact]
    public async Task CreateConfigurationAsync_WithValidData_ShouldCreateConfiguration()
    {
        var user = GetTestUser();
        var configName = "New Config";
        var settingsJson = GetValidSettingsJson();
        var createdConfig = new Configuration { Id = Guid.NewGuid(), Name = configName };

        _userRepoMock
            .Setup(x => x.GetByIdAsync(user.Id))
            .ReturnsAsync(user);

        _configRepoMock
            .Setup(x => x.GetByNameAndUserIdAsync(configName, user.Id))
            .ReturnsAsync((Configuration?)null);

        _configRepoMock
            .Setup(x => x.CreateAsync(It.IsAny<Configuration>()))
            .ReturnsAsync(createdConfig);

        var result = await _configService.CreateConfigurationAsync(user.Id, configName, settingsJson);

        result.Should().NotBeNull();
        result.Id.Should().Be(createdConfig.Id);

        _configRepoMock.Verify(x => x.CreateAsync(It.Is<Configuration>(c =>
            c.Name == configName &&
            c.UserId == user.Id &&
            !c.IsActive &&
            c.Versions.Count == 1 &&
            c.Versions.First().IsActive == true
        )), Times.Once);

        _notificationMock.Verify(x => x.NotifyConfigurationCreated(createdConfig), Times.Once);
    }

    [Fact]
    public async Task CreateConfigurationAsync_WithNonExistingUser_ShouldThrowException()
    {
        var userId = Guid.NewGuid();
        _userRepoMock
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _configService.CreateConfigurationAsync(userId, "Test", "{}"));
    }

    [Fact]
    public async Task CreateConfigurationAsync_WithDuplicateName_ShouldThrowException()
    {
        var user = GetTestUser();
        var configName = "Duplicate";
        var existingConfig = new Configuration { Id = Guid.NewGuid(), Name = configName };

        _userRepoMock
            .Setup(x => x.GetByIdAsync(user.Id))
            .ReturnsAsync(user);

        _configRepoMock
            .Setup(x => x.GetByNameAndUserIdAsync(configName, user.Id))
            .ReturnsAsync(existingConfig);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _configService.CreateConfigurationAsync(user.Id, configName, "{}"));
    }

    [Fact]
    public async Task UpdateConfigurationAsync_WithValidData_ShouldCreateNewVersion()
    {
        var user = GetTestUser();
        var config = GetTestConfiguration(user.Id, 2);
        var newSettings = GetValidSettingsJson();
        var newVersion = new ConfigurationVersion { Id = Guid.NewGuid(), VersionNumber = 3 };

        _configRepoMock
            .Setup(x => x.GetByIdWithVersionsAsync(config.Id))
            .ReturnsAsync(config);

        _configRepoMock
            .Setup(x => x.AddVersionAsync(config.Id, 3, newSettings))
            .ReturnsAsync(newVersion);

        _configRepoMock
            .Setup(x => x.UpdateVersionsAsync(It.IsAny<IEnumerable<ConfigurationVersion>>()))
            .Returns(Task.CompletedTask);

        _configRepoMock
            .Setup(x => x.UpdateAsync(It.IsAny<Configuration>()))
            .Returns(Task.CompletedTask);

        _configRepoMock
            .Setup(x => x.GetByIdWithVersionsAsync(config.Id))
            .ReturnsAsync(config);

        var result = await _configService.UpdateConfigurationAsync(config.Id, "New Name", newSettings);

        result.Should().NotBeNull();
        result?.Name.Should().Be("New Name");

        _configRepoMock.Verify(x => x.AddVersionAsync(config.Id, 3, newSettings), Times.Once);
        _notificationMock.Verify(x => x.NotifyConfigurationUpdated(It.IsAny<Configuration>()), Times.Once);
    }

    [Fact]
    public async Task UpdateConfigurationAsync_WithNonExistingConfig_ShouldReturnNull()
    {
        var nonExistingId = Guid.NewGuid();
        _configRepoMock
            .Setup(x => x.GetByIdWithVersionsAsync(nonExistingId))
            .ReturnsAsync((Configuration?)null);

        var result = await _configService.UpdateConfigurationAsync(nonExistingId, "name", "{}");

        result.Should().BeNull();
        _notificationMock.Verify(x => x.NotifyConfigurationUpdated(It.IsAny<Configuration>()), Times.Never);
    }

    [Fact]
    public async Task SetActiveConfigurationAsync_ShouldDeactivateOthersAndActivateSelected()
    {
        var userId = Guid.NewGuid();
        var configId = Guid.NewGuid();
        var config = GetTestConfiguration(userId);
        config.Id = configId;

        var otherConfig1 = new Configuration { Id = Guid.NewGuid(), UserId = userId, IsActive = true };
        var otherConfig2 = new Configuration { Id = Guid.NewGuid(), UserId = userId, IsActive = false };

        var userConfigs = new List<Configuration> { otherConfig1, otherConfig2, config };

        _configRepoMock
            .Setup(x => x.GetByIdAsync(configId))
            .ReturnsAsync(config);

        _configRepoMock
            .Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync(userConfigs);

        var result = await _configService.SetActiveConfigurationAsync(userId, configId);

        result.Should().BeTrue();

        _configRepoMock.Verify(x => x.UpdateAsync(It.IsAny<Configuration>()), Times.Exactly(4));

        _notificationMock.Verify(x => x.NotifyConfigurationActivated(config), Times.Once);
    }

    [Fact]
    public async Task DeleteConfigurationAsync_ShouldDeleteAndNotify()
    {
        var configId = Guid.NewGuid();

        _configRepoMock
            .Setup(x => x.DeleteAsync(configId))
            .ReturnsAsync(true);

        var result = await _configService.DeleteConfigurationAsync(configId);

        result.Should().BeTrue();
        _notificationMock.Verify(x => x.NotifyConfigurationDeleted(configId), Times.Once);
    }

    [Fact]
    public async Task RollbackToVersionAsync_WithValidVersion_ShouldActivateTargetVersion()
    {
        var user = GetTestUser();
        var config = GetTestConfiguration(user.Id, 3);

        var versionsToUpdate = new List<ConfigurationVersion>();

        _configRepoMock
            .Setup(x => x.GetByIdWithVersionsAsync(config.Id))
            .ReturnsAsync(config);

        _configRepoMock
            .Setup(x => x.UpdateVersionsAsync(It.IsAny<IEnumerable<ConfigurationVersion>>()))
            .Callback<IEnumerable<ConfigurationVersion>>(v => versionsToUpdate = v.ToList())
            .Returns(Task.CompletedTask);

        var result = await _configService.RollbackToVersionAsync(config.Id, 1);

        result.Should().NotBeNull();

        versionsToUpdate.Should().HaveCount(2);
        versionsToUpdate.Should().Contain(v => v.VersionNumber == 1 && v.IsActive);
        versionsToUpdate.Should().Contain(v => v.VersionNumber == 3 && !v.IsActive);

        _notificationMock.Verify(x => x.NotifyConfigurationUpdated(config), Times.Once);
    }

    [Fact]
    public async Task GetUserConfigurationsAsync_ShouldReturnUserConfigs()
    {
        var userId = Guid.NewGuid();
        var configs = new List<Configuration>
        {
            GetTestConfiguration(userId),
            GetTestConfiguration(userId)
        };

        _configRepoMock
            .Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync(configs);

        var result = await _configService.GetUserConfigurationsAsync(userId);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllConfigurationsAsync_ShouldReturnAllWithCurrentVersion()
    {
        var userId = Guid.NewGuid();
        var configs = new List<Configuration>
        {
            GetTestConfiguration(userId, 2),
            GetTestConfiguration(userId, 3)
        };

        _configRepoMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(configs);

        var result = await _configService.GetAllConfigurationsAsync();

        result.Should().HaveCount(2);
        result.First().CurrentVersion.Should().NotBeNull();
        result.First().VersionsCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetSortedConfigurationsAsync_ShouldReturnSortedConfigs()
    {
        var userId = Guid.NewGuid();
        var configs = new List<Configuration>
        {
            new Configuration { Id = Guid.NewGuid(), Name = "B", CreatedAt = DateTime.UtcNow.AddDays(-1) },
            new Configuration { Id = Guid.NewGuid(), Name = "A", CreatedAt = DateTime.UtcNow },
            new Configuration { Id = Guid.NewGuid(), Name = "C", CreatedAt = DateTime.UtcNow.AddDays(-2) }
        };

        _configRepoMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(configs);

        var nameAscResult = await _configService.GetSortedConfigurationsAsync(ConfigurationSort.NameAsc);
        var dateDescResult = await _configService.GetSortedConfigurationsAsync(ConfigurationSort.DateDesc);

        nameAscResult.First().Name.Should().Be("A");
        dateDescResult.First().CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }
}
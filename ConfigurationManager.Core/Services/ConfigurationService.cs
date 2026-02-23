using ConfigurationManager.Core.Infrastructure.Notification;
using ConfigurationManager.Core.Models;
using ConfigurationManager.Core.Models.Dto;
using ConfigurationManager.Core.Models.Dto.Mappings;
using ConfigurationManager.Db.Models;
using ConfigurationManager.Db.Repositories;
using ConfigurationManager.Db.Validators;

namespace ConfigurationManager.Core.Services;

public class ConfigurationService : IConfigurationService
{
    private readonly IConfigurationRepository _configurationRepository;
    private readonly IUserRepository _userRepository;
    private readonly INotificationService _notificationService;

    public ConfigurationService(
        IConfigurationRepository configurationRepository,
        IUserRepository userRepository,
        INotificationService notificationService)
    {
        _configurationRepository = configurationRepository;
        _userRepository = userRepository;
        _notificationService = notificationService;
    }

    private async Task<ConfigurationVersion> CreateNewVersionAsync(
        Configuration config,
        string settingsJson,
        bool makeActive = true)
    {
        var lastVersion = config.Versions.Max(v => v.VersionNumber);
        var newVersionNumber = lastVersion + 1;

        if (makeActive)
        {
            await DeactivateAllVersionsAsync(config);
        }

        var newVersion = await _configurationRepository.AddVersionAsync(
            config.Id,
            newVersionNumber,
            settingsJson);

        config.UpdatedAt = DateTime.UtcNow;
        await _configurationRepository.UpdateAsync(config);

        return newVersion;
    }

    public async Task<Configuration> CreateConfigurationAsync(
        Guid userId,
        string name,
        string settingsJson)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new InvalidOperationException("User not found");

        var (nameIsValid, nameError) = await ConfigurationValidator.ValidateConfigurationNameAsync(
            _configurationRepository, name, userId);
        if (!nameIsValid) throw new InvalidOperationException(nameError);

        var (isValid, errors) = ConfigurationValidator.ValidateSettingsJson(settingsJson);

        if (!isValid)
            throw new InvalidOperationException($"Invalid settings: {string.Join("; ", errors)}");

        var configuration = new Configuration
        {
            Name = name,
            UserId = userId,
            IsActive = false,
            Versions = new List<ConfigurationVersion>
            {
                new ConfigurationVersion
                {
                    SettingsJson = settingsJson,
                    IsActive = true
                }
            }
        };

        var created = await _configurationRepository.CreateAsync(configuration);

        await _notificationService.NotifyConfigurationCreated(created);

        return created;
    }

    public async Task<Configuration?> UpdateConfigurationAsync(Guid id, string name, string settingsJson)
    {
        var config = await _configurationRepository.GetByIdWithVersionsAsync(id);
        if (config == null) return null;

        var (isValid, errors) = ConfigurationValidator.ValidateSettingsJson(settingsJson);
        if (!isValid) throw new InvalidOperationException($"Invalid settings: {string.Join("; ", errors)}");

        if (!string.IsNullOrWhiteSpace(name) && name != config.Name)
        {
            var (nameIsValid, nameError) = await ConfigurationValidator.ValidateConfigurationNameAsync(
                _configurationRepository,
                name,
                config.UserId,
                config.Id);

            if (!nameIsValid)
            {
                throw new InvalidOperationException(nameError);
            }
            config.Name = name;
        }

        await CreateNewVersionAsync(config, settingsJson);

        var updatedConfig = await _configurationRepository.GetByIdWithVersionsAsync(id);
        await _notificationService.NotifyConfigurationUpdated(updatedConfig!);

        return updatedConfig;
    }

    public async Task<bool> SetActiveConfigurationAsync(Guid userId, Guid configurationId)
    {
        var config = await _configurationRepository.GetByIdAsync(configurationId);
        if (config == null || config.UserId != userId) return false;

        var userConfigs = await _configurationRepository.GetByUserIdAsync(userId);
        foreach (var c in userConfigs)
        {
            c.IsActive = false;
            await _configurationRepository.UpdateAsync(c);
        }

        config.IsActive = true;
        await _configurationRepository.UpdateAsync(config);

        await _notificationService.NotifyConfigurationActivated(config);

        return true;
    }

    public async Task<bool> DeleteConfigurationAsync(Guid id)
    {
        var result = await _configurationRepository.DeleteAsync(id);

        if (result)
        {
            await _notificationService.NotifyConfigurationDeleted(id);
        }

        return result;
    }

    private async Task<List<ConfigurationVersion>> DeactivateAllVersionsAsync(Configuration config)
    {
        var versionsToUpdate = new List<ConfigurationVersion>();
        foreach (var version in config.Versions)
        {
            if (version.IsActive)
            {
                version.IsActive = false;
                versionsToUpdate.Add(version);
            }
        }

        if (versionsToUpdate.Any())
        {
            await _configurationRepository.UpdateVersionsAsync(versionsToUpdate);
        }

        return versionsToUpdate;
    }

    public async Task<Configuration?> RollbackToVersionAsync(Guid configurationId, int versionNumber)
    {
        var config = await _configurationRepository.GetByIdWithVersionsAsync(configurationId);
        if (config == null) return null;

        var targetVersion = config.Versions.FirstOrDefault(v => v.VersionNumber == versionNumber);
        if (targetVersion == null) return null;

        var versionsToUpdate = new List<ConfigurationVersion>();

        foreach (var version in config.Versions)
        {
            if (version.IsActive != (version.Id == targetVersion.Id))
            {
                version.IsActive = (version.Id == targetVersion.Id);
                versionsToUpdate.Add(version);
            }
        }

        if (versionsToUpdate.Any())
        {
            await _configurationRepository.UpdateVersionsAsync(versionsToUpdate);
        }

        config.UpdatedAt = DateTime.UtcNow;
        await _configurationRepository.UpdateAsync(config);

        await _notificationService.NotifyConfigurationUpdated(config);

        return config;
    }

    public async Task<IEnumerable<ConfigurationResponse>> GetAllConfigurationsAsync()
    {
        var configs = await _configurationRepository.GetAllAsync();
        return configs.Select(c => new ConfigurationResponse
        {
            Id = c.Id,
            Name = c.Name,
            UserId = c.UserId,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt,
            IsActive = c.IsActive,
            VersionsCount = c.Versions?.Count ?? 0,
            CurrentVersion = c.Versions?
            .Where(v => v.IsActive)
            .Select(v => new ConfigurationVersionDto
            {
                VersionNumber = v.VersionNumber,
                SettingsJson = v.SettingsJson,
                CreatedAt = v.CreatedAt
            }).FirstOrDefault()
        });
    }

    public async Task<IEnumerable<ConfigurationResponse>> GetSortedConfigurationsAsync(ConfigurationSort sort)
    {
        var configs = await _configurationRepository.GetAllAsync();

        var sorted = sort switch
        {
            ConfigurationSort.DateDesc => configs.OrderByDescending(c => c.CreatedAt),
            ConfigurationSort.NameAsc => configs.OrderBy(c => c.Name),
            ConfigurationSort.NameDesc => configs.OrderByDescending(c => c.Name),
            _ => configs.OrderBy(c => c.CreatedAt)
        };

        return sorted.Select(c => c.ToResponse());
    }

    public async Task<IEnumerable<Configuration>> GetUserConfigurationsAsync(Guid userId)
    {
        return await _configurationRepository.GetByUserIdAsync(userId);
    }

    public async Task<Configuration?> GetConfigurationByIdAsync(Guid id)
    {
        return await _configurationRepository.GetByIdAsync(id);
    }

    public async Task<Configuration?> GetConfigurationWithVersionsAsync(Guid id)
    {
        return await _configurationRepository.GetByIdWithVersionsAsync(id);
    }
}
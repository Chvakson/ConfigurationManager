using ConfigurationManager.Core.Models;
using ConfigurationManager.Core.Models.Dto;
using ConfigurationManager.Db.Models;

namespace ConfigurationManager.Core.Services;

public interface IConfigurationService
{
    Task<IEnumerable<Configuration>> GetUserConfigurationsAsync(Guid userId);
    Task<Configuration?> GetConfigurationByIdAsync(Guid id);
    Task<Configuration?> GetConfigurationWithVersionsAsync(Guid id);
    Task<IEnumerable<ConfigurationResponse>> GetAllConfigurationsAsync();
    Task<IEnumerable<ConfigurationResponse>> GetSortedConfigurationsAsync(ConfigurationSort sort);

    Task<Configuration> CreateConfigurationAsync(
        Guid userId,
        string name,
        string settingsJson);

    Task<Configuration?> UpdateConfigurationAsync(
        Guid id,
        string name,
        string settingsJson);

    Task<Configuration?> RollbackToVersionAsync(
        Guid configurationId,
        int versionNumber);

    Task<bool> SetActiveConfigurationAsync(
        Guid userId,
        Guid configurationId);

    Task<bool> DeleteConfigurationAsync(Guid id);
}
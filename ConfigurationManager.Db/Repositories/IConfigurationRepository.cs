using ConfigurationManager.Db.Models;

namespace ConfigurationManager.Db.Repositories;

public interface IConfigurationRepository
{
    Task<IEnumerable<Configuration>> GetAllAsync();
    Task<IEnumerable<Configuration>> GetByNameFilterAsync(string name);
    Task<IEnumerable<Configuration>> GetByDateFilterAsync(DateTime date);
    Task<Configuration?> GetByIdAsync(Guid id);
    Task<IEnumerable<Configuration>> GetByUserIdAsync(Guid userId);
    Task<Configuration?> GetByIdWithVersionsAsync(Guid id);
    Task<Configuration?> GetByNameAndUserIdAsync(string name, Guid userId);

    Task<bool> ExistsAsync(string name, Guid userId);
    Task<ConfigurationVersion> AddVersionAsync(Guid configurationId, int versionNumber, string settingsJson);

    Task<Configuration> CreateAsync(Configuration configuration);
    Task UpdateAsync(Configuration configuration);
    Task UpdateVersionsAsync(IEnumerable<ConfigurationVersion> versions);

    Task<bool> DeleteAsync(Guid id);
}
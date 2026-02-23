using ConfigurationManager.Db.Models;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;

namespace ConfigurationManager.Db.Repositories;

public class ConfigurationRepository : IConfigurationRepository
{
    private readonly AppDbContext _context;

    public ConfigurationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Configuration>> GetAllAsync()
    {
        return await _context.Configurations
            .Include(c => c.Versions)
            .OrderByDescending(c => c.UpdatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Configuration>> GetByNameFilterAsync(string name)
    {
        return await _context.Configurations
            .Include(c => c.Versions)
            .Where(c => c.Name.Contains(name))
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Configuration>> GetByDateFilterAsync(DateTime date)
    {
        return await _context.Configurations
            .Include(c => c.Versions)
            .Where(c => c.CreatedAt.Date == date.Date)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<Configuration?> GetByIdAsync(Guid id)
    {
        return await _context.Configurations
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<Configuration>> GetByUserIdAsync(Guid userId)
    {
        return await _context.Configurations
            .Where(c => c.UserId == userId)
            .Include(c => c.Versions)
            .OrderByDescending(c => c.UpdatedAt)
            .ToListAsync();
    }

    public async Task<Configuration?> GetByIdWithVersionsAsync(Guid id)
    {
        return await _context.Configurations
            .Include(c => c.Versions.OrderByDescending(v => v.VersionNumber))
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<bool> ExistsAsync(string name, Guid userId)
    {
        return await _context.Configurations
            .AnyAsync(c => c.Name == name && c.UserId == userId);
    }

    public async Task<Configuration> CreateAsync(Configuration configuration)
    {
        configuration.Id = Guid.NewGuid();
        configuration.CreatedAt = DateTime.UtcNow;
        configuration.UpdatedAt = DateTime.UtcNow;

        var versionNumber = 1;

        foreach (var version in configuration.Versions)
        {
            version.Id = Guid.NewGuid();
            version.VersionNumber = versionNumber++;
            version.CreatedAt = DateTime.UtcNow;
        }

        _context.Configurations.Add(configuration);
        await _context.SaveChangesAsync();

        return configuration;
    }

    public async Task<Configuration?> GetByNameAndUserIdAsync(string name, Guid userId)
    {
        return await _context.Configurations
            .FirstOrDefaultAsync(c => c.Name == name && c.UserId == userId);
    }


    public async Task<ConfigurationVersion> AddVersionAsync(Guid configurationId, int versionNumber, string settingsJson)
    {
        var newVersion = new ConfigurationVersion
        {
            Id = Guid.NewGuid(),
            ConfigurationId = configurationId,
            VersionNumber = versionNumber,
            SettingsJson = settingsJson,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.ConfigurationVersions.Add(newVersion);
        await _context.SaveChangesAsync();
        return newVersion;
    }

    public async Task UpdateAsync(Configuration configuration)
    {
        var existingConfig = await _context.Configurations
            .FindAsync(configuration.Id);

        if (existingConfig == null)
            throw new InvalidOperationException($"Configuration with id {configuration.Id} not found");

        existingConfig.Name = configuration.Name;
        existingConfig.UpdatedAt = configuration.UpdatedAt;
        existingConfig.IsActive = configuration.IsActive;

        await _context.SaveChangesAsync();
    }

    public async Task UpdateVersionsAsync(IEnumerable<ConfigurationVersion> versions)
    {
        foreach (var version in versions)
        {
            _context.Entry(version).State = EntityState.Modified;
        }
        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var configuration = await _context.Configurations.FindAsync(id);
        if (configuration == null) return false;

        _context.Configurations.Remove(configuration);
        await _context.SaveChangesAsync();
        return true;
    }
}
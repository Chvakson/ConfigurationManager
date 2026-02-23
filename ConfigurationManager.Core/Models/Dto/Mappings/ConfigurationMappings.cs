using ConfigurationManager.Db.Models;

namespace ConfigurationManager.Core.Models.Dto.Mappings;

public static class ConfigurationMappings
{
    /// <summary>
    /// Преобразует Configuration в ConfigurationResponse (для списка)
    /// </summary>
    public static ConfigurationResponse ToResponse(this Configuration config)
    {
        return new ConfigurationResponse
        {
            Id = config.Id,
            Name = config.Name,
            UserId = config.UserId,
            CreatedAt = config.CreatedAt,
            UpdatedAt = config.UpdatedAt,
            IsActive = config.IsActive,
            VersionsCount = config.Versions?.Count ?? 0,
            CurrentVersion = config.Versions?
                .Where(v => v.IsActive)  // ← фильтр по активной версии
                .Select(v => new ConfigurationVersionDto
                {
                    VersionNumber = v.VersionNumber,
                    SettingsJson = v.SettingsJson,
                    CreatedAt = v.CreatedAt,
                    IsActive = v.IsActive  // ← добавить
                }).FirstOrDefault()
        };
    }

    /// <summary>
    /// Преобразует коллекцию Configuration в коллекцию ConfigurationResponse
    /// </summary>
    public static IEnumerable<ConfigurationResponse> ToResponse(this IEnumerable<Configuration> configs)
    {
        return configs.Select(c => c.ToResponse());
    }

    /// <summary>
    /// Преобразует Configuration в ConfigurationDetailResponse (с деталями и версиями)
    /// </summary>
    public static ConfigurationDetailResponse ToDetailResponse(this Configuration config)
    {
        return new ConfigurationDetailResponse
        {
            Id = config.Id,
            Name = config.Name,
            UserId = config.UserId,
            CreatedAt = config.CreatedAt,
            UpdatedAt = config.UpdatedAt,
            IsActive = config.IsActive,
            VersionsCount = config.Versions?.Count ?? 0,
            CurrentVersion = config.Versions?
                .Where(v => v.IsActive)
                .Select(v => new ConfigurationVersionDto
                {
                    VersionNumber = v.VersionNumber,
                    SettingsJson = v.SettingsJson,
                    CreatedAt = v.CreatedAt,
                    IsActive = v.IsActive
                }).FirstOrDefault(),
            Versions = config.Versions?
                .OrderByDescending(v => v.VersionNumber)
                .Select(v => new ConfigurationVersionDto
                {
                    VersionNumber = v.VersionNumber,
                    SettingsJson = v.SettingsJson,
                    CreatedAt = v.CreatedAt,
                    IsActive = v.IsActive
                }).ToList() ?? new()
        };
    }
}
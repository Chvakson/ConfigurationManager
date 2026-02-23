namespace ConfigurationManager.Core.Models.Dto;

// Для списка конфигураций (краткая информация)
public class ConfigurationResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsActive { get; set; }
    public int VersionsCount { get; set; }
    public ConfigurationVersionDto? CurrentVersion { get; set; }
}

// Для детальной информации (с версиями)
public class ConfigurationDetailResponse : ConfigurationResponse
{
    public List<ConfigurationVersionDto> Versions { get; set; } = new();
}

// Для информации о версии
public class ConfigurationVersionDto
{
    public int VersionNumber { get; set; }
    public string SettingsJson { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }

}

public class CreateConfigurationRequest
{
    public string Name { get; set; } = string.Empty;
    public string SettingsJson { get; set; } = string.Empty;
}

public class UpdateConfigurationRequest
{
    public string Name { get; set; } = string.Empty;
    public string SettingsJson { get; set; } = string.Empty;
}
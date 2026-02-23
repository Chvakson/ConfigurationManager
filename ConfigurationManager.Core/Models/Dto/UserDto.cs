namespace ConfigurationManager.Core.Models.Dto;

public class UserDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class UserWithConfigurationsDto : UserDto
{
    public IEnumerable<ConfigurationResponse> Configurations { get; set; } = new List<ConfigurationResponse>();
}

public class ConfigurationSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int VersionsCount { get; set; }
}
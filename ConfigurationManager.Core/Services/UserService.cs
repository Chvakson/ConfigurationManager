using ConfigurationManager.Core.Constants;
using ConfigurationManager.Core.Models.Dto;
using ConfigurationManager.Db.Models;
using ConfigurationManager.Db.Repositories;

namespace ConfigurationManager.Core.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfigurationRepository _configurationRepository;

    public UserService(
        IUserRepository userRepository,
        IConfigurationRepository configurationRepository)
    {
        _userRepository = userRepository;
        _configurationRepository = configurationRepository;
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        return await _userRepository.GetAllAsync();
    }

    public async Task<User?> GetUserByIdAsync(Guid id)
    {
        return await _userRepository.GetByIdAsync(id);
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _userRepository.GetByEmailAsync(email);
    }

    public async Task<User> CreateUserAsync(string username, string email)
    {
        if (await _userRepository.ExistsAsync(email))
        {
            throw new InvalidOperationException("User with this email already exists");
        }

        var user = new User
        {
            Username = username,
            Email = email
        };

        var createdUser = await _userRepository.CreateAsync(user);

        var defaultConfig = new Configuration
        {
            Name = DefaultSettings.DefaultConfigurationName,
            UserId = createdUser.Id,
            IsActive = true,
            Versions = new List<ConfigurationVersion>
            {
                new ConfigurationVersion
                {
                    VersionNumber = 1,
                    SettingsJson = DefaultSettings.GetDefaultSettingsJson(),
                    IsActive = true
                }
            }
        };

        await _configurationRepository.CreateAsync(defaultConfig);

        return createdUser;
    }

    public async Task<User?> UpdateUserAsync(Guid id, string? username, string? email)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return null;

        if (email != null && email != user.Email)
        {
            if (await _userRepository.ExistsAsync(email))
            {
                throw new InvalidOperationException("User with this email already exists");
            }
        }

        var updatedUser = new User
        {
            Username = username ?? user.Username,
            Email = email ?? user.Email
        };

        return await _userRepository.UpdateAsync(id, updatedUser);
    }

    public async Task<bool> DeleteUserAsync(Guid id)
    {
        return await _userRepository.DeleteAsync(id);
    }

    public async Task<UserWithConfigurationsDto?> GetUserWithConfigurationsAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return null;

        var configurations = await _configurationRepository.GetByUserIdAsync(id);

        return new UserWithConfigurationsDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            CreatedAt = user.CreatedAt,
            Configurations = configurations.Select(c => new ConfigurationResponse
            {
                Id = c.Id,
                Name = c.Name,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                VersionsCount = c.Versions?.Count ?? 0
            })
        };
    }
}

public class ConfigurationDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int VersionsCount { get; set; }
}
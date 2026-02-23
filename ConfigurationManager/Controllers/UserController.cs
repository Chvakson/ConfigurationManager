using ConfigurationManager.Core.Models.Dto.Mappings;
using ConfigurationManager.Core.Models.Dto;
using ConfigurationManager.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace ConfigurationManager.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IConfigurationService _configurationService;


    public UserController(IUserService userService, IConfigurationService configurationService)
    {
        _userService = userService;
        _configurationService = configurationService;
    }

    /// <summary>
    /// Получить всех пользователей
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(users.ToDto());
    }


    /// <summary>
    /// Получить пользователя по id
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUser(Guid id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null) return NotFound();
        return Ok(user.ToDto());
    }

    /// <summary>
    /// Получить пользователя с конфигурациями
    /// </summary>
    [HttpGet("{id}/configurations")]
    public async Task<ActionResult<UserWithConfigurationsDto>> GetUserWithConfigurations(Guid id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null) return NotFound();

        var configs = await _configurationService.GetUserConfigurationsAsync(id);

        var result = new UserWithConfigurationsDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            CreatedAt = user.CreatedAt,
            Configurations = configs.Select(c => c.ToResponse()).ToList()  // ← используем маппинг

        };

        return Ok(result);
    }

    /// <summary>
    /// Создать нового поьзьзователя с дефолтной конфигурацией
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<UserDto>> CreateUser(CreateUserRequest request)
    {
        try
        {
            var user = await _userService.CreateUserAsync(request.Username, request.Email);
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user.ToDto());
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Обновить данные пользователя
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(Guid id, UpdateUserRequest request)
    {
        try
        {
            var user = await _userService.UpdateUserAsync(id, request.Username, request.Email);
            if (user == null) return NotFound();
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Удалить пользователя
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var deleted = await _userService.DeleteUserAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }
}

public class CreateUserRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class UpdateUserRequest
{
    public string? Username { get; set; }
    public string? Email { get; set; }
}
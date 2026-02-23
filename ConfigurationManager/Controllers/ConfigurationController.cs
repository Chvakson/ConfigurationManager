using ConfigurationManager.Core.Models;
using ConfigurationManager.Core.Models.Dto;
using ConfigurationManager.Core.Models.Dto.Mappings;
using ConfigurationManager.Core.Services;
using ConfigurationManager.Db.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ConfigurationManager.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConfigurationController : ControllerBase
{
    private readonly IConfigurationService _configurationService;

    public ConfigurationController(IConfigurationService configurationService)
    {
        _configurationService = configurationService;
    }


    /// <summary>
    /// Получить список всех конфигураций по фильтру
    /// </summary>
    /// <param name="sort">
    /// Тип сортировки:<br/>
    /// 0 - DateAsc (по дате, старые-новые)<br/>
    /// 1 - DateDesc (по дате, новые-старые)<br/>
    /// 2 - NameAsc (по имени А-Я)<br/>
    /// 3 - NameDesc (по имени Я-А)<br/>
    /// </param>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ConfigurationResponse>>> GetConfigurations(
        [FromQuery] ConfigurationSort sort = ConfigurationSort.DateAsc)
    {
        var configs = await _configurationService.GetSortedConfigurationsAsync(sort);
        return Ok(configs);
    }


    /// <summary>
    /// Получить конфигурации поьзователя
    /// </summary>
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<ConfigurationResponse>>> GetUserConfigurations(Guid userId)
    {
        var configs = await _configurationService.GetUserConfigurationsAsync(userId);
        return Ok(configs.ToResponse());
    }

    /// <summary>
    /// Получить конфигурацию по id
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ConfigurationDetailResponse>> GetConfiguration(Guid id)
    {
        var config = await _configurationService.GetConfigurationWithVersionsAsync(id);

        if (config == null)
            return NotFound();

        return Ok(config.ToDetailResponse());
    }

    /// <summary>
    /// Создать конфигурацию для пользователя
    /// </summary>
    /// <remarks>
    /// ## Допустимые значения для настроек:
    /// 
    /// ### Тема (theme):
    ///   - "light" - светлая
    ///   - "dark" - тёмная
    ///   - "system" - системная
    ///   - "high-contrast" - высокая контрастность
    /// 
    /// ### Цвета (colors):
    ///   - background: "#ffffff", "#1e1e1e", "#000000"
    ///   - text: "#000000", "#ffffff", "#1e1e1e"
    ///   - accent: "#007acc", "#0066b4", "#f48771", "#ffcc00"
    /// 
    /// ### Шрифты (font):
    ///   - family: "Segoe UI", "Arial", "Consolas", "Roboto", "system"
    ///   - size: от 8 до 24 (8,9,10,11,12,14,16,18,20,24)
    ///   - lineHeight: от 1.0 до 3.0
    /// 
    /// ### Горячие клавиши (hotkeys):
    ///   - Модификаторы: Ctrl, Alt, Shift, Win
    ///   - Клавиши: A-Z, F1-F12, 0-9
    ///   - Примеры: "Ctrl+S", "Alt+F4", "F5"
    /// 
    /// ### Расположение окон (windowLayout):
    ///   - position: "center", "left", "right", "top", "bottom"
    ///   - sidebar.position: "left", "right"
    /// 
    /// Полный пример смотри в модели запроса.
    /// </remarks>
    [HttpPost("user/{userId}")]
    public async Task<ActionResult<ConfigurationResponse>> CreateConfiguration(Guid userId, CreateConfigurationRequest request)
    {
        try
        {
            var config = await _configurationService.CreateConfigurationAsync(
                userId,
                request.Name,
                request.SettingsJson);

            return CreatedAtAction(nameof(GetConfiguration),
                new { id = config.Id },
                config.ToResponse());
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Обновить конфигурацию
    /// </summary>
    /// <remarks>
    /// ## Допустимые значения для настроек:
    /// 
    /// ### Тема (theme):
    ///   - "light" - светлая
    ///   - "dark" - тёмная
    ///   - "system" - системная
    ///   - "high-contrast" - высокая контрастность
    /// 
    /// ### Цвета (colors):
    ///   - background: "#ffffff", "#1e1e1e", "#000000"
    ///   - text: "#000000", "#ffffff", "#1e1e1e"
    ///   - accent: "#007acc", "#0066b4", "#f48771", "#ffcc00"
    /// 
    /// ### Шрифты (font):
    ///   - family: "Segoe UI", "Arial", "Consolas", "Roboto", "system"
    ///   - size: от 8 до 24 (8,9,10,11,12,14,16,18,20,24)
    ///   - lineHeight: от 1.0 до 3.0
    /// 
    /// ### Горячие клавиши (hotkeys):
    ///   - Модификаторы: Ctrl, Alt, Shift, Win
    ///   - Клавиши: A-Z, F1-F12, 0-9
    ///   - Примеры: "Ctrl+S", "Alt+F4", "F5"
    /// 
    /// ### Расположение окон (windowLayout):
    ///   - position: "center", "left", "right", "top", "bottom"
    ///   - sidebar.position: "left", "right"
    /// 
    /// Полный пример смотри в модели запроса.
    /// </remarks>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateConfiguration(Guid id, UpdateConfigurationRequest request)
    {
        try
        {
            var config = await _configurationService.UpdateConfigurationAsync(id, request.Name, request.SettingsJson);

            if (config == null)
                return NotFound();

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Выбрать версию конфигурации
    /// </summary>
    [HttpPost("{id}/rollback/{versionNumber}")]
    public async Task<IActionResult> RollbackToVersion(Guid id, int versionNumber)
    {
        var config = await _configurationService.RollbackToVersionAsync(id, versionNumber);

        if (config == null)
            return NotFound();

        return Ok(new { message = $"Rolled back to version {versionNumber}" });
    }

    /// <summary>
    /// Активировать конфигурацию
    /// </summary>
    [HttpPost("user/{userId}/activate/{id}")]
    public async Task<IActionResult> ActivateConfiguration(Guid userId, Guid id)
    {
        var result = await _configurationService.SetActiveConfigurationAsync(userId, id);

        if (!result)
            return NotFound();

        return Ok(new { message = "Configuration activated" });
    }

    /// <summary>
    /// Удалить конфигурацию
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteConfiguration(Guid id)
    {
        var result = await _configurationService.DeleteConfigurationAsync(id);

        if (!result)
            return NotFound();

        return NoContent();
    }
}
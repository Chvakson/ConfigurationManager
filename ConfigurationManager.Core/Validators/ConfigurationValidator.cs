using ConfigurationManager.Core.Models.Settings;
using ConfigurationManager.Db.Repositories;
using System.ComponentModel.DataAnnotations;

namespace ConfigurationManager.Db.Validators;

public static class ConfigurationValidator
{
    public static (bool IsValid, IEnumerable<string> Errors) ValidateSettings(UserConfigurationSettings settings)
    {
        var results = new List<ValidationResult>();

        ValidateObjectRecursive(settings, results, new HashSet<object>());

        return (!results.Any(), results.Select(r => r.ErrorMessage ?? string.Empty));
    }

    private static void ValidateObjectRecursive(object obj, List<ValidationResult> results, HashSet<object> visited)
    {
        if (obj == null || visited.Contains(obj)) return;

        visited.Add(obj);

        // Валидация текущего объекта
        var context = new ValidationContext(obj);
        Validator.TryValidateObject(obj, context, results, true);

        // Рекурсивная валидация всех свойств
        foreach (var prop in obj.GetType().GetProperties())
        {
            if (prop.PropertyType.IsClass && prop.PropertyType != typeof(string))
            {
                var value = prop.GetValue(obj);
                if (value != null)
                {
                    ValidateObjectRecursive(value, results, visited);
                }
            }
        }
    }

    public static (bool IsValid, IEnumerable<string> Errors) ValidateSettingsJson(string json)
    {
        try
        {
            var options = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var settings = System.Text.Json.JsonSerializer.Deserialize<UserConfigurationSettings>(json, options);

            if (settings == null)
                return (false, new[] { "Invalid JSON format" });

            return ValidateSettings(settings);
        }
        catch (System.Text.Json.JsonException ex)
        {
            return (false, new[] { $"JSON parsing error: {ex.Message}" });
        }
    }

    public static async Task<(bool IsValid, string? Error)> ValidateConfigurationNameAsync(
      IConfigurationRepository repository,
      string name,
      Guid userId,
      Guid? currentConfigurationId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return (false, "Configuration name cannot be empty");
        }

        var existingConfig = await repository.GetByNameAndUserIdAsync(name, userId);

        if (existingConfig != null)
        {
            if (currentConfigurationId.HasValue && existingConfig.Id == currentConfigurationId.Value)
            {
                return (true, null);
            }

            return (false, $"You already have a configuration named '{name}'");
        }

        return (true, null);
    }
}
using ConfigurationManager.Core.Constants;
using ConfigurationManager.Core.Models.Settings;
using System.ComponentModel.DataAnnotations;

namespace ConfigurationManager.Db.Validators;

public class AllowedThemeAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is string theme && ConfigurationConstants.Themes.All.Contains(theme))
            return ValidationResult.Success;

        return new ValidationResult($"Theme must be one of: {string.Join(", ", ConfigurationConstants.Themes.All)}");
    }
}

public class AllowedBackgroundColorAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is string color && ConfigurationConstants.Colors.AllBackgrounds.Contains(color))
            return ValidationResult.Success;

        return new ValidationResult($"Background color must be one of: {string.Join(", ", ConfigurationConstants.Colors.AllBackgrounds)}");
    }
}

public class AllowedTextColorAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is string color && ConfigurationConstants.Colors.AllText.Contains(color))
            return ValidationResult.Success;

        return new ValidationResult($"Text color must be one of: {string.Join(", ", ConfigurationConstants.Colors.AllText)}");
    }
}

public class AllowedAccentColorAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is string color && ConfigurationConstants.Colors.AllAccents.Contains(color))
            return ValidationResult.Success;

        return new ValidationResult($"Accent color must be one of: {string.Join(", ", ConfigurationConstants.Colors.AllAccents)}");
    }
}

public class AllowedFontAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is string font && ConfigurationConstants.Fonts.All.Contains(font))
            return ValidationResult.Success;

        return new ValidationResult($"Font must be one of: {string.Join(", ", ConfigurationConstants.Fonts.All)}");
    }
}

public class AllowedWindowPositionAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is string position && ConfigurationConstants.WindowPositions.All.Contains(position))
            return ValidationResult.Success;

        return new ValidationResult($"Window position must be one of: {string.Join(", ", ConfigurationConstants.WindowPositions.All)}");
    }
}

public class HotkeyValidationAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not HotkeySettings hotkeySettings)
            return ValidationResult.Success;

        var errors = new List<string>();

        foreach (var kvp in hotkeySettings.KeyBindings)
        {
            if (!IsValidHotkey(kvp.Value))
                errors.Add($"Invalid hotkey format for '{kvp.Key}': {kvp.Value}");
        }

        if (errors.Any())
            return new ValidationResult(string.Join("; ", errors));

        return ValidationResult.Success;
    }

    private bool IsValidHotkey(string hotkey)
    {
        hotkey = System.Text.RegularExpressions.Regex.Unescape(hotkey);

        var parts = hotkey.Split('+');

        if (parts.Length == 1)
        {
            return ConfigurationConstants.HotkeyValidation.Keys.Contains(parts[0]);
        }

        if (parts.Length == 2)
        {
            return ConfigurationConstants.HotkeyValidation.Modifiers.Contains(parts[0]) &&
                   ConfigurationConstants.HotkeyValidation.Keys.Contains(parts[1]);
        }

        return false;
    }
}
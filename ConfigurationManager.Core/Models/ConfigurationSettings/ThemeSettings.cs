using System.ComponentModel.DataAnnotations;
using ConfigurationManager.Core.Constants;
using ConfigurationManager.Db.Validators;

namespace ConfigurationManager.Core.Models.Settings;

public class ThemeSettings
{
    [Required]
    [AllowedTheme]
    public string Theme { get; set; } = ConfigurationConstants.Themes.System;

    [Required]
    public ColorSettings Colors { get; set; } = new();
}
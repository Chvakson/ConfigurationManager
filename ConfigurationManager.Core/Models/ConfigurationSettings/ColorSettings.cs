using System.ComponentModel.DataAnnotations;
using ConfigurationManager.Core.Constants;
using ConfigurationManager.Db.Validators;

namespace ConfigurationManager.Core.Models.Settings;

public class ColorSettings
{
    [Required]
    [AllowedBackgroundColor]
    public string Background { get; set; } = ConfigurationConstants.Colors.White;

    [Required]
    [AllowedTextColor]
    public string Text { get; set; } = ConfigurationConstants.Colors.Black;

    [Required]
    [AllowedAccentColor]
    public string Accent { get; set; } = ConfigurationConstants.Colors.Blue;
}
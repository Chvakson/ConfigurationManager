using System.ComponentModel.DataAnnotations;
using ConfigurationManager.Core.Constants;
using ConfigurationManager.Db.Validators;

namespace ConfigurationManager.Core.Models.Settings;

public class FontSettings
{
    [Required]
    [AllowedFont]
    public string Family { get; set; } = ConfigurationConstants.Fonts.SegoeUI;

    [Required]
    [Range(ConfigurationConstants.FontSizes.Min, ConfigurationConstants.FontSizes.Max)]
    public int Size { get; set; } = 14;

    [Range(1.0, 3.0)]
    public double LineHeight { get; set; } = 1.5;
}
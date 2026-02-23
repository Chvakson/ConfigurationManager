using System.ComponentModel.DataAnnotations;

namespace ConfigurationManager.Core.Models.Settings;

public class UserConfigurationSettings
{
    [Required]
    public ThemeSettings Theme { get; set; } = new();

    [Required]
    public FontSettings Font { get; set; } = new();

    public HotkeySettings Hotkeys { get; set; } = new();

    public WindowLayoutSettings WindowLayout { get; set; } = new();
}
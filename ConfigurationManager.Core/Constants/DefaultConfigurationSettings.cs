using ConfigurationManager.Core.Models.Settings;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace ConfigurationManager.Core.Constants;

public static class DefaultSettings
{
    public const string DefaultConfigurationName = "Default";

    public static UserConfigurationSettings GetDefaultSettings()
    {
        return new UserConfigurationSettings
        {
            Theme = new ThemeSettings
            {
                Theme = ConfigurationConstants.Themes.System,
                Colors = new ColorSettings
                {
                    Background = ConfigurationConstants.Colors.White,
                    Text = ConfigurationConstants.Colors.Black,
                    Accent = ConfigurationConstants.Colors.Blue
                }
            },
            Font = new FontSettings
            {
                Family = ConfigurationConstants.Fonts.SegoeUI,
                Size = 14,
                LineHeight = 1.5
            },
            Hotkeys = new HotkeySettings(),
            WindowLayout = new WindowLayoutSettings
            {
                Fullscreen = false,
                Sidebar = new SidebarSettings
                {
                    Visible = true,
                    Width = 250,
                    Position = ConfigurationConstants.WindowPositions.Right
                }
            }
        };
    }

    public static string GetDefaultSettingsJson()
    {
        var settings = GetDefaultSettings();
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        return JsonSerializer.Serialize(settings, options);
    }
}
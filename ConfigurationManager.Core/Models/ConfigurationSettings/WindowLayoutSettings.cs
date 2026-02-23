using System.ComponentModel.DataAnnotations;
using ConfigurationManager.Core.Constants;
using ConfigurationManager.Db.Validators;

namespace ConfigurationManager.Core.Models.Settings;

public class WindowLayoutSettings
{
    public bool Fullscreen { get; set; } = false;

    [Range(0, 4000)]
    public int? Width { get; set; }

    [Range(0, 4000)]
    public int? Height { get; set; }

    [AllowedWindowPosition]
    public string Position { get; set; } = ConfigurationConstants.WindowPositions.Center;

    public SidebarSettings? Sidebar { get; set; }
}

public class SidebarSettings
{
    public bool Visible { get; set; } = true;

    [Range(100, 800)]
    public int Width { get; set; } = 250;

    [AllowedWindowPosition]
    public string Position { get; set; } = ConfigurationConstants.WindowPositions.Right;
}
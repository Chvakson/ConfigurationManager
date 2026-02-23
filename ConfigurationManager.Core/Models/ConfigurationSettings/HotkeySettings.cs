using ConfigurationManager.Db.Validators;

namespace ConfigurationManager.Core.Models.Settings;

public class HotkeySettings
{
    [HotkeyValidation]
    public Dictionary<string, string> KeyBindings { get; set; } = new()
    {
        ["save"] = "Ctrl+S",
        ["copy"] = "Ctrl+C",
        ["paste"] = "Ctrl+V",
        ["find"] = "Ctrl+F"
    };
}
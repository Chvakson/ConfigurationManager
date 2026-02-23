namespace ConfigurationManager.Core.Constants;

public static class ConfigurationConstants
{
    public static class Themes
    {
        public const string Light = "light";
        public const string Dark = "dark";
        public const string System = "system";
        public const string HighContrast = "high-contrast";

        public static readonly string[] All = { Light, Dark, System, HighContrast };
    }

    public static class Colors
    {
        public const string White = "#ffffff";
        public const string Black = "#000000";
        public const string DarkGray = "#1e1e1e";
        public const string Blue = "#007acc";
        public const string LightBlue = "#0066b4";
        public const string Red = "#f48771";
        public const string Yellow = "#ffcc00";

        public static readonly string[] AllBackgrounds = { White, DarkGray, Black };
        public static readonly string[] AllText = { Black, White, DarkGray };
        public static readonly string[] AllAccents = { Blue, LightBlue, Red, Yellow };
    }

    public static class Fonts
    {
        public const string SegoeUI = "Segoe UI";
        public const string Arial = "Arial";
        public const string Consolas = "Consolas";
        public const string Roboto = "Roboto";
        public const string System = "system";

        public static readonly string[] All = { SegoeUI, Arial, Consolas, Roboto, System };
    }

    public static class FontSizes
    {
        public static readonly int[] All = { 8, 9, 10, 11, 12, 14, 16, 18, 20, 24 };
        public const int Min = 8;
        public const int Max = 24;
    }

    public static class WindowPositions
    {
        public const string Center = "center";
        public const string Left = "left";
        public const string Right = "right";
        public const string Top = "top";
        public const string Bottom = "bottom";

        public static readonly string[] All = { Center, Left, Right, Top, Bottom };
    }

    public static class HotkeyValidation
    {
        public static readonly string[] Modifiers = { "Ctrl", "Alt", "Shift", "Win" };
        public static readonly string[] Keys = {
            "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M",
            "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z",
            "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "F10", "F11", "F12",
            "0", "1", "2", "3", "4", "5", "6", "7", "8", "9"
        };
    }
}
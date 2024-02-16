using I2.Loc;

namespace ksizer.Utils;

public static class LocalizationStrings
{
    public static readonly Dictionary<string, LocalizedString> OAB_DESCRIPTION = new()
    {
        { "Name", "KSizer/OAB/Name" },
        { "Radius", "KSizer/OAB/Radius" },
        { "Height", "KSizer/OAB/Height" },
        { "Type", "KSizer/OAB/Type" },
        { "Texture", "KSizer/OAB/Texture" }
    };
    public static readonly Dictionary<string, LocalizedString> OAB_TANK = new()
    {
        { "Tank1", "KSizer/OAB/Tank1" },
        { "Tank2", "KSizer/OAB/Tank2" }
    };
}

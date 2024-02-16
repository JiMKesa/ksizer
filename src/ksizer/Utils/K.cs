using BepInEx.Logging;

namespace ksizer.Utils;

public static class K
{
    private static readonly ManualLogSource ksizer = Logger.CreateLogSource("Kesa Log");
    public static void Log(string msg)
    {
        ksizer.LogDebug(msg);
    }
}

using BepInEx;
using JetBrains.Annotations;
using SpaceWarp;
using SpaceWarp.API.Mods;

namespace ksizer;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency(SpaceWarpPlugin.ModGuid, SpaceWarpPlugin.ModVer)]
public class ksizerPlugin : BaseSpaceWarpPlugin
{
    // Useful in case some other mod wants to use this mod a dependency
    [PublicAPI] public const string ModGuid = MyPluginInfo.PLUGIN_GUID;
    [PublicAPI] public const string ModName = MyPluginInfo.PLUGIN_NAME;
    [PublicAPI] public const string ModVer = MyPluginInfo.PLUGIN_VERSION;

    /// Singleton instance of the plugin class
    [PublicAPI] public static ksizerPlugin Instance { get; set; }
    public override void OnInitialized()
    {
        base.OnInitialized();

        Instance = this;

        // Load all the other assemblies used by this mod
        //LoadAssemblies();
    }

    private static void LoadAssemblies()
    {
        // Load the Unity project assembly
        //var currentFolder = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory!.FullName;
        //var unityAssembly = Assembly.LoadFrom(Path.Combine(currentFolder, "ksizer.Unity.dll"));
        // Register any custom UI controls from the loaded assembly
        //CustomControls.RegisterFromAssembly(unityAssembly);
    }
}

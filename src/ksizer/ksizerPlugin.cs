using BepInEx;
using JetBrains.Annotations;
using SpaceWarp;
using SpaceWarp.API.Mods;
using ksizer.Utils;
using HarmonyLib;
using SpaceWarp.Modules;
using System.Reflection;
using UitkForKsp2.API;
using SpaceWarp.API.Assets;
using UnityEngine;
using SpaceWarp.API.UI.Appbar;
using ksizer.Modules;

namespace ksizer;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency(SpaceWarpPlugin.ModGuid, SpaceWarpPlugin.ModVer)]
public class KsizerPlugin : BaseSpaceWarpPlugin
{
    // Useful in case some other mod wants to use this mod a dependency
    [PublicAPI] public const string ModGuid = MyPluginInfo.PLUGIN_GUID;
    [PublicAPI] public const string ModName = MyPluginInfo.PLUGIN_NAME;
    [PublicAPI] public const string ModVer = MyPluginInfo.PLUGIN_VERSION;

    /// Singleton instance of the plugin class
    [PublicAPI] public static KsizerPlugin Instance { get; set; }

    internal const string ToolbarOabButtonID = "BTN-kesaOAB";
    internal const string ToolbarFlightButtonID = "BTN-kesaFlight";

    public override void OnInitialized()
    {
        base.OnInitialized();
        Settings.Initialize();
        Instance = this;
        // Load all the other assemblies used by this mod
        LoadAssemblies();
    }

    private static void LoadAssemblies()
    {
        // Load the Unity project assembly
        var currentFolder = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory!.FullName;
        var unityAssembly = Assembly.LoadFrom(Path.Combine(currentFolder, "ksizer.Unity.dll"));
        // Register any custom UI controls from the loaded assembly
        CustomControls.RegisterFromAssembly(unityAssembly);
    }
}

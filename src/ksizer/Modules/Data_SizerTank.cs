using KSP.Api;
using KSP.Modules;
using KSP.Sim;
using KSP.Sim.Definitions;
using Newtonsoft.Json;
using UnityEngine;
namespace ksizer.Modules;

[Serializable]

public class Data_SizerTank : ModuleData
{
    public override Type ModuleType => typeof(Module_SizerTank);
    [LocalizedField("KSizer/OAB/Radius")]
    [KSPState(CopyToSymmetrySet = true)]
    [HideInInspector]
    [SteppedRange(0f, 6f, 1f)]
    [PAMDisplayControl(SortIndex = 2)]
    public ModuleProperty<float> SliderScaleWidth = new ModuleProperty<float>(1f, false, new ToStringDelegate(GetConversionScale));

    [LocalizedField("KSizer/OAB/Height")]
    [KSPState(CopyToSymmetrySet = true)]
    [HideInInspector]
    [SteppedRange(1f, 20f, 1f)]
    [PAMDisplayControl(SortIndex = 3)]
    public ModuleProperty<float> SliderScaleHeight = new ModuleProperty<float>(1f, false, new ToStringDelegate(GetConversionScale));

    private static string GetConversionScale(object valueObj)
    {
        return ((float) valueObj).ToString("F0");
    }

    [JsonIgnore]
    public PartComponentModule_SizerTank PartComponentModule;

}

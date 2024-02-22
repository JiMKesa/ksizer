using ksizer.Utils;
using KSP.Api;
using KSP.Modules;
using KSP.Sim;
using KSP.Sim.Definitions;
using KSP.UI.Binding;
using Newtonsoft.Json;
using UnityEngine;

namespace ksizer.Modules;
using static ksizer.Utils.Settings;

[Serializable]

public class Data_SizerTank : ModuleData
{
    public static Module_SizerTank _Module_SizerTank;
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
    [SteppedRange(1f, 30f, 1f)]
    [PAMDisplayControl(SortIndex = 3)]
    public ModuleProperty<float> SliderScaleHeight = new ModuleProperty<float>(1f, false, new ToStringDelegate(GetConversionScale));
    
    [LocalizedField("KSizer/OAB/Resource")]
    [KSPState(CopyToSymmetrySet = true)]
    [HideInInspector]
    [PAMDisplayControl(SortIndex = 4)]
    public ModuleProperty<string> ResourcesList = new ModuleProperty<string>("7");
    //_Module_SizerTank.idresource.ToString()

    [KSPState(CopyToSymmetrySet = true)]
    public float mass;
    [KSPState(CopyToSymmetrySet = true)]
    public float DryMass;
    [KSPState(CopyToSymmetrySet = true)]
    private double massModifyAmount;
    [KSPState(CopyToSymmetrySet = true)]
    private double resourceMass;
    /*
    private double mass;
    private double massModifyAmount;
    private double resourceMass;
    private double greenMass;
    */

    private static string GetConversionScale(object valueObj)
    {
        return ((float) valueObj).ToString("F0");
    }
    
    public override void OnPartBehaviourModuleInit()
    {
        var dropdownList = new DropdownItemList();
        foreach (int id in Enum.GetValues(typeof(FuelTypes)))
        {
            string item = Enum.GetName(typeof(FuelTypes), id);
            dropdownList.Add(item, new DropdownItem() { key = id.ToString(), text = item });
        }
        SetDropdownData(ResourcesList, dropdownList);
    }

    [JsonIgnore]
    public PartComponentModule_SizerTank PartComponentModule;

}

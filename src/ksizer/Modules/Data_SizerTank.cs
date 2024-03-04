using ksizer.Utils;
using KSP.Api;
using KSP.Modules;
using KSP.Sim;
using KSP.Sim.Definitions;
using KSP.UI.Binding;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI.Extensions;

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
    [PAMDisplayControl(SortIndex = 2)]
    public ModuleProperty<string> SliderScaleWidth = new ModuleProperty<string>("1");

    [LocalizedField("KSizer/OAB/Height")]
    [KSPState(CopyToSymmetrySet = true)]
    [HideInInspector]
    [PAMDisplayControl(SortIndex = 3)]
    public ModuleProperty<string> SliderScaleHeight = new ModuleProperty<string>("1");

    [LocalizedField("KSizer/OAB/Material")]
    [KSPState(CopyToSymmetrySet = true)]
    [HideInInspector]
    [PAMDisplayControl(SortIndex = 4)]
    public ModuleProperty<string> SliderMaterial = new ModuleProperty<string>("1");
    /*
    [LocalizedField("KSizer/OAB/Resource")]
    [KSPState(CopyToSymmetrySet = true)]
    [HideInInspector]
    [PAMDisplayControl(SortIndex = 5)]
    public ModuleProperty<string> ResourcesList = new ModuleProperty<string>("8");
    //_Module_SizerTank.idresource.ToString()
    */
    [KSPState(CopyToSymmetrySet = true)]
    public float mass;
    [KSPState(CopyToSymmetrySet = true)]
    public float DryMass;
    [KSPState(CopyToSymmetrySet = true)]
    private double massModifyAmount;
    [KSPState(CopyToSymmetrySet = true)]
    private double resourceMass;
    [KSPState]
    public Vector3 AssemblyRelativePosition = Vector3.zero;

    //public DropdownItemList MaterialList = new DropdownItemList();

    private static string GetConversionScale(object valueObj)
    {
        return ((float) valueObj).ToString("F0");
    }

    public override void OnPartBehaviourModuleInit()
    {
        var ScaleWList = new DropdownItemList();
        for (int cpt = 0; cpt < 7; cpt++)
        {
            ScaleWList.Add(cpt.ToString(), new DropdownItem() { key = cpt.ToString(), text = cpt.ToString() });
        }
        SetDropdownData(SliderScaleWidth, ScaleWList);

        var ScaleHList = new DropdownItemList();
        for (int cpt = 1; cpt < 31; cpt++)
        {
            ScaleHList.Add(cpt.ToString(), new DropdownItem() { key = cpt.ToString(), text = cpt.ToString() });
        }
        SetDropdownData(SliderScaleHeight, ScaleHList);

        //var MaterialList = new DropdownItemList();
        ///SetDropdownData(SliderMaterial, MaterialList);
    }
/*
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
*/
    [JsonIgnore]
    public PartComponentModule_SizerTank PartComponentModule;
}

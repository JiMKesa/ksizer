using KSP.Api;
using KSP.Modules;
using KSP.Sim;
using KSP.Sim.Definitions;
using Newtonsoft.Json;
using UnityEngine;
using static KSP.OAB.ObjectAssemblyMagicValues;

namespace ksizer.Modules;
[Serializable]

public class Data_SizerTank : ModuleData
{
    public override Type ModuleType => typeof(Module_SizerTank);

    [KSPState]
    [HideInInspector]
    [PAMDisplayControl(SortIndex = 2)]
    [Range(0.0f, 5f)]
    public ModuleProperty<float> conversionRate = new ModuleProperty<float>(0.5f, false, new ToStringDelegate(Data_ResourceConverter.GetConversionRateString));

    // OAB SpaceObs description
    /*public override List<OABPartData.PartInfoModuleEntry> GetPartInfoEntries(Type partBehaviourModuleType,
        List<OABPartData.PartInfoModuleEntry> delegateList)
    {
        if (partBehaviourModuleType == ModuleType)
        {
            // add SpaceObs module description description
            delegateList.Add(new OABPartData.PartInfoModuleEntry("", (_) => LocalizationStrings.OAB_DESCRIPTION["ModuleDescription"]));
            // MapType header
            var entry = new OABPartData.PartInfoModuleEntry(LocalizationStrings.OAB_DESCRIPTION["ResourcesRequired"],
                _ =>
                {
                    // Subentries
                    var subEntries = new List<OABPartData.PartInfoModuleSubEntry>();
                    if (UseResources)
                    {
                        subEntries.Add(new OABPartData.PartInfoModuleSubEntry(
                            LocalizationStrings.OAB_DESCRIPTION["ElectricCharge"],
                            $"{RequiredResource.Rate.ToString("N3")} /s"
                        ));
                    }
                    return subEntries;
                });
            delegateList.Add(entry);
        }
        return delegateList;
    }*/

    [JsonIgnore]
    public PartComponentModule_SizerTank PartComponentModule;

}

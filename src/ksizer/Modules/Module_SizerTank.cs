using KSP.Sim.Definitions;
using UnityEngine;

namespace ksizer.Modules;
[DisallowMultipleComponent]

public class Module_SizerTank : PartBehaviourModule
{
    public override Type PartComponentModuleType => typeof(PartComponentModule_SizerTank);

    [SerializeField]
    protected Data_SizerTank _data_SizerTank;
    public override void AddDataModules()
    {
        base.AddDataModules();
        _data_SizerTank ??= new Data_SizerTank();
        DataModules.TryAddUnique(_data_SizerTank, out _data_SizerTank);
    }

    public override void OnInitialize()
    {
        base.OnInitialize();
        if (PartBackingMode == PartBackingModes.Flight)
        {
            moduleIsEnabled = false;
        }
        else
        {
            moduleIsEnabled = true;
            UpdateOabPAMVisibility();
        }
    }

    public override void OnShutdown()
    {

    }

    private void UpdateFlightPAMVisibility(bool state)
    {
        //_data_SizerTank.SetVisible(_Data_SpaceObs.EnabledToggle, true);
    }
    private void UpdateOabPAMVisibility()
    {
        //_data_SizerTank.SetVisible(_Data_SpaceObs.EnabledToggle, false);
    }

}

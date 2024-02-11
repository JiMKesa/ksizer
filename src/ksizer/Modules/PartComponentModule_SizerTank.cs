using KSP.Sim.impl;

namespace ksizer.Modules;

public class PartComponentModule_SizerTank : PartComponentModule
{
    public override Type PartBehaviourModuleType => typeof(Module_SizerTank);

    private Data_SizerTank _dataSizerTank;

    public override void OnStart(double universalTime)
    {
        if (!DataModules.TryGetByType<Data_SizerTank>(out _dataSizerTank))
            // check module exists
            if (!DataModules.TryGetByType<Data_SizerTank>(out _dataSizerTank))
            {
                return;
            }
    }

    public override void OnUpdate(double universalTime, double deltaUniversalTime)
    {

    }

}

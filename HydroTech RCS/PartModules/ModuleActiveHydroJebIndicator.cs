using HydroTech_RCS;
using HydroTech_RCS.PartModules.Base;

public class ModuleActiveHydroJebIndicator : ModuleActiveIndicator
{
    protected override Status GetStatus()
    {
        return HydroJebCore.IsActiveJeb((HydroJeb)this.part) ? Status.ACTIVE : Status.IDLE;
    }
}
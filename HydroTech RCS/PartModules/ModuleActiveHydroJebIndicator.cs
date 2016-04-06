using HydroTech_RCS;
using HydroTech_RCS.PartModules.Base;

public class ModuleActiveHydroJebIndicator : ModuleActiveIndicator
{
    protected override Status GetStatus()
    {
        if (HydroJebCore.IsActiveJeb((HydroJeb)this.part)) { return Status.ACTIVE; }
        return Status.IDLE;
    }
}
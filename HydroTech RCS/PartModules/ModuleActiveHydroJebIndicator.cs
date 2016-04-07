using HydroTech_RCS.PartModules.Base;

namespace HydroTech_RCS.PartModules
{
    public class ModuleActiveHydroJebIndicator : ModuleActiveIndicator
    {
        #region Overrides
        protected override Status GetStatus()
        {
            return HydroJebCore.IsActiveJeb((HydroJeb)this.part) ? Status.ACTIVE : Status.IDLE;
        }
        #endregion
    }
}
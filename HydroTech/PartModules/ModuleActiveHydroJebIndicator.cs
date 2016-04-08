using HydroTech.PartModules.Base;
using HydroTech.Parts;

namespace HydroTech.PartModules
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
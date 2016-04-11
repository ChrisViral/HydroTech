using HydroTech.Managers;
using HydroTech.PartModules.Base;

namespace HydroTech.PartModules
{
    public class ModuleActiveHydroJebIndicator : ModuleActiveIndicator
    {
        #region Overrides
        protected override Status GetStatus()
        {
            return HydroFlightManager.Instance.IsActiveJeb(this.part.Modules["HydroJebModule"] as HydroJebModule) ? Status.ACTIVE : Status.IDLE;
        }
        #endregion
    }
}
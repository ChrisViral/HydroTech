using System.Collections.Generic;
using HydroTech.PartModules.Base;

namespace HydroTech.Data
{
    public class HydroPartModuleList : HydroPartListBase<HydroPartModule>
    {
        #region Overrides
        protected override bool IsNull(HydroPartModule item)
        {
            return item.part == null;
        }

        protected override Vessel GetVessel(HydroPartModule item)
        {
            return item.vessel;
        }

        protected override void AddItem(List<HydroPartModule> list, HydroPartModule item)
        {
            base.AddItem(list, item);
            item.OnFlightStart();
        }

        protected override void RemoveItem(List<HydroPartModule> list, HydroPartModule item)
        {
            base.RemoveItem(list, item);
            item.OnDestroy();
        }

        protected override bool NeedsToRemove(HydroPartModule item)
        {
            return item.part == null;
        }
        #endregion
    }
}
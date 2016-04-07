using HydroTech_FC;
using HydroTech_RCS.PartModules;
using UnityEngine;

namespace HydroTech_RCS.Autopilots.Calculators
{
    public class DockingAssistCalculator : HoldDirectionCalculator
    {
        #region Methods
        public void Calculate(ModuleDockAssistCam mcam, ModuleDockAssistTarget mtgt)
        {
            Calculate(mtgt.Dir, mtgt.Right, mcam.Dir, mcam.Right, mcam.vessel);
            Vector3 r = mtgt.Pos - mcam.Pos;
            this.X = HMaths.DotProduct(r, mtgt.Right);
            this.Y = HMaths.DotProduct(r, mtgt.Down);
            this.Z = HMaths.DotProduct(r, mtgt.Dir);
        }
        #endregion
    }
}
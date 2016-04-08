using HydroTech.PartModules;
using UnityEngine;

namespace HydroTech.Autopilots.Calculators
{
    public class DockingAssistCalculator : HoldDirectionCalculator
    {
        #region Methods
        public void Calculate(ModuleDockAssistCam mcam, ModuleDockAssistTarget mtgt)
        {
            Calculate(mtgt.Dir, mtgt.Right, mcam.Dir, mcam.Right, mcam.vessel);
            Vector3 r = mtgt.Pos - mcam.Pos;
            this.x = Vector3.Dot(r, mtgt.Right);
            this.y = Vector3.Dot(r, mtgt.Down);
            this.z = Vector3.Dot(r, mtgt.Dir);
        }
        #endregion
    }
}
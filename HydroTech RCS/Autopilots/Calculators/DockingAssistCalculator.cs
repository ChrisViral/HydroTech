using HydroTech_FC;
using HydroTech_RCS.PartModules;
using HydroTech_RCS.Utils;
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
            this.X = Vector3.Dot(r, mtgt.Right);
            this.Y = Vector3.Dot(r, mtgt.Down);
            this.Z = Vector3.Dot(r, mtgt.Dir);
        }
        #endregion
    }
}
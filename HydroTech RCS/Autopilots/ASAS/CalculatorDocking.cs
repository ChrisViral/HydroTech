using HydroTech_FC;
using UnityEngine;

namespace HydroTech_RCS.Autopilots.ASAS
{
    public class DockAssistStateCalculator : HoldDirStateCalculator
    {
        public void Calculate(ModuleDockAssistCam mcam, ModuleDockAssistTarget mtgt)
        {
            Calculate(mtgt.Dir, mtgt.Right, mcam.Dir, mcam.Right, mcam.vessel);
            Vector3 r = mtgt.Pos - mcam.Pos;
            this.X = HMaths.DotProduct(r, mtgt.Right);
            this.Y = HMaths.DotProduct(r, mtgt.Down);
            this.Z = HMaths.DotProduct(r, mtgt.Dir);
        }

#if DEBUG
        public override string ToString()
        {
            return this.yaw.ToString("0.00") + "," + this.roll.ToString("0.00") + "," + this.pitch.ToString("0.00") + ";" + this.X.ToString("0.00") + "," + this.Y.ToString("0.00") + "," + this.Z.ToString("0.00");
        }
#endif
    }
}
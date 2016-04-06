using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HydroTech_RCS.Autopilots.ASAS
{
    using UnityEngine;
    using HydroTech_FC;

    public class DockAssistStateCalculator : HoldDirStateCalculator
    {
        public void Calculate(ModuleDockAssistCam mcam, ModuleDockAssistTarget mtgt)
        {
            Calculate(mtgt.Dir, mtgt.Right, mcam.Dir, mcam.Right, mcam.vessel);
            Vector3 r = mtgt.Pos - mcam.Pos;
            X = HMaths.DotProduct(r, mtgt.Right);
            Y = HMaths.DotProduct(r, mtgt.Down);
            Z = HMaths.DotProduct(r, mtgt.Dir);
        }

#if DEBUG
        public override string ToString()
        {
            return yaw.ToString("0.00") + ","
                + roll.ToString("0.00") + ","
                + pitch.ToString("0.00") + ";"
                + X.ToString("0.00") + ","
                + Y.ToString("0.00") + ","
                + Z.ToString("0.00");
        }
#endif
    }
}

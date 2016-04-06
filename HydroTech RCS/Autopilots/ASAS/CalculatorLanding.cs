using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HydroTech_RCS.Autopilots.ASAS
{
    using UnityEngine;
    using Constants.Autopilots.Landing;

    public class LandingStateCalculator : HoldDirStateCalculator
    {
        public void Calculate(
            bool VABPod,
            Vector3 dir, Vector3 right,
            Vessel vessel
            )
        {
            if (VABPod)
                Calculate(dir, right, vessel.ReferenceTransform, vessel);
            else
                Calculate(
                    dir, right,
                    -vessel.ReferenceTransform.forward,
                    vessel.ReferenceTransform.right,
                    vessel);
        }
    }
}

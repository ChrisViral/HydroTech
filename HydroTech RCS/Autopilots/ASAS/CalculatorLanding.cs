using UnityEngine;

namespace HydroTech_RCS.Autopilots.ASAS
{
    public class LandingStateCalculator : HoldDirStateCalculator
    {
        public void Calculate(bool vabPod, Vector3 dir, Vector3 right, Vessel vessel)
        {
            if (vabPod) { Calculate(dir, right, vessel.ReferenceTransform, vessel); }
            else
            { Calculate(dir, right, -vessel.ReferenceTransform.forward, vessel.ReferenceTransform.right, vessel); }
        }
    }
}
using HydroTech_FC;
using HydroTech_RCS.Panels;
using UnityEngine;

#if DEBUG

namespace HydroTech_RCS.Autopilots.ASAS
{
    public class KillRotationCalculator : CtrlStateCalculator
    {
        public void Calculate(Vector3 angularV, Vector3 transformDir, Vector3 transformRight, Transform vesselTransform)
        {
            Vector3 transformDown = HMaths.CrossProduct(transformDir, transformRight);
            Vector3 _aV = SwitchTransformCalculator.VectorTransform(angularV, transformRight, transformDown, transformDir);

            PanelDebug.ThePanel.AddWatch("_aV", _aV);

            ChangeTransformRotation(transformRight, transformDown, transformDir, vesselTransform);
        }

        public void Calculate(Vessel vessel)
        {
            Calculate(vessel.rigidbody.angularVelocity, vessel.ReferenceTransform.up, vessel.ReferenceTransform.right, vessel.ReferenceTransform);
        }
    }
}

#endif
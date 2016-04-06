using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if DEBUG
namespace HydroTech_RCS.Autopilots.ASAS
{
    using UnityEngine;
    using HydroTech_FC;

    public class KillRotationCalculator : CtrlStateCalculator
    {
        public void Calculate(
            Vector3 angularV,
            Vector3 transformDir, Vector3 transformRight,
            Transform vesselTransform
            )
        {
            Vector3 transformDown = HMaths.CrossProduct(transformDir, transformRight);
            Vector3 _aV = SwitchTransformCalculator.VectorTransform(
                angularV,
                transformRight,
                transformDown,
                transformDir
                );

            Panels.PanelDebug.thePanel.AddWatch("_aV", _aV);

            ChangeTransformRotation(
                transformRight,
                transformDown,
                transformDir,
                vesselTransform
                );
        }

        public void Calculate(Vessel vessel)
        {
            Calculate(
                vessel.rigidbody.angularVelocity,
                vessel.ReferenceTransform.up,
                vessel.ReferenceTransform.right,
                vessel.ReferenceTransform
                );
        }
    }
}
#endif
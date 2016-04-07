using HydroTech_FC;
using HydroTech_RCS.Constants;
using UnityEngine;

namespace HydroTech_RCS.Autopilots.Calculators
{
    public class HoldDirectionCalculator : CtrlStateCalculator
    {
        #region Fields
        public bool reverse;
        #endregion

        #region Methods
        public void Calculate(Vector3 dir, Vector3 right, Vector3 angularV, Vector3 transformDir, Vector3 transformRight, Transform vesselTransform)
        {
            Vector3 transformDown = HMaths.CrossProduct(transformDir, transformRight);
            Vector3 d = SwitchTransformCalculator.VectorTransform(dir, transformRight, transformDown, transformDir);
            Vector3 r = SwitchTransformCalculator.VectorTransform(right, transformRight, transformDown, transformDir);
            Vector3 av = SwitchTransformCalculator.VectorTransform(angularV, transformRight, transformDown, transformDir);
            this.reverse = false;
            if (Steer(AutopilotConsts.reverseDirSin))
            {
                this.reverse = true;
                bool upDown = d.z < 0;
                bool forBack = r.x < 0;
                if (!upDown && forBack) { this.roll = HMaths.Sign(r.y); }
                else if (upDown && forBack) { this.yaw = HMaths.Sign(d.x); }
                else if (upDown) { this.pitch = -HMaths.Sign(d.y); }
                else { this.reverse = false; }
            }

            if (!this.reverse)
            {
                this.yaw = d.x;
                this.pitch = -d.y;
                this.roll = r.y;
                if (av.y * this.yaw > 0) { this.yaw *= AutopilotConsts.rotationHoldRate; }
                if (av.z * this.roll > 0) { this.roll *= AutopilotConsts.rotationHoldRate; }
                if (av.x * this.pitch > 0) { this.pitch *= AutopilotConsts.rotationHoldRate; }
            }

            ChangeTransformRotation(transformRight, transformDown, transformDir, vesselTransform);
        }

        public void Calculate(Vector3 dir, Vector3 right, Vector3 transformDir, Vector3 transformRight, Vessel vessel)
        {
            Calculate(dir, right, vessel.GetComponent<Rigidbody>().angularVelocity, transformDir, transformRight, vessel.ReferenceTransform);
        }

        public void Calculate(Vector3 dir, Vector3 right, Transform transform, Vessel vessel)
        {
            Calculate(dir, right, transform.up, transform.right, vessel);
        }

        public void Calculate(Vector3 dir, Vector3 right, Vessel vessel)
        {
            Calculate(dir, right, vessel.ReferenceTransform, vessel);
        }

        public bool Steer(float maxSteerErrSin)
        {
            return !this.reverse && HMaths.Abs(this.yaw) <= maxSteerErrSin && HMaths.Abs(this.roll) <= maxSteerErrSin && HMaths.Abs(this.pitch) <= maxSteerErrSin;
        }
        #endregion
    }
}
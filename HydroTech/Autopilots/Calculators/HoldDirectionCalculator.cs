using UnityEngine;

namespace HydroTech.Autopilots.Calculators
{
    public class HoldDirectionCalculator : CtrlStateCalculator
    {
        #region Fields
        public bool reverse;
        #endregion

        #region Methods
        public void Calculate(Vector3 dir, Vector3 right, Vector3 angularV, Vector3 transformDir, Vector3 transformRight, Transform vesselTransform)
        {
            Vector3 transformDown = -Vector3.Cross(transformDir, transformRight);
            Vector3 d = SwitchTransformCalculator.VectorTransform(dir, transformRight, transformDown, transformDir);
            Vector3 r = SwitchTransformCalculator.VectorTransform(right, transformRight, transformDown, transformDir);
            Vector3 av = SwitchTransformCalculator.VectorTransform(angularV, transformRight, transformDown, transformDir);
            this.reverse = false;
            if (Steer(0.1f))    //5.74°
            {
                this.reverse = true;
                bool upDown = d.z < 0;
                bool forBack = r.x < 0;
                if (!upDown && forBack) { this.roll = Mathf.Sign(r.y); }
                else if (upDown && forBack) { this.yaw = Mathf.Sign(d.x); }
                else if (upDown) { this.pitch = -Mathf.Sign(d.y); }
                else { this.reverse = false; }
            }

            if (!this.reverse)
            {
                this.yaw = d.x;
                this.pitch = -d.y;
                this.roll = r.y;
                if (av.y * this.yaw > 0) { this.yaw *= 10; }
                if (av.z * this.roll > 0) { this.roll *= 10; }
                if (av.x * this.pitch > 0) { this.pitch *= 10; }
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
            return !this.reverse && Mathf.Abs(this.yaw) <= maxSteerErrSin && Mathf.Abs(this.roll) <= maxSteerErrSin && Mathf.Abs(this.pitch) <= maxSteerErrSin;
        }
        #endregion
    }
}
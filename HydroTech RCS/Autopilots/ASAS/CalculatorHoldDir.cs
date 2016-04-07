#if DEBUG
//#define PRINT_CALCULATION
#endif

using HydroTech_FC;
using HydroTech_RCS.Constants.Autopilots.General;
using UnityEngine;

namespace HydroTech_RCS.Autopilots.ASAS
{
    public class HoldDirStateCalculator : CtrlStateCalculator
    {
        public bool reverse;

        public void Calculate(Vector3 dir, Vector3 right, Vector3 angularV, Vector3 transformDir, Vector3 transformRight, Transform vesselTransform)
        {
#if PRINT_CALCULATION
            print("Calculator:\n"
                + "dir = " + dir.ToString("#0.00") + ",\n"
                + "right = " + right.ToString("#0.00") + ",\n"
                + "aV = " + angularV.ToString("#0.00") + ",\n"
                + "transDir = " + transformDir.ToString("#0.00") + ",\n"
                + "transRight = " + transformRight.ToString("#0.00"));
#endif

            Vector3 transformDown = HMaths.CrossProduct(transformDir, transformRight);

            Vector3 _dir = SwitchTransformCalculator.VectorTransform(dir, transformRight, transformDown, transformDir);
            Vector3 _right = SwitchTransformCalculator.VectorTransform(right, transformRight, transformDown, transformDir);
            Vector3 _aV = SwitchTransformCalculator.VectorTransform(angularV, transformRight, transformDown, transformDir);

#if PRINT_CALCULATION
            print("Calculator:\n"
                + "_dir = " + _dir.ToString("#0.00") + ",\n"
                + "_right = " + _right.ToString("#0.00") + ",\n"
                + "_aV = " + _aV.ToString("#0.00"));
#endif

            this.reverse = false;
            if (Steer(Angle.reverseDirSin))
            {
                this.reverse = true;
                bool upDown = _dir.z < 0;
                bool forBack = _right.x < 0;
                if (!upDown && forBack) { this.roll = HMaths.Sign(_right.y); }
                else if (upDown && forBack) { this.yaw = HMaths.Sign(_dir.x); }
                else if (upDown && !forBack) { this.pitch = -HMaths.Sign(_dir.y); }
                else
                {
                    this.reverse = false;
                }
            }

            if (!this.reverse)
            {
                this.yaw = _dir.x;
                this.pitch = -_dir.y;
                this.roll = _right.y;
                if (_aV.y * this.yaw > 0) { this.yaw *= Rotation.rotationHoldRate; }
                if (_aV.z * this.roll > 0) { this.roll *= Rotation.rotationHoldRate; }
                if (_aV.x * this.pitch > 0) { this.pitch *= Rotation.rotationHoldRate; }
                /*
                yaw = _dir.x * Rotation.KillAngularDiff + _aV.y * Rotation.KillAngularV;
                pitch = -dir.y * Rotation.KillAngularDiff + _aV.x * Rotation.KillAngularV;
                roll = _right.y * Rotation.KillAngularDiff + _aV.z * Rotation.KillAngularV;
                */
            }

#if PRINT_CALCULATION
            print("Calculator:\n"
                + "reverse = " + reverse + ",\n"
                + "yaw = " + yaw.ToString("#0.00") + ",\n"
                + "pitch = " + pitch.ToString("#0.00") + ",\n"
                + "roll = " + roll.ToString("#0.00"));
#endif

            ChangeTransformRotation(transformRight, transformDown, transformDir, vesselTransform);

#if PRINT_CALCULATION
            print("Calculator:\n"
                + "yaw = " + yaw.ToString("#0.00") + ",\n"
                + "pitch = " + pitch.ToString("#0.00") + ",\n"
                + "roll = " + roll.ToString("#0.00"));
#endif
        }

        public void Calculate(Vector3 dir, Vector3 right, Vector3 transformDir, Vector3 transformRight, Vessel vessel)
        {
            Calculate(dir, right, vessel.rigidbody.angularVelocity, transformDir, transformRight, vessel.ReferenceTransform);
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

#if DEBUG
        public override string ToString()
        {
            return "reverse = " + this.reverse + ", yaw = " + this.yaw.ToString("#0.000") + ", roll = " + this.roll.ToString("#0.000") + ", pitch = " + this.pitch.ToString("#0.000");
        }
#endif
    }
}
#if DEBUG
//#define PRINT_CALCULATION
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HydroTech_RCS.Autopilots.ASAS
{
    using UnityEngine;
    using HydroTech_FC;
    using Constants.Autopilots.General;
    using Modules;

    public class HoldDirStateCalculator : CtrlStateCalculator
    {
        public bool reverse = false;

        public void Calculate(
            Vector3 dir, Vector3 right,
            Vector3 angularV,
            Vector3 transformDir, Vector3 transformRight,
            Transform vesselTransform
            )
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

            Vector3 _dir = SwitchTransformCalculator.VectorTransform(
                dir,
                transformRight,
                transformDown,
                transformDir
                );
            Vector3 _right = SwitchTransformCalculator.VectorTransform(
                right,
                transformRight,
                transformDown,
                transformDir
                );
            Vector3 _aV = SwitchTransformCalculator.VectorTransform(
                angularV,
                transformRight,
                transformDown,
                transformDir
                );

#if PRINT_CALCULATION
            print("Calculator:\n"
                + "_dir = " + _dir.ToString("#0.00") + ",\n"
                + "_right = " + _right.ToString("#0.00") + ",\n"
                + "_aV = " + _aV.ToString("#0.00"));
#endif

            reverse = false;
            if (Steer(Angle.ReverseDirSin))
            {
                reverse = true;
                bool UpDown = _dir.z < 0;
                bool ForBack = _right.x < 0;
                if (!UpDown && ForBack)
                    roll = HMaths.Sign(_right.y);
                else if (UpDown && ForBack)
                    yaw = HMaths.Sign(_dir.x);
                else if (UpDown && !ForBack)
                    pitch = -HMaths.Sign(_dir.y);
                else
                    reverse = false;
            }

            if (!reverse)
            {
                yaw = _dir.x;
                pitch = -_dir.y;
                roll = _right.y;
                if (_aV.y * yaw > 0)
                    yaw *= Rotation.RotationHoldRate;
                if (_aV.z * roll > 0)
                    roll *= Rotation.RotationHoldRate;
                if (_aV.x * pitch > 0)
                    pitch *= Rotation.RotationHoldRate;
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

            ChangeTransformRotation(
                transformRight,
                transformDown,
                transformDir,
                vesselTransform
                );

#if PRINT_CALCULATION
            print("Calculator:\n"
                + "yaw = " + yaw.ToString("#0.00") + ",\n"
                + "pitch = " + pitch.ToString("#0.00") + ",\n"
                + "roll = " + roll.ToString("#0.00"));
#endif
        }

        public void Calculate(
            Vector3 dir, Vector3 right,
            Vector3 transformDir, Vector3 transformRight,
            Vessel vessel
            )
        {
            Calculate(
                dir, right, vessel.rigidbody.angularVelocity,
                transformDir, transformRight, vessel.ReferenceTransform
                );
        }

        public void Calculate(
            Vector3 dir, Vector3 right,
            Transform transform,
            Vessel vessel
            )
        {
            Calculate(dir, right, transform.up, transform.right, vessel);
        }

        public void Calculate(
            Vector3 dir, Vector3 right,
            Vessel vessel
            )
        {
            Calculate(dir, right, vessel.ReferenceTransform, vessel);
        }

        public bool Steer(float MaxSteerErrSin)
        {
            return !reverse
                && HMaths.Abs(yaw) <= MaxSteerErrSin
                && HMaths.Abs(roll) <= MaxSteerErrSin
                && HMaths.Abs(pitch) <= MaxSteerErrSin;
        }

#if DEBUG
        public override string ToString()
        {
            return "reverse = " + reverse.ToString()
               + ", yaw = " + yaw.ToString("#0.000")
               + ", roll = " + roll.ToString("#0.000")
               + ", pitch = " + pitch.ToString("#0.000");
        }
#endif
    }
}
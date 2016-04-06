using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HydroTech_FC
{
    using UnityEngine;

    public class CtrlStateCalculator
    {
        public float yaw = 0.0F;
        public float roll = 0.0F;
        public float pitch = 0.0F;
        public float X = 0.0F;
        public float Y = 0.0F;
        public float Z = 0.0F;

        protected void ChangeTransformRotation(
            Vector3 right,
            Vector3 down,
            Vector3 forward,
            Transform transform
            )
        {
            SwitchTransformCalculator sCal = new SwitchTransformCalculator();
            sCal.GetRotation(this);
            sCal.ChangeTransformRotation(right, down, forward, transform);
            sCal.SetRotation(this);
        }

        protected void ChangeTransformTranslation(
            Vector3 right,
            Vector3 down,
            Vector3 forward,
            Transform transform
            )
        {
            SwitchTransformCalculator sCal = new SwitchTransformCalculator();
            sCal.GetTranslation(this);
            sCal.ChangeTransformTranslation(right, down, forward, transform);
            sCal.SetTranslation(this);
        }

        public void RotationMultiplier(float m)
        {
            yaw *= m;
            roll *= m;
            pitch *= m;
        }

        public void TranslationMultiplier(float m)
        {
            X *= m;
            Y *= m;
            Z *= m;
        }

        public void SetCtrlStateRotation(FlightCtrlState ctrlState)
        {
            ctrlState.yaw = yaw;
            ctrlState.roll = roll;
            ctrlState.pitch = pitch;
        }

        public void SetCtrlStateTranslation(FlightCtrlState ctrlState)
        {
            ctrlState.X = X;
            ctrlState.Y = Y;
            ctrlState.Z = Z;
        }

#if DEBUG
        public string PrintRot()
        {
            return "yaw = " + yaw.ToString("#0.000")
                + ", roll = " + roll.ToString("#0.000")
                + ", pitch = " + pitch.ToString("#0.000");
        }

        public string PrintTrans()
        {
            return "X = " + X.ToString("#0.000")
                + ", Y = " + Y.ToString("#0.000")
                + ", Z = " + Z.ToString("#0.000");
        }

        public string Print()
        {
            return "yaw = " + yaw.ToString("#0.000")
                + ", roll = " + roll.ToString("#0.000")
                + ", pitch = " + pitch.ToString("#0.000")
                + "\n"
                + "X = " + X.ToString("#0.000")
                + ", Y = " + Y.ToString("#0.000")
                + ", Z = " + Z.ToString("#0.000");
        }

        public override string ToString()
        {
            return Print();
        }
#endif
    }
}

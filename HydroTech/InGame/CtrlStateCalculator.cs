﻿using UnityEngine;

namespace HydroTech.InGame
{
    public class CtrlStateCalculator
    {
        public float yaw;
        public float roll;
        public float pitch;
        public float x;
        public float y;
        public float z;

        protected void ChangeTransformRotation(Vector3 right, Vector3 down, Vector3 forward, Transform transform)
        {
            SwitchTransformCalculator sCal = new SwitchTransformCalculator();
            sCal.GetRotation(this);
            sCal.ChangeTransformRotation(right, down, forward, transform);
            sCal.SetRotation(this);
        }

        protected void ChangeTransformTranslation(Vector3 right, Vector3 down, Vector3 forward, Transform transform)
        {
            SwitchTransformCalculator sCal = new SwitchTransformCalculator();
            sCal.GetTranslation(this);
            sCal.ChangeTransformTranslation(right, down, forward, transform);
            sCal.SetTranslation(this);
        }

        public void RotationMultiplier(float m)
        {
            this.yaw *= m;
            this.roll *= m;
            this.pitch *= m;
        }

        public void TranslationMultiplier(float m)
        {
            this.x *= m;
            this.y *= m;
            this.z *= m;
        }

        public void SetCtrlStateRotation(FlightCtrlState ctrlState)
        {
            ctrlState.yaw = this.yaw;
            ctrlState.roll = this.roll;
            ctrlState.pitch = this.pitch;
        }

        public void SetCtrlStateTranslation(FlightCtrlState ctrlState)
        {
            ctrlState.X = this.x;
            ctrlState.Y = this.y;
            ctrlState.Z = this.z;
        }

#if DEBUG
        public string PrintRot()
        {
            return "yaw = " + this.yaw.ToString("#0.000")
                + ", roll = " + this.roll.ToString("#0.000")
                + ", pitch = " + this.pitch.ToString("#0.000");
        }

        public string PrintTrans()
        {
            return "X = " + this.x.ToString("#0.000")
                + ", Y = " + this.y.ToString("#0.000")
                + ", Z = " + this.z.ToString("#0.000");
        }

        public string Print()
        {
            return "yaw = " + this.yaw.ToString("#0.000")
                + ", roll = " + this.roll.ToString("#0.000")
                + ", pitch = " + this.pitch.ToString("#0.000")
                + "\n"
                + "X = " + this.x.ToString("#0.000")
                + ", Y = " + this.y.ToString("#0.000")
                + ", Z = " + this.z.ToString("#0.000");
        }

        public override string ToString()
        {
            return Print();
        }
#endif
    }
}
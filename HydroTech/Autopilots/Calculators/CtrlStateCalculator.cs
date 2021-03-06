﻿using UnityEngine;

namespace HydroTech.Autopilots.Calculators
{
    public class CtrlStateCalculator
    {
        #region Fields
        public float pitch, yaw, roll;
        public float x, y, z;
        #endregion

        #region Methods
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
        #endregion

        #region Debug
#if DEBUG
        public string PrintRot()
        {
            return $"ptich: {this.pitch:#0.000}, yaw: {this.yaw:#0.000}, roll: {this.roll:#0.000}";
        }

        public string PrintTrans()
        {
            return $"X: {this.x:#0.000}, Y: {this.y:#0.000}, Z: {this.z:#0.000}";
        }

        public override string ToString()
        {
            return $"ptich: {this.pitch:#0.000}, yaw: {this.yaw:#0.000}, roll: {this.roll:#0.000}\nX: {this.x:#0.000}, Y: {this.y:#0.000}, Z: {this.z:#0.000}";
        }
#endif
        #endregion
    }
}
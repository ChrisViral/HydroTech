using UnityEngine;

namespace HydroTech
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
            return string.Format("ptich: {0:#0.000}, yaw: {1:#0.000}, roll: {2:#0.000}", this.pitch, this.yaw, this.roll);
        }

        public string PrintTrans()
        {
            return string.Format("X: {0:#0.000}, Y: {1:#0.000}, Z: {2:#0.000}", this.x, this.y, this.z);
        }

        public override string ToString()
        {
            return string.Format("ptich: {0:#0.000}, yaw: {1:#0.000}, roll: {2:#0.000}\nX: {3:#0.000}, Y: {4:#0.000}, Z: {5:#0.000}", this.pitch, this.yaw, this.roll, this.x, this.y, this.z);
        }
#endif
        #endregion
    }
}
using UnityEngine;

namespace HydroTech_FC
{
    public class SwitchTransformCalculator
    {
        protected Vector3 state;

        public static Vector3 VectorTransform(Vector3 vec, Vector3 x, Vector3 y, Vector3 z)
        {
            return new Vector3(Vector3.Dot(vec, x), Vector3.Dot(vec, y), Vector3.Dot(vec, z));
        }

        public static Vector3 VectorTransform(Vector3 vec, Transform trans)
        {
            /*  For rotation, input: rotation vector (that you rotate along)
             *  output: (pitch, yaw, roll).
             *  For translation, input: direction vector (that you move along)
             *  output: (X, Y, Z).
             */
            return VectorTransform(vec, trans.right, trans.forward, trans.up);
        }

        public static Vector3 ReverseVectorTransform(Vector3 vec, Vector3 x, Vector3 y, Vector3 z)
        {
            return (vec.x * x) + (vec.y * y) + (vec.z * z);
        }

        public static Vector3 ReverseVectorTransform(Vector3 vec, Transform trans)
        {
            return ReverseVectorTransform(vec, trans.right, trans.forward, trans.up);
        }

        public void ChangeTransformRotation(Vector3 right, Vector3 down, Vector3 forward, Transform transform)
        {
            float yaw = this.state.x;
            float roll = this.state.y;
            float pitch = this.state.z;
            Vector3 yawUnit = VectorTransform(down, transform);
            Vector3 rollUnit = VectorTransform(forward, transform);
            Vector3 pitchUnit = VectorTransform(right, transform);
            this.state.x = (yaw * yawUnit.y) + (roll * rollUnit.y) + (pitch * pitchUnit.y);
            this.state.y = (yaw * yawUnit.z) + (roll * rollUnit.z) + (pitch * pitchUnit.z);
            this.state.z = (yaw * yawUnit.x) + (roll * rollUnit.x) + (pitch * pitchUnit.x);
        }

        public void ChangeTransformTranslation(Vector3 right, Vector3 down, Vector3 forward, Transform transform)
        {
            float x = this.state.x;
            float y = this.state.y;
            float z = this.state.z;
            Vector3 xUnit = VectorTransform(right, transform);
            Vector3 yUnit = VectorTransform(down, transform);
            Vector3 zUnit = VectorTransform(forward, transform);
            this.state.x = (x * xUnit.x) + (y * yUnit.x) + (z * zUnit.x);
            this.state.y = (x * xUnit.y) + (y * yUnit.y) + (z * zUnit.y);
            this.state.z = (x * xUnit.z) + (y * yUnit.z) + (z * zUnit.z);
        }

        public void GetRotation(CtrlStateCalculator cal)
        {
            this.state.x = cal.yaw;
            this.state.y = cal.roll;
            this.state.z = cal.pitch;
        }

        public void GetRotation(FlightCtrlState ctrlState)
        {
            this.state.x = ctrlState.yaw;
            this.state.y = ctrlState.roll;
            this.state.z = ctrlState.pitch;
        }

        public void SetRotation(CtrlStateCalculator cal)
        {
            cal.yaw = this.state.x;
            cal.roll = this.state.y;
            cal.pitch = this.state.z;
        }

        public void SetRotation(FlightCtrlState ctrlState)
        {
            ctrlState.yaw = this.state.x;
            ctrlState.roll = this.state.y;
            ctrlState.pitch = this.state.z;
        }

        public void GetTranslation(CtrlStateCalculator cal)
        {
            this.state.x = cal.x;
            this.state.y = cal.y;
            this.state.z = cal.z;
        }

        public void GetTranslation(FlightCtrlState ctrlState)
        {
            this.state.x = ctrlState.X;
            this.state.y = ctrlState.Y;
            this.state.z = ctrlState.Z;
        }

        public void SetTranslation(CtrlStateCalculator cal)
        {
            cal.x = this.state.x;
            cal.y = this.state.y;
            cal.z = this.state.z;
        }

        public void SetTranslation(FlightCtrlState ctrlState)
        {
            ctrlState.X = this.state.x;
            ctrlState.Y = this.state.y;
            ctrlState.Z = this.state.z;
        }
    }
}
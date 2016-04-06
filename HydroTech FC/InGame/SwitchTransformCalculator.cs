using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HydroTech_FC
{
    using UnityEngine;

    public class SwitchTransformCalculator
    {
        protected Vector3 state = new Vector3();

        public static Vector3 VectorTransform(Vector3 vec, Vector3 x, Vector3 y, Vector3 z)
        {
            return new Vector3(
                HMaths.DotProduct(vec, x),
                HMaths.DotProduct(vec, y),
                HMaths.DotProduct(vec, z)
                );
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
            return vec.x * x + vec.y * y + vec.z * z;
        }
        public static Vector3 ReverseVectorTransform(Vector3 vec, Transform trans)
        {
            return ReverseVectorTransform(vec, trans.right, trans.forward, trans.up);
        }

        public void ChangeTransformRotation(
            Vector3 right,
            Vector3 down,
            Vector3 forward,
            Transform transform
            )
        {
            float yaw = state.x;
            float roll = state.y;
            float pitch = state.z;
            Vector3 YawUnit = VectorTransform(down, transform);
            Vector3 RollUnit = VectorTransform(forward, transform);
            Vector3 PitchUnit = VectorTransform(right, transform);
            state.x = yaw * YawUnit.y + roll * RollUnit.y + pitch * PitchUnit.y;
            state.y = yaw * YawUnit.z + roll * RollUnit.z + pitch * PitchUnit.z;
            state.z = yaw * YawUnit.x + roll * RollUnit.x + pitch * PitchUnit.x;
        }

        public void ChangeTransformTranslation(
            Vector3 right,
            Vector3 down,
            Vector3 forward,
            Transform transform
            )
        {
            float X = state.x;
            float Y = state.y;
            float Z = state.z;
            Vector3 XUnit = VectorTransform(right, transform);
            Vector3 YUnit = VectorTransform(down, transform);
            Vector3 ZUnit = VectorTransform(forward, transform);
            state.x = X * XUnit.x + Y * YUnit.x + Z * ZUnit.x;
            state.y = X * XUnit.y + Y * YUnit.y + Z * ZUnit.y;
            state.z = X * XUnit.z + Y * YUnit.z + Z * ZUnit.z;
        }

        public void GetRotation(CtrlStateCalculator Cal)
        {
            state.x = Cal.yaw;
            state.y = Cal.roll;
            state.z = Cal.pitch;
        }

        public void GetRotation(FlightCtrlState ctrlState)
        {
            state.x = ctrlState.yaw;
            state.y = ctrlState.roll;
            state.z = ctrlState.pitch;
        }

        public void SetRotation(CtrlStateCalculator Cal)
        {
            Cal.yaw = state.x;
            Cal.roll = state.y;
            Cal.pitch = state.z;
        }

        public void SetRotation(FlightCtrlState ctrlState)
        {
            ctrlState.yaw = state.x;
            ctrlState.roll = state.y;
            ctrlState.pitch = state.z;
        }

        public void GetTranslation(CtrlStateCalculator Cal)
        {
            state.x = Cal.X;
            state.y = Cal.Y;
            state.z = Cal.Z;
        }

        public void GetTranslation(FlightCtrlState ctrlState)
        {
            state.x = ctrlState.X;
            state.y = ctrlState.Y;
            state.z = ctrlState.Z;
        }

        public void SetTranslation(CtrlStateCalculator Cal)
        {
            Cal.X = state.x;
            Cal.Y = state.y;
            Cal.Z = state.z;
        }

        public void SetTranslation(FlightCtrlState ctrlState)
        {
            ctrlState.X = state.x;
            ctrlState.Y = state.y;
            ctrlState.Z = state.z;
        }
    }
}

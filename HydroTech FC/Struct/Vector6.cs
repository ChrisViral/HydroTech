using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HydroTech_FC
{
    using UnityEngine;

    public class Vector6
    {
        public Vector6() { Reset(); }
        public Vector6(Vector6 vec)
        {
            xp = vec.xp;
            xn = vec.xn;
            yp = vec.yp;
            yn = vec.yn;
            zp = vec.zp;
            zn = vec.zn;
        }
        public Vector6(Vector3 vectorP, Vector3 vectorN)
        {
            VectorPositive = vectorP;
            VectorNegative = vectorN;
        }

        public float xp, xn;
        public float yp, yn;
        public float zp, zn;

        public Vector3 VectorPositive
        {
            get { return new Vector3(xp, yp, zp); }
            set
            {
                xp = value.x;
                yp = value.y;
                zp = value.z;
            }
        }
        public Vector3 VectorNegative
        {
            get { return new Vector3(xn, yn, zn); }
            set
            {
                xn = value.x;
                yn = value.y;
                zn = value.z;
            }
        }

        public void Reset() { xp = xn = yp = yn = zp = zn = 0.0F; }

        public void AddX(float x)
        {
            if (x >= 0.0F) xp += x;
            else xn -= x;
        }
        public void AddY(float y)
        {
            if (y >= 0.0F) yp += y;
            else yn -= y;
        }
        public void AddZ(float z)
        {
            if (z >= 0.0F) zp += z;
            else zn -= z;
        }
        public static Vector6 operator *(Vector6 vec, float num)
        {
            return new Vector6(
                vec.VectorPositive * num,
                vec.VectorNegative * num
                );
        }
        public static Vector6 operator /(Vector6 vec, float num)
        {
            return vec * (1 / num);
        }
        public static Vector6 operator *(Vector6 vec, Vector3 vec2)
        {
            Vector6 res = new Vector6(vec);
            res.xp *= vec2.x;
            res.xn *= vec2.x;
            res.yp *= vec2.y;
            res.yn *= vec2.y;
            res.zp *= vec2.z;
            res.zn *= vec2.z;
            return res;
        }
        public static Vector6 operator /(Vector6 vec, Vector3 vec2)
        {
            return vec * new Vector3(1 / vec2.x, 1 / vec2.y, 1 / vec2.y);
        }

        public override string ToString()
        {
            return "(" + xp.ToString() + ", -" + xn.ToString() + "; "
                + yp.ToString() + ", -" + yn.ToString() + "; "
                + zp.ToString() + ", -" + zn.ToString() + ")";
        }
        public virtual string ToString(string format)
        {
            return "(" + xp.ToString(format) + ", -" + xn.ToString(format) + "; "
                + yp.ToString(format) + ", -" + yn.ToString(format) + "; "
                + zp.ToString(format) + ", -" + zn.ToString(format) + ")";
        }
    }
}

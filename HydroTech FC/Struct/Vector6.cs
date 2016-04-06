using UnityEngine;

namespace HydroTech_FC
{
    public class Vector6
    {
        public float xp, xn;
        public float yp, yn;
        public float zp, zn;

        public Vector3 VectorPositive
        {
            get { return new Vector3(this.xp, this.yp, this.zp); }
            set
            {
                this.xp = value.x;
                this.yp = value.y;
                this.zp = value.z;
            }
        }

        public Vector3 VectorNegative
        {
            get { return new Vector3(this.xn, this.yn, this.zn); }
            set
            {
                this.xn = value.x;
                this.yn = value.y;
                this.zn = value.z;
            }
        }

        public Vector6()
        {
            Reset();
        }

        public Vector6(Vector6 vec)
        {
            this.xp = vec.xp;
            this.xn = vec.xn;
            this.yp = vec.yp;
            this.yn = vec.yn;
            this.zp = vec.zp;
            this.zn = vec.zn;
        }

        public Vector6(Vector3 vectorP, Vector3 vectorN)
        {
            this.VectorPositive = vectorP;
            this.VectorNegative = vectorN;
        }

        public void Reset()
        {
            this.xp = this.xn = this.yp = this.yn = this.zp = this.zn = 0.0F;
        }

        public void AddX(float x)
        {
            if (x >= 0.0F) { this.xp += x; }
            else
            { this.xn -= x; }
        }

        public void AddY(float y)
        {
            if (y >= 0.0F) { this.yp += y; }
            else
            { this.yn -= y; }
        }

        public void AddZ(float z)
        {
            if (z >= 0.0F) { this.zp += z; }
            else
            { this.zn -= z; }
        }

        public static Vector6 operator *(Vector6 vec, float num)
        {
            return new Vector6(vec.VectorPositive * num, vec.VectorNegative * num);
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
            return "(" + this.xp + ", -" + this.xn + "; " + this.yp + ", -" + this.yn + "; " + this.zp + ", -" + this.zn + ")";
        }

        public virtual string ToString(string format)
        {
            return "(" + this.xp.ToString(format) + ", -" + this.xn.ToString(format) + "; " + this.yp.ToString(format) + ", -" + this.yn.ToString(format) + "; " + this.zp.ToString(format) + ", -" + this.zn.ToString(format) + ")";
        }
    }
}
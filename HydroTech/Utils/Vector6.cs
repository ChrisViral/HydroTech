using UnityEngine;

namespace HydroTech.Utils
{
    public struct Vector6
    {
        #region Fields
        public float xp, xn;
        public float yp, yn;
        public float zp, zn;
        #endregion

        #region Properties
        public Vector3 Positive
        {
            get { return new Vector3(this.xp, this.yp, this.zp); }
            set
            {
                this.xp = value.x;
                this.yp = value.y;
                this.zp = value.z;
            }
        }

        public Vector3 Negative
        {
            get { return new Vector3(this.xn, this.yn, this.zn); }
            set
            {
                this.xn = value.x;
                this.yn = value.y;
                this.zn = value.z;
            }
        }
        #endregion

        #region Constructors
        public Vector6(Vector3 vectorP, Vector3 vectorN)
        {
            this.xp = vectorP.x; this.xn = vectorN.x;
            this.yp = vectorP.y; this.yn = vectorN.y;
            this.zp = vectorP.z; this.zn = vectorN.z;
        }
        #endregion

        #region Methods
        public void Reset()
        {
            this.xp = this.xn = 0;
            this.yp = this.yn = 0;
            this.zp = this.zn = 0;
        }

        public void AddX(float x)
        {
            if (x > 0) { this.xp += x; }
            else { this.xn -= x; }
        }

        public void AddY(float y)
        {
            if (y > 0) { this.yp += y; }
            else { this.yn -= y; }
        }

        public void AddZ(float z)
        {
            if (z > 0) { this.zp += z; }
            else { this.zn -= z; }
        }
        #endregion

        #region Overrides
        public override string ToString()
        {
            return $"({this.xp}, -{this.xn}; {this.yp}, -{this.yn}; {this.zp}, -{this.zn})";
        }

        public string ToString(string format)
        {
            return $"({this.xp.ToString(format)}, -{this.xn.ToString(format)}; {this.yp.ToString(format)}, -{this.yn.ToString(format)}; {this.zp.ToString(format)}, -{this.zn.ToString(format)})";
        }
        #endregion

        #region Operators
        public static Vector6 operator *(Vector6 v, float f)
        {
            return new Vector6(v.Positive * f, v.Negative * f);
        }

        public static Vector6 operator *(Vector6 v1, Vector3 v2)
        {
            Vector6 result = v1;
            result.xp *= v2.x; result.xn *= v2.x;
            result.yp *= v2.y; result.yn *= v2.y;
            result.zp *= v2.z; result.zn *= v2.z;
            return result;
        }

        public static Vector6 operator /(Vector6 v, float f)
        {
            f = 1 / f;
            return new Vector6(v.Positive * f, v.Negative * f);
        }

        public static Vector6 operator /(Vector6 v1, Vector3 v2)
        {
            Vector6 result = v1;
            v2.Set(1 / v2.x, 1 / v2.y, 1 / v2.y);
            result.xp *= v2.x; result.xn *= v2.x;
            result.yp *= v2.y; result.yn *= v2.y;
            result.zp *= v2.z; result.zn *= v2.z;
            return result;
        }
        #endregion
    }
}
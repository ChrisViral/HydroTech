using System;
using UnityEngine;

namespace HydroTech.Utils
{
    public struct Vector6 : IEquatable<Vector6>
    {
        #region Static properties
        public static Vector6 Zero { get; } = new Vector6();

        public static Vector6 One { get; } = new Vector6 { xp = 1, yp = 1, zp = 1 };
        #endregion

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

        public override bool Equals(object obj)
        {
            return obj is Vector6 && Equals((Vector6)obj);
        }

        public bool Equals(Vector6 other)
        {
            return this.xp.Equals(other.xp) && this.xn.Equals(other.xn)
                && this.yp.Equals(other.xp) && this.yn.Equals(other.xn)
                && this.zp.Equals(other.xp) && this.zn.Equals(other.xn);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = this.xp.GetHashCode();
                hashCode = (hashCode * 397) ^ this.xn.GetHashCode();
                hashCode = (hashCode * 397) ^ this.yp.GetHashCode();
                hashCode = (hashCode * 397) ^ this.yn.GetHashCode();
                hashCode = (hashCode * 397) ^ this.zp.GetHashCode();
                hashCode = (hashCode * 397) ^ this.zn.GetHashCode();
                return hashCode;
            }
        }
        #endregion

        #region Operators
        public static explicit operator Vector3(Vector6 v)
        {
            return new Vector3(v.xp + v.xn, v.yp + v.yn, v.zp + v.zn);
        }

        public static explicit operator Vector6(Vector3 v)
        {
            Vector6 result = new Vector6();
            result.AddX(v.x);
            result.AddY(v.y);
            result.AddZ(v.z);
            return result;
        }

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

        public static bool operator ==(Vector6 v1, Vector6 v2)
        {
            return v1.xp == v2.xp && v1.xn == v2.xn
                && v1.yp == v2.xp && v1.yn == v2.xn
                && v1.zp == v2.xp && v1.zn == v2.xn;
        }

        public static bool operator !=(Vector6 v1, Vector6 v2)
        {
            return v1.xp != v2.xp || v1.xn != v2.xn
                || v1.yp != v2.xp || v1.yn != v2.xn
                || v1.zp != v2.xp || v1.zn != v2.xn;
        }
        #endregion
    }
}
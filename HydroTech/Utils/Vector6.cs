using System;
using UnityEngine;

namespace HydroTech.Utils
{
    public struct Vector6 : IEquatable<Vector6>
    {
        #region Static properties
        /// <summary>
        /// Zero vector
        /// </summary>
        public static Vector6 Zero { get; } = new Vector6();

        /// <summary>
        /// Positive one vector
        /// </summary>
        public static Vector6 One { get; } = new Vector6 { xp = 1, yp = 1, zp = 1 };
        #endregion

        #region Fields
        public float xp, xn;    //X components
        public float yp, yn;    //Y components
        public float zp, zn;    //Z components
        #endregion

        #region Properties
        /// <summary>
        /// The positive components of the vector
        /// </summary>
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

        /// <summary>
        /// The negative components of the vector
        /// </summary>
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
        /// <summary>
        /// Creates a new Vector6 from a positive and negative components vectosrs
        /// </summary>
        /// <param name="positive">Positive components vector</param>
        /// <param name="negative">Negative components vector</param>
        public Vector6(Vector3 positive, Vector3 negative)
        {
            this.xp = positive.x; this.xn = negative.x;
            this.yp = positive.y; this.yn = negative.y;
            this.zp = positive.z; this.zn = negative.z;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Resets the Vector6 to the zero vector
        /// </summary>
        public void Reset()
        {
            this.xp = this.xn = 0;
            this.yp = this.yn = 0;
            this.zp = this.zn = 0;
        }

        /// <summary>
        /// Adds the amount to the correct x component
        /// </summary>
        /// <param name="x">Amount to add to the x component</param>
        public void AddX(float x)
        {
            if (x > 0) { this.xp += x; }
            else { this.xn -= x; }
        }

        /// <summary>
        /// Adds the amount to the correct y component
        /// </summary>
        /// <param name="y">Amount to add to the y component</param>
        public void AddY(float y)
        {
            if (y > 0) { this.yp += y; }
            else { this.yn -= y; }
        }

        /// <summary>
        /// Adds the amount to the correct z component
        /// </summary>
        /// <param name="z">Amount to add to the z component</param>
        public void AddZ(float z)
        {
            if (z > 0) { this.zp += z; }
            else { this.zn -= z; }
        }
        #endregion

        #region Overrides
        /// <summary>
        /// String format of this vector
        /// </summary>
        /// <returns>This vector's string representation</returns>
        public override string ToString()
        {
            return $"({this.xp}, -{this.xn}; {this.yp}, -{this.yn}; {this.zp}, -{this.zn})";
        }

        /// <summary>
        /// String format of this vector, formatted in the given way
        /// </summary>
        /// <param name="format">Format of the numbers in the string</param>
        /// <returns>This vector's string representation in the specified format</returns>
        public string ToString(string format)
        {
            return $"({this.xp.ToString(format)}, -{this.xn.ToString(format)}; {this.yp.ToString(format)}, -{this.yn.ToString(format)}; {this.zp.ToString(format)}, -{this.zn.ToString(format)})";
        }
        
        /// <summary>
        /// Equality comparison between the instance and the passed object
        /// </summary>
        /// <param name="obj">Object to compare to</param>
        /// <returns>If the object is equal to the instance</returns>
        public override bool Equals(object obj)
        {
            return obj is Vector6 && Equals((Vector6)obj);
        }

        /// <summary>
        /// Equality comparison between the instance and the passed Vector6
        /// </summary>
        /// <param name="other">Vector6 to compare to</param>
        /// <returns>If the vector is equal to the instance</returns>
        public bool Equals(Vector6 other)
        {
            return this.xp.Equals(other.xp) && this.xn.Equals(other.xn)
                && this.yp.Equals(other.xp) && this.yn.Equals(other.xn)
                && this.zp.Equals(other.xp) && this.zn.Equals(other.xn);
        }

        /// <summary>
        /// Hashing function of the vector
        /// </summary>
        /// <returns>Vector's hashcode, based off the internal elements</returns>
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
        /// <summary>
        /// Converts the given Vector6 to it's Vector3 equivalent
        /// </summary>
        /// <param name="v">Vector6 to convert to Vector3</param>
        public static explicit operator Vector3(Vector6 v)
        {
            return new Vector3(v.xp + v.xn, v.yp + v.yn, v.zp + v.zn);
        }

        /// <summary>
        /// Converts the given Vector3 to it's Vector6 equivalent
        /// </summary>
        /// <param name="v">Vector3 to convert to Vector3</param>
        public static explicit operator Vector6(Vector3 v)
        {
            Vector6 result = new Vector6();
            result.AddX(v.x);
            result.AddY(v.y);
            result.AddZ(v.z);
            return result;
        }

        /// <summary>
        /// Multiplies all the elements of the Vector6 by the given amount
        /// </summary>
        /// <param name="v">Vector6 to multiply</param>
        /// <param name="f">Amount to multiply by</param>
        /// <returns>The resulting Vector6</returns>
        public static Vector6 operator *(Vector6 v, float f)
        {
            return new Vector6(v.Positive * f, v.Negative * f);
        }

        /// <summary>
        /// Multiplies all the elements of the Vector6 by the given amounts for each components
        /// </summary>
        /// <param name="v1">Vector6 to multiply</param>
        /// <param name="v2">Amount vector to multiply by</param>
        /// <returns>The resulting Vector6</returns>
        public static Vector6 operator *(Vector6 v1, Vector3 v2)
        {
            Vector6 result = v1;
            result.xp *= v2.x; result.xn *= v2.x;
            result.yp *= v2.y; result.yn *= v2.y;
            result.zp *= v2.z; result.zn *= v2.z;
            return result;
        }

        /// <summary>
        /// Divides all the elements of the Vector6 by the given amount
        /// </summary>
        /// <param name="v">Vector6 to divide</param>
        /// <param name="f">Amount to divide by</param>
        /// <returns>The resulting Vector6</returns>
        public static Vector6 operator /(Vector6 v, float f)
        {
            return new Vector6(v.Positive / f, v.Negative / f);
        }

        /// <summary>
        /// Divides all the elements of the Vector6 by the given amounts for each components
        /// </summary>
        /// <param name="v1">Vector6 to divide</param>
        /// <param name="v2">Amount vector to divide by</param>
        /// <returns>The resulting Vector6</returns>
        public static Vector6 operator /(Vector6 v1, Vector3 v2)
        {
            Vector6 result = v1;
            result.xp /= v2.x; result.xn /= v2.x;
            result.yp /= v2.y; result.yn /= v2.y;
            result.zp /= v2.z; result.zn /= v2.z;
            return result;
        }

        /// <summary>
        /// Equality comparison between two vectors
        /// </summary>
        /// <param name="v1">First vector to compare</param>
        /// <param name="v2">Second vector to compare</param>
        /// <returns>If the two vectors are equal</returns>
        public static bool operator ==(Vector6 v1, Vector6 v2)
        {
            return v1.xp == v2.xp && v1.xn == v2.xn
                && v1.yp == v2.xp && v1.yn == v2.xn
                && v1.zp == v2.xp && v1.zn == v2.xn;
        }

        /// <summary>
        /// Inequality comparison between two vectors
        /// </summary>
        /// <param name="v1">First vector to compare</param>
        /// <param name="v2">Second vector to compare</param>
        /// <returns>If the two vectors are inequal</returns>
        public static bool operator !=(Vector6 v1, Vector6 v2)
        {
            return v1.xp != v2.xp || v1.xn != v2.xn
                || v1.yp != v2.xp || v1.yn != v2.xn
                || v1.zp != v2.xp || v1.zn != v2.xn;
        }
        #endregion
    }
}
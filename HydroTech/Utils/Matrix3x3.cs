using System;
using UnityEngine;

namespace HydroTech.Utils
{
    //ReSharper disable once InconsistentNaming
    public struct Matrix3x3 : IEquatable<Matrix3x3>
    {
        #region Static properties
        /// <summary>
        /// The Zero (empty) 3x3 Matrix
        /// </summary>
        public static Matrix3x3 Zero { get; } = new Matrix3x3();

        /// <summary>
        /// The Identity 3x3 matrix
        /// | 1 0 0 |
        /// | 0 1 0 |
        /// | 0 0 1 |
        /// </summary>
        public static Matrix3x3 Identity { get; } = new Matrix3x3 { m00 = 1, m11 = 1, m22 = 1 };
        #endregion

        #region Fields
        public float m00, m01, m02; //First row
        public float m10, m11, m12; //Second row
        public float m20, m21, m22; //third row
        #endregion

        #region Properties
        /// <summary>
        /// The main diagonal of the matrix
        /// </summary>
        public Vector3 Diagonal
        {
            get { return new Vector3(this.m00, this.m11, this.m22); }
            set
            {
                this.m00 = value.x;
                this.m11 = value.y;
                this.m22 = value.z;
            }
        }

        /// <summary>
        /// The first column of the matrix
        /// </summary>
        public Vector3 ColumnX
        {
            get { return new Vector3(this.m00, this.m10, this.m20); }
            set
            {
                this.m00 = value.x;
                this.m10 = value.y;
                this.m20 = value.z;
            }
        }

        /// <summary>
        /// The second column of the matrix
        /// </summary>
        public Vector3 ColumnY
        {
            get { return new Vector3(this.m01, this.m11, this.m21); }
            set
            {
                this.m01 = value.x;
                this.m11 = value.y;
                this.m21 = value.z;
            }
        }

        /// <summary>
        /// The third column of the matrix
        /// </summary>
        public Vector3 ColumnZ
        {
            get { return new Vector3(this.m02, this.m12, this.m22); }
            set
            {
                this.m02 = value.x;
                this.m12 = value.y;
                this.m22 = value.z;
            }
        }

        /// <summary>
        /// The first row of the matrix
        /// </summary>
        public Vector3 RowX
        {
            get { return new Vector3(this.m00, this.m01, this.m02); }
            set
            {
                this.m00 = value.x;
                this.m01 = value.y;
                this.m02 = value.z;
            }
        }

        /// <summary>
        /// The second row of the matrix
        /// </summary>
        public Vector3 RowY
        {
            get { return new Vector3(this.m10, this.m11, this.m12); }
            set
            {
                this.m10 = value.x;
                this.m11 = value.y;
                this.m12 = value.z;
            }
        }

        /// <summary>
        /// The third row of the matrix
        /// </summary>
        public Vector3 RowZ
        {
            get { return new Vector3(this.m20, this.m21, this.m22); }
            set
            {
                this.m20 = value.x;
                this.m21 = value.y;
                this.m22 = value.z;
            }
        }

        /// <summary>
        /// The inverse of the current matrix
        /// </summary>
        public Matrix3x3 Inverse => Invert(this);
        #endregion

        #region Methods
        /// <summary>
        /// Resets the Matrix3x3 to the zero matrix
        /// </summary>
        public void Reset()
        {
            this.m00 = this.m01 = this.m02 = 0;
            this.m10 = this.m11 = this.m12 = 0;
            this.m20 = this.m21 = this.m22 = 0;
        }
        #endregion

        #region Static methods
        /// <summary>
        /// Obtains the inverse (A^−1) matrix. This is not a "simple" operation
        /// </summary>
        /// <returns>The inverse matrix of this instance</returns>
        public static Matrix3x3 Invert(Matrix3x3 matrix)
        {
            Matrix3x3 result = Identity;
            /*  We start with
             *  m00 m01 m02 | 1   0   0
             *  m10 m11 m12 | 0   1   0
             *  m20 m21 m22 | 0   0   1 */
            if (matrix.m00 == 0)
            {
                if (matrix.m10 == 0)
                {
                    if (matrix.m20 == 0) { throw new InvalidOperationException("Matrix3x3 Inverse failed at column 0"); }
                    result.RowX += result.RowZ;
                    matrix.RowX += matrix.RowZ;
                }
                else
                {
                    result.RowX += result.RowY;
                    matrix.RowX += matrix.RowY;
                }
            }
            result.RowX /= matrix.m00;
            matrix.RowX /= matrix.m00;

            /*  1   m01 m02 | m00 0   0
             *  m10 m11 m12 | 0   1   0
             *  m20 m21 m22 | 0   0   1 */
            result.RowY -= result.RowX * matrix.m10;
            matrix.RowY -= matrix.RowX * matrix.m10;
            result.RowZ -= result.RowX * matrix.m20;
            matrix.RowZ -= matrix.RowX * matrix.m20;

            /*  1   m01 m02 | m00 0   0
             *  0   m11 m12 | m10 1   0
             *  0   m21 m22 | m20 0   1 */
            if (matrix.m11 == 0)
            {
                if (matrix.m21 == 0) { throw new InvalidOperationException("Matrix3x3 Inverse failed at column 1"); }
                result.RowY += result.RowZ;
                matrix.RowY += matrix.RowZ;
            }
            result.RowY /= matrix.m11;
            matrix.RowY /= matrix.m11;

            /*  1   m01 m02 | m00 0   0
             *  0   1   m12 | m10 m11 0
             *  0   m21 m22 | m20 0   1 */
            result.RowX -= result.RowY * matrix.m01;
            matrix.RowX -= matrix.RowY * matrix.m01;
            result.RowZ -= result.RowY * matrix.m21;
            matrix.RowZ -= matrix.RowY * matrix.m21;

            /*  1   0   m02 | m00 m01 m02
             *  0   1   m12 | m10 m11 0
             *  0   0   m22 | m20 m21 1 */
            if (matrix.m22 == 0) { throw new InvalidOperationException("Matrix3x3 Inverse failed at column 2"); }
            result.RowZ /= matrix.m22;
            matrix.RowZ /= matrix.m22;

            /*  1   0   m02 | m00 m01 m02
             *  0   1   m12 | m10 m11 0
             *  0   0   1   | m20 m21 m22 */
            result.RowX -= result.RowZ * matrix.m02;
            matrix.RowX -= matrix.RowZ * matrix.m02;
            result.RowY -= result.RowZ * matrix.m12;
            matrix.RowY -= matrix.RowZ * matrix.m12;

            /*  1   0   0  |  m00 m01 m02
             *  0   1   0  |  m10 m11 m12
             *  0   0   1  |  m20 m21 m22 */
            if (matrix != Identity) { throw new InvalidOperationException("Matrix3x3 Inverse failed at final check"); }
            return result;
        }
        #endregion

        #region Overrides
        /// <summary>
        /// String format of this 3x3 matrix
        /// </summary>
        /// <returns>This matrix's string representation</returns>
        public override string ToString()
        {
            return $"| {this.m00} {this.m01} {this.m02} |\n| {this.m10} {this.m11} {this.m12} |\n| {this.m20} {this.m21} {this.m22} |";
        }

        /// <summary>
        /// String format of this 3x3 matrix, formatted in the given way
        /// </summary>
        /// <param name="format">Format of the numbers in the string</param>
        /// <returns>This matrix's string representation in the specified format</returns>
        public string ToString(string format)
        {
            return $"| {this.m00.ToString(format)} {this.m01.ToString(format)} {this.m02.ToString(format)} |\n| {this.m10.ToString(format)} {this.m11.ToString(format)} {this.m12.ToString(format)} |\n| {this.m20.ToString(format)} {this.m21.ToString(format)} {this.m22.ToString(format)} |";
        }

        /// <summary>
        /// Equality comparison between the instance and the passed object
        /// </summary>
        /// <param name="obj">Object to compare to</param>
        /// <returns>If the object is equal to the instance</returns>
        public override bool Equals(object obj)
        {
            return obj is Matrix3x3 && Equals((Matrix3x3)obj);
        }

        /// <summary>
        /// Equality comparison between the instance and the passed Matrix3x3
        /// </summary>
        /// <param name="other">Matrix3x3 to compare to</param>
        /// <returns>If the matrix is equal to the instance</returns>
        public bool Equals(Matrix3x3 other)
        {
            return this.m00.Equals(other.m00) && this.m01.Equals(other.m01) && this.m02.Equals(other.m02)
                && this.m10.Equals(other.m10) && this.m11.Equals(other.m11) && this.m12.Equals(other.m12)
                && this.m20.Equals(other.m20) && this.m21.Equals(other.m21) && this.m22.Equals(other.m22);
        }

        /// <summary>
        /// Hashing function of the matrix
        /// </summary>
        /// <returns>Matrix's hashcode, based off the internal elements</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = this.m00.GetHashCode();
                hashCode = (hashCode * 397) ^ this.m01.GetHashCode();
                hashCode = (hashCode * 397) ^ this.m02.GetHashCode();
                hashCode = (hashCode * 397) ^ this.m10.GetHashCode();
                hashCode = (hashCode * 397) ^ this.m11.GetHashCode();
                hashCode = (hashCode * 397) ^ this.m12.GetHashCode();
                hashCode = (hashCode * 397) ^ this.m20.GetHashCode();
                hashCode = (hashCode * 397) ^ this.m21.GetHashCode();
                hashCode = (hashCode * 397) ^ this.m22.GetHashCode();
                return hashCode;
            }
        }
        #endregion

        #region Operators
        /// <summary>
        /// Matrix multiplication between 3x3 matrix and a 3 component vector
        /// </summary>
        /// <param name="m">Matrix to multiply</param>
        /// <param name="v">Vector to multiply</param>
        /// <returns>The resulting multiplied vector</returns>
        public static Vector3 operator *(Matrix3x3 m, Vector3 v)
        {
            return new Vector3(Vector3.Dot(m.RowX, v), Vector3.Dot(m.RowY, v), Vector3.Dot(m.RowZ, v));
        }

        /// <summary>
        /// Matrix inversed multiplication between a 3x3 matrix and a 3 component vector
        /// </summary>
        /// <param name="m">Matrix to multiply</param>
        /// <param name="v">Vector to multiply</param>
        /// <returns>The resulting inverse multiplied vector</returns>
        public static Vector3 operator /(Vector3 v, Matrix3x3 m)
        {
            m = m.Inverse;
            return new Vector3(Vector3.Dot(m.RowX, v), Vector3.Dot(m.RowY, v), Vector3.Dot(m.RowZ, v));
        }

        /// <summary>
        /// Equality comparison between two matrices
        /// </summary>
        /// <param name="m1">First matrix to compare</param>
        /// <param name="m2">Second matrix to compare</param>
        /// <returns>If the two matrices are equal</returns>
        public static bool operator ==(Matrix3x3 m1, Matrix3x3 m2)
        {
            return m1.m00 == m2.m00 && m1.m01 == m2.m01 && m1.m02 == m2.m02
                && m1.m10 == m2.m10 && m1.m11 == m2.m11 && m1.m12 == m2.m12
                && m1.m20 == m2.m20 && m1.m21 == m2.m21 && m1.m22 == m2.m22;
        }

        /// <summary>
        /// Inequality comparison between two matrices
        /// </summary>
        /// <param name="m1">First matrix to compare</param>
        /// <param name="m2">Second matrix to compare</param>
        /// <returns>If the two matrices are inequal</returns>
        public static bool operator !=(Matrix3x3 m1, Matrix3x3 m2)
        {
            return m1.m00 != m2.m00 || m1.m01 != m2.m01 || m1.m02 != m2.m02
                || m1.m10 != m2.m10 || m1.m11 != m2.m11 || m1.m12 != m2.m12
                || m1.m20 != m2.m20 || m1.m21 != m2.m21 || m1.m22 != m2.m22;
        }
        #endregion
    }
}
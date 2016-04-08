using System;
using UnityEngine;

namespace HydroTech_RCS.Utils
{
    public class Matrix3X3
    {
        #region Static properties
        private static readonly Matrix3X3 i = new Matrix3X3 { m00 = 1, m11 = 1, m22 = 1 };
        public static Matrix3X3 I
        {
            get { return i; }
        }
        #endregion

        #region Fields
        public float m00, m01, m02;
        public float m10, m11, m12;
        public float m20, m21, m22;
        #endregion

        #region Properties
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

        public Matrix3X3 InverseMatrix
        {
            get
            {
                Matrix3X3 thisMatrix = new Matrix3X3(this);
                Matrix3X3 resMatrix = new Matrix3X3(I);
                /*  We start with
                 *  m00 m01 m02 | 1   0   0
                 *  m10 m11 m12 | 0   1   0
                 *  m20 m21 m22 | 0   0   1 */
                if (thisMatrix.m00 == 0)
                {
                    if (thisMatrix.m10 == 0)
                    {
                        if (thisMatrix.m20 == 0) { throw new InvalidOperationException("Matrix3x3.InverseMatrix failed at column 0"); }
                        resMatrix.RowX += resMatrix.RowZ;
                        thisMatrix.RowX += thisMatrix.RowZ;
                    }
                    else
                    {
                        resMatrix.RowX += resMatrix.RowY;
                        thisMatrix.RowX += thisMatrix.RowY;
                    }
                }
                resMatrix.RowX /= thisMatrix.m00;
                thisMatrix.RowX /= thisMatrix.m00;

                /*  1   m01 m02 | m00 0   0
                 *  m10 m11 m12 | 0   1   0
                 *  m20 m21 m22 | 0   0   1 */
                resMatrix.RowY -= resMatrix.RowX * thisMatrix.m10;
                thisMatrix.RowY -= thisMatrix.RowX * thisMatrix.m10;
                resMatrix.RowZ -= resMatrix.RowX * thisMatrix.m20;
                thisMatrix.RowZ -= thisMatrix.RowX * thisMatrix.m20;

                /*  1   m01 m02 | m00 0   0
                 *  0   m11 m12 | m10 1   0
                 *  0   m21 m22 | m20 0   1 */
                if (thisMatrix.m11 == 0)
                {
                    if (thisMatrix.m21 == 0) { throw new InvalidOperationException("Matrix3x3.InverseMatrix failed at column 1"); }
                    resMatrix.RowY += resMatrix.RowZ;
                    thisMatrix.RowY += thisMatrix.RowZ;
                }
                resMatrix.RowY /= thisMatrix.m11;
                thisMatrix.RowY /= thisMatrix.m11;

                /*  1   m01 m02 | m00 0   0
                 *  0   1   m12 | m10 m11 0
                 *  0   m21 m22 | m20 0   1 */
                resMatrix.RowX -= resMatrix.RowY * thisMatrix.m01;
                thisMatrix.RowX -= thisMatrix.RowY * thisMatrix.m01;
                resMatrix.RowZ -= resMatrix.RowY * thisMatrix.m21;
                thisMatrix.RowZ -= thisMatrix.RowY * thisMatrix.m21;

                /*  1   0   m02 | m00 m01 m02
                 *  0   1   m12 | m10 m11 0
                 *  0   0   m22 | m20 m21 1 */
                if (thisMatrix.m22 == 0) { throw new InvalidOperationException("Matrix3x3.InverseMatrix failed at column 2"); }
                resMatrix.RowZ /= thisMatrix.m22;
                thisMatrix.RowZ /= thisMatrix.m22;

                /*  1   0   m02 | m00 m01 m02
                 *  0   1   m12 | m10 m11 0
                 *  0   0   1   | m20 m21 m22 */
                resMatrix.RowX -= resMatrix.RowZ * thisMatrix.m02;
                thisMatrix.RowX -= thisMatrix.RowZ * thisMatrix.m02;
                resMatrix.RowY -= resMatrix.RowZ * thisMatrix.m12;
                thisMatrix.RowY -= thisMatrix.RowZ * thisMatrix.m12;

                /*  1   0   0  |  m00 m01 m02
                 *  0   1   0  |  m10 m11 m12
                 *  0   0   1  |  m20 m21 m22 */
                if (thisMatrix != I) { throw new InvalidOperationException("Matrix3x3.InverseMatrix failed at final check"); }
                return resMatrix;
            }
        }
        #endregion

        #region Constructors
        public Matrix3X3() { }

        public Matrix3X3(Matrix3X3 m)
        {
            this.m00 = m.m00;
            this.m01 = m.m01;
            this.m02 = m.m02;
            this.m10 = m.m10;
            this.m11 = m.m11;
            this.m12 = m.m12;
            this.m20 = m.m20;
            this.m21 = m.m21;
            this.m22 = m.m22;
        }
        #endregion

        #region Methods
        public void Reset()
        {
            this.m00 = this.m01 = this.m02 = 0;
            this.m10 = this.m11 = this.m12 = 0;
            this.m20 = this.m21 = this.m22 = 0;
        }
        #endregion

        #region Operators
        public static Vector3 operator *(Matrix3X3 matrix, Vector3 vec)
        {
            return new Vector3(Vector3.Dot(matrix.RowX, vec), Vector3.Dot(matrix.RowY, vec), Vector3.Dot(matrix.RowZ, vec));
        }

        public static Vector3 operator /(Vector3 vec, Matrix3X3 matrix)
        {
            return matrix.InverseMatrix * vec;
        }

        public static bool operator ==(Matrix3X3 m1, Matrix3X3 m2)
        {
            if ((object)m1 == null) { return (object)m2 == null; }
            if ((object)m2 == null) { return false;}
            return m1.m00 == m2.m00 && m1.m01 == m2.m01 && m1.m02 == m2.m02
                && m1.m10 == m2.m10 && m1.m11 == m2.m11 && m1.m12 == m2.m12
                && m1.m20 == m2.m20 && m1.m21 == m2.m21 && m1.m22 == m2.m22;
        }

        public static bool operator !=(Matrix3X3 m1, Matrix3X3 m2)
        {
            if ((object)m1 == null) { return (object)m2 != null; }
            if ((object)m2 == null) { return true; }
            return !(m1.m00 == m2.m00) || !(m1.m01 == m2.m01) || !(m1.m02 == m2.m02)
                || !(m1.m10 == m2.m10) || !(m1.m11 == m2.m11) || !(m1.m12 == m2.m12)
                || !(m1.m20 == m2.m20) || !(m1.m21 == m2.m21) || !(m1.m22 == m2.m22);
        }
        #endregion

        #region Overrides
        public override string ToString()
        {
            return string.Format("{{({0}, {1}, {2}), ({3}, {4}, {5}), ({6}, {7}, {8})}}", this.m00, this.m01, this.m02, this.m10, this.m11, this.m12, this.m20, this.m21, this.m22);
        }

        public virtual string ToString(string format)
        {
            return string.Format("{{({0}, {1}, {2}), ({3}, {4}, {5}), ({6}, {7}, {8})}}", this.m00.ToString(format), this.m01.ToString(format), this.m02.ToString(format), this.m10.ToString(format), this.m11.ToString(format), this.m12.ToString(format), this.m20.ToString(format), this.m21.ToString(format), this.m22.ToString(format));
        }

        public override bool Equals(object obj)
        {
            if (obj == null) { return false; }
            if (ReferenceEquals(this, obj)) { return true; }
            return obj is Matrix3X3 && Equals((Matrix3X3)obj);
        }

        public virtual bool Equals(Matrix3X3 other)
        {
            return this.m00.Equals(other.m00) && this.m01.Equals(other.m01) && this.m02.Equals(other.m02)
                && this.m10.Equals(other.m10) && this.m11.Equals(other.m11) && this.m12.Equals(other.m12)
                && this.m20.Equals(other.m20) && this.m21.Equals(other.m21) && this.m22.Equals(other.m22);
        }

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
    }
}
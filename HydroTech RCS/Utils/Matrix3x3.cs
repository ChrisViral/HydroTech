using System;
using UnityEngine;

namespace HydroTech_RCS.Utils
{
    public class Matrix3X3
    {
        public float m00, m01, m02;
        public float m10, m11, m12;
        public float m20, m21, m22;

        public static Matrix3X3 I
        {
            get
            {
                Matrix3X3 res = new Matrix3X3();
                res.m00 = res.m11 = res.m22 = 1.0F;
                return res;
            }
        }

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
                 *  m00 m01 m02 1   0   0
                 *  m10 m11 m12 0   1   0
                 *  m20 m21 m22 0   0   1
                 */
                if (thisMatrix.m00 == 0.0F)
                {
                    if (thisMatrix.m10 == 0.0F)
                    {
                        if (thisMatrix.m20 == 0.0F) { throw new Exception("Matrix3x3.InverseMatrix failed at column 0"); }
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
                /*  1   m01 m02 m00 0   0
                 *  m10 m11 m12 0   1   0
                 *  m20 m21 m22 0   0   1
                 */
                resMatrix.RowY -= resMatrix.RowX * thisMatrix.m10;
                thisMatrix.RowY -= thisMatrix.RowX * thisMatrix.m10;
                resMatrix.RowZ -= resMatrix.RowX * thisMatrix.m20;
                thisMatrix.RowZ -= thisMatrix.RowX * thisMatrix.m20;
                /*  1   m01 m02 m00 0   0
                 *  0   m11 m12 m10 1   0
                 *  0   m21 m22 m20 0   1
                 */
                if (thisMatrix.m11 == 0.0F)
                {
                    if (thisMatrix.m21 == 0.0F) { throw new Exception("Matrix3x3.InverseMatrix failed at column 1"); }
                    resMatrix.RowY += resMatrix.RowZ;
                    thisMatrix.RowY += thisMatrix.RowZ;
                }
                resMatrix.RowY /= thisMatrix.m11;
                thisMatrix.RowY /= thisMatrix.m11;
                /*  1   m01 m02 m00 0   0
                 *  0   1   m12 m10 m11 0
                 *  0   m21 m22 m20 0   1
                 */
                resMatrix.RowX -= resMatrix.RowY * thisMatrix.m01;
                thisMatrix.RowX -= thisMatrix.RowY * thisMatrix.m01;
                resMatrix.RowZ -= resMatrix.RowY * thisMatrix.m21;
                thisMatrix.RowZ -= thisMatrix.RowY * thisMatrix.m21;
                /*  1   0   m02 m00 m01 m02
                 *  0   1   m12 m10 m11 0
                 *  0   0   m22 m20 m21 1
                 */
                if (thisMatrix.m22 == 0.0F) { throw new Exception("Matrix3x3.InverseMatrix failed at column 2"); }
                resMatrix.RowZ /= thisMatrix.m22;
                thisMatrix.RowZ /= thisMatrix.m22;
                /*  1   0   m02 m00 m01 m02
                 *  0   1   m12 m10 m11 0
                 *  0   0   1   m20 m21 m22
                 */
                resMatrix.RowX -= resMatrix.RowZ * thisMatrix.m02;
                thisMatrix.RowX -= thisMatrix.RowZ * thisMatrix.m02;
                resMatrix.RowY -= resMatrix.RowZ * thisMatrix.m12;
                thisMatrix.RowY -= thisMatrix.RowZ * thisMatrix.m12;
                /*  1   0   0   m00 m01 m02
                 *  0   1   0   m10 m11 m12
                 *  0   0   1   m20 m21 m22
                 */
                if (thisMatrix != I) { throw new Exception("Matrix3x3.InverseMatrix failed at final check"); }
                return resMatrix;
            }
        }

        public Matrix3X3()
        {
            Reset();
        }

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

        public void Reset()
        {
            this.m00 = this.m01 = this.m02 = 0.0F;
            this.m10 = this.m11 = this.m12 = 0.0F;
            this.m20 = this.m21 = this.m22 = 0.0F;
        }

        public static Vector3 operator *(Matrix3X3 matrix, Vector3 vec)
        {
            return new Vector3(HMaths.DotProduct(matrix.RowX, vec), HMaths.DotProduct(matrix.RowY, vec), HMaths.DotProduct(matrix.RowZ, vec));
        }

        public static Vector3 operator /(Vector3 vec, Matrix3X3 matrix)
        {
            return matrix.InverseMatrix * vec;
        }

        public override string ToString()
        {
            return "{(" + this.m00 + ", " + this.m01 + ", " + this.m02 + "), " + "(" + this.m10 + ", " + this.m11 + ", " + this.m12 + "), " + "(" + this.m20 + ", " + this.m21 + ", " + this.m22 + ")}";
        }

        public virtual string ToString(string format)
        {
            return "{(" + this.m00.ToString(format) + ", " + this.m01.ToString(format) + ", " + this.m02.ToString(format) + "), " + "(" + this.m10.ToString(format) + ", " + this.m11.ToString(format) + ", " + this.m12.ToString(format) + "), " + "(" + this.m20.ToString(format) + ", " + this.m21.ToString(format) + ", " + this.m22.ToString(format) + ")}";
        }
    }
}
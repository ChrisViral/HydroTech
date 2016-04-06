using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HydroTech_FC
{
    using UnityEngine;

    public class Matrix3x3
    {
        public Matrix3x3() { Reset(); }
        public Matrix3x3(Matrix3x3 m)
        {
            m00 = m.m00;
            m01 = m.m01;
            m02 = m.m02;
            m10 = m.m10;
            m11 = m.m11;
            m12 = m.m12;
            m20 = m.m20;
            m21 = m.m21;
            m22 = m.m22;
        }

        public float m00, m01, m02;
        public float m10, m11, m12;
        public float m20, m21, m22;

        static public Matrix3x3 I
        {
            get
            {
                Matrix3x3 res = new Matrix3x3();
                res.m00 = res.m11 = res.m22 = 1.0F;
                return res;
            }
        }
        public Vector3 Diagonal
        {
            get { return new Vector3(m00, m11, m22); }
            set
            {
                m00 = value.x;
                m11 = value.y;
                m22 = value.z;
            }
        }
        public Vector3 ColumnX
        {
            get { return new Vector3(m00, m10, m20); }
            set
            {
                m00 = value.x;
                m10 = value.y;
                m20 = value.z;
            }
        }
        public Vector3 ColumnY
        {
            get { return new Vector3(m01, m11, m21); }
            set
            {
                m01 = value.x;
                m11 = value.y;
                m21 = value.z;
            }
        }
        public Vector3 ColumnZ
        {
            get { return new Vector3(m02, m12, m22); }
            set
            {
                m02 = value.x;
                m12 = value.y;
                m22 = value.z;
            }
        }
        public Vector3 RowX
        {
            get { return new Vector3(m00, m01, m02); }
            set
            {
                m00 = value.x;
                m01 = value.y;
                m02 = value.z;
            }
        }
        public Vector3 RowY
        {
            get { return new Vector3(m10, m11, m12); }
            set
            {
                m10 = value.x;
                m11 = value.y;
                m12 = value.z;
            }
        }
        public Vector3 RowZ
        {
            get { return new Vector3(m20, m21, m22); }
            set
            {
                m20 = value.x;
                m21 = value.y;
                m22 = value.z;
            }
        }
        public Matrix3x3 InverseMatrix
        {
            get
            {
                Matrix3x3 thisMatrix = new Matrix3x3(this);
                Matrix3x3 resMatrix = new Matrix3x3(I);
                /*  We start with
                 *  m00 m01 m02 1   0   0
                 *  m10 m11 m12 0   1   0
                 *  m20 m21 m22 0   0   1
                 */
                if (thisMatrix.m00 == 0.0F)
                {
                    if (thisMatrix.m10 == 0.0F)
                    {
                        if (thisMatrix.m20 == 0.0F)
                            throw (new Exception("Matrix3x3.InverseMatrix failed at column 0"));
                        else
                        {
                            resMatrix.RowX += resMatrix.RowZ;
                            thisMatrix.RowX += thisMatrix.RowZ;
                        }
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
                    if (thisMatrix.m21 == 0.0F)
                        throw (new Exception("Matrix3x3.InverseMatrix failed at column 1"));
                    else
                    {
                        resMatrix.RowY += resMatrix.RowZ;
                        thisMatrix.RowY += thisMatrix.RowZ;
                    }
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
                if (thisMatrix.m22 == 0.0F)
                    throw (new Exception("Matrix3x3.InverseMatrix failed at column 2"));
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
                if (thisMatrix != I)
                    throw (new Exception("Matrix3x3.InverseMatrix failed at final check"));
                return resMatrix;
            }
        }

        public void Reset()
        {
            m00 = m01 = m02 = 0.0F;
            m10 = m11 = m12 = 0.0F;
            m20 = m21 = m22 = 0.0F;
        }

        static public Vector3 operator *(Matrix3x3 matrix, Vector3 vec)
        {
            return new Vector3(
                HMaths.DotProduct(matrix.RowX, vec),
                HMaths.DotProduct(matrix.RowY, vec),
                HMaths.DotProduct(matrix.RowZ, vec)
                );
        }
        static public Vector3 operator /(Vector3 vec, Matrix3x3 matrix)
        {
            return matrix.InverseMatrix * vec;
        }

        public override string ToString()
        {
            return "{(" + m00.ToString() + ", " + m01.ToString() + ", " + m02.ToString() + "), "
                + "(" + m10.ToString() + ", " + m11.ToString() + ", " + m12.ToString() + "), "
                + "(" + m20.ToString() + ", " + m21.ToString() + ", " + m22.ToString() + ")}";
        }
        virtual public string ToString(string format)
        {
            return "{(" + m00.ToString(format) + ", " + m01.ToString(format) + ", " + m02.ToString(format) + "), "
                + "(" + m10.ToString(format) + ", " + m11.ToString(format) + ", " + m12.ToString(format) + "), "
                + "(" + m20.ToString(format) + ", " + m21.ToString(format) + ", " + m22.ToString(format) + ")}";
        }
    }
}

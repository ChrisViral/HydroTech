using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HydroTech_FC
{
    using UnityEngine;

    static public class HMaths
    {
        static public double Power(double x, double y) { return Math.Pow(x, y); }
        static public double SqRt(double x) { return Math.Sqrt(x); }
        static public double CubeRoot(double x) { return Power(x, 1.0 / 3); }

        static public float Power(float x, float y) { return Mathf.Pow(x, y); }
        static public float SqRt(float x) { return Mathf.Sqrt(x); }
        static public float CubeRoot(float x) { return Power(x, 1.0F / 3); }

        public const double PI = Math.PI;
        public const double RadToDegMultiplier = 180 / PI;
        public const double DegToRadMultiplier = 1 / RadToDegMultiplier;

        public const float PI_f = Mathf.PI;
        public const float RadToDegMultiplier_f = Mathf.Rad2Deg;
        public const float DegToRadMultiplier_f = Mathf.Deg2Rad;

        static public double RadToDeg(double x) { return x * RadToDegMultiplier; }
        static public double DegToRad(double x) { return x * DegToRadMultiplier; }

        static public float RadToDeg(float x) { return x * RadToDegMultiplier_f; }
        static public float DegToRad(float x) { return x * DegToRadMultiplier_f; }

        static public double Atan(double x) { return Math.Atan(x); }

        static public float Atan(float x) { return Mathf.Atan(x); }

        static public int Abs(int x) { return Math.Abs(x); }
        static public double Abs(double x) { return Math.Abs(x); }
        static public float Abs(float x) { return Math.Abs(x); }

        static public int Sign(int x) { return Math.Sign(x); }
        static public int Sign(double x) { return Math.Sign(x); }
        static public int Sign(float x) { return Math.Sign(x); }

        static public int Max(int x, int y) { return Math.Max(x, y); }
        static public double Max(double x, double y) { return Math.Max(x, y); }
        static public float Max(float x, float y) { return Math.Max(x, y); }

        static public int Min(int x, int y) { return Math.Min(x, y); }
        static public double Min(double x, double y) { return Math.Min(x, y); }
        static public float Min(float x, float y) { return Math.Min(x, y); }

        static public int Cut(int x, int min, int max) { return Max(min, Min(max, x)); }
        static public double Cut(double x, double min, double max) { return Max(min, Min(max, x)); }
        static public float Cut(float x, float min, float max) { return Max(min, Min(max, x)); }

        static public Vector3 CrossProduct(Vector3 x, Vector3 y) { return -Vector3.Cross(x, y); }
        static public float DotProduct(Vector3 x, Vector3 y) { return Vector3.Dot(x, y); }
        static public float DotProduct(Vector2 x, Vector2 y) { return Vector2.Dot(x, y); }
    }
}
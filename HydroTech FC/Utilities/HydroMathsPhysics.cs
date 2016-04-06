using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HydroTech_FC
{
    using UnityEngine;

    public static class HMaths
    {
        public static double Power(double x, double y) { return Math.Pow(x, y); }
        public static double SqRt(double x) { return Math.Sqrt(x); }
        public static double CubeRoot(double x) { return Power(x, 1.0 / 3); }

        public static float Power(float x, float y) { return Mathf.Pow(x, y); }
        public static float SqRt(float x) { return Mathf.Sqrt(x); }
        public static float CubeRoot(float x) { return Power(x, 1.0F / 3); }

        public const double PI = Math.PI;
        public const double RadToDegMultiplier = 180 / PI;
        public const double DegToRadMultiplier = 1 / RadToDegMultiplier;

        public const float PI_f = Mathf.PI;
        public const float RadToDegMultiplier_f = Mathf.Rad2Deg;
        public const float DegToRadMultiplier_f = Mathf.Deg2Rad;

        public static double RadToDeg(double x) { return x * RadToDegMultiplier; }
        public static double DegToRad(double x) { return x * DegToRadMultiplier; }

        public static float RadToDeg(float x) { return x * RadToDegMultiplier_f; }
        public static float DegToRad(float x) { return x * DegToRadMultiplier_f; }

        public static double Atan(double x) { return Math.Atan(x); }

        public static float Atan(float x) { return Mathf.Atan(x); }

        public static int Abs(int x) { return Math.Abs(x); }
        public static double Abs(double x) { return Math.Abs(x); }
        public static float Abs(float x) { return Math.Abs(x); }

        public static int Sign(int x) { return Math.Sign(x); }
        public static int Sign(double x) { return Math.Sign(x); }
        public static int Sign(float x) { return Math.Sign(x); }

        public static int Max(int x, int y) { return Math.Max(x, y); }
        public static double Max(double x, double y) { return Math.Max(x, y); }
        public static float Max(float x, float y) { return Math.Max(x, y); }

        public static int Min(int x, int y) { return Math.Min(x, y); }
        public static double Min(double x, double y) { return Math.Min(x, y); }
        public static float Min(float x, float y) { return Math.Min(x, y); }

        public static int Cut(int x, int min, int max) { return Max(min, Min(max, x)); }
        public static double Cut(double x, double min, double max) { return Max(min, Min(max, x)); }
        public static float Cut(float x, float min, float max) { return Max(min, Min(max, x)); }

        public static Vector3 CrossProduct(Vector3 x, Vector3 y) { return -Vector3.Cross(x, y); }
        public static float DotProduct(Vector3 x, Vector3 y) { return Vector3.Dot(x, y); }
        public static float DotProduct(Vector2 x, Vector2 y) { return Vector2.Dot(x, y); }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HydroTech_RCS.Constants.Units
{
    public static class UnitStrings
    {
        private const String _Dot = "·";
        private const String _Index = "^";
        private const String _Divide = "/";

        public const String Length = "m";
        public const String Time = "s";
        public const String Angle = "rad";

        public const String Force = "N";

        public const String Speed = Length + _Dot + Time + _Index + "-1";
        public const String Speed_Simple = Length + _Divide + Time;
        public const String Acceleration = Length + _Dot + Time + _Index + "-2";
        public const String AngularV = Angle + _Dot + Time + _Index + "-1";
        public const String AngularV_Simple = Angle + _Divide + Time;
        public const String AngularAcc = Angle + _Dot + Time + _Index + "-2";
        public const String Torque = Force + _Dot + Length;
    }
}

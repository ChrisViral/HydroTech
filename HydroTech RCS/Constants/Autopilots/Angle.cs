using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HydroTech_RCS.Constants.Autopilots
{
    namespace General
    {
        public static class Angle
        {
            public const float ReverseDirSin = 0.1F; // 5.74
        }
    }
    namespace Landing
    {
        public static class Angle
        {
            public const float TranslationReadyAngleSin = 0.05F; // 2.87
        }
    }
    namespace Docking
    {
        public static class Angle
        {
            public const float TranslationReadyAngleSin = 0.05F; // 2.87
            public const float MaxTranslationErrAngleCos = 0.95F; // 18.19 degrees
        }
    }
}

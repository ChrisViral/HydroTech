using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HydroTech_RCS.Constants.Autopilots
{
    namespace General
    {
        public static class Rotation
        {
            public const float RotationHoldRate = 10.0F;

            public const float KillAngularDiff = 5.0F;
            public const float KillAngularV = 4.0F;
        }
    }
    namespace Landing
    {
        public static class Rotation
        {
            public const float KillRotThrustRate = 1.0F;
        }
    }
}

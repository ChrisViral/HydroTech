using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HydroTech_RCS.Constants.Autopilots
{
    namespace Landing
    {
        static public class Velocity
        {
            public const float SafeHorizontalSpeed = 0.1F; // maximum horizontal speed for touchdown
            // public const float MaxAscentSpeed = 1.0F; // maximum vertical ascending speed when holding altitude
        }
    }
    namespace Docking
    {
        static public class Velocity
        {
            public const float Vel0 = 1.0F;
            public const float MaxSpeed = 0.7F;
            public const float SafeSpeed = 0.5F;
            public const float StopSpeed = 0.05F;
            public const float FinalStageSpeedMaxMultiplier = 1.1F;
        }
    }
}

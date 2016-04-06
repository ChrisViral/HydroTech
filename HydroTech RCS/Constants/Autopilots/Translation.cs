using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HydroTech_RCS.Constants.Autopilots
{
    namespace Landing
    {
        static public class Translation
        {
            public const float IdleThrust = 0.0F;
            public const float VertBrakeThrust = 1.0F; // throttle for vertical braking
            public const float KillHoriThrustRate = 0.3F; // max throttle for decelerating (horizontal)
            public const float HoriBrakeThrust = 1.0F; // max throttle for horizontal braking
        }
    }
}

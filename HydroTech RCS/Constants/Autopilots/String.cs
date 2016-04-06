using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HydroTech_RCS.Constants.Autopilots
{
    namespace Translation
    {
        static public class Str
        {
            public const String nameString = "TranslationAP";
        }
    }
    namespace Landing
    {
        static public class Str
        {
            public const String nameString = "LandingAP";

            static public class Status
            {
                public const String DISENGAGED = "Disengaged";
                public const String IDLE = "Idle";
                public const String DECELERATE = "Decelerating";
                public const String DESCEND = "Final Descent";
                public const String VERTICAL = "Vertical braking";
                public const String HORIZONTAL = "Horizontal braking";
                public const String WARP = "Warping";
                public const String AVOID = "Avoiding contact";
                public const String LANDED = "Holding orientation";
                public const String FLOAT = "Holding altitude";
            }

            static public class Warning
            {
                public const String LANDED = "Landed";
                public const String WARP = "Safe to warp";
                public const String SAFE = "Safe to land";
                public const String OK = "Ready to land";
                public const String DANGER = "Dangerous to land";
                public const String LOWTWR = "TWR too low";
                public const String OUTSYNC = "Outside of synchronous altitude";
                public const String FINAL = "Close to ground";
                public const String FLOAT = FINAL;
            }
        }
    }
    namespace Docking
    {
        static public class Str
        {
            public const String nameString = "DockAP.Active";
            public const String nameString_Target = "DockAP.Target";
        }
    }
    namespace PreciseControl
    {
        static public class Str
        {
            public const String nameString = "PreciseAP";
        }
    }
}

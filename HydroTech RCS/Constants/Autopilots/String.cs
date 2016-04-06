using System;

namespace HydroTech_RCS.Constants.Autopilots
{
    namespace Translation
    {
        public static class Str
        {
            public const string nameString = "TranslationAP";
        }
    }

    namespace Landing
    {
        public static class Str
        {
            public static class Status
            {
                public const string disengaged = "Disengaged";
                public const string idle = "Idle";
                public const string decelerate = "Decelerating";
                public const string descend = "Final Descent";
                public const string vertical = "Vertical braking";
                public const string horizontal = "Horizontal braking";
                public const string warp = "Warping";
                public const string avoid = "Avoiding contact";
                public const string landed = "Holding orientation";
                public const string FLOAT = "Holding altitude";
            }

            public static class Warning
            {
                public const string landed = "Landed";
                public const string warp = "Safe to warp";
                public const string safe = "Safe to land";
                public const string ok = "Ready to land";
                public const string danger = "Dangerous to land";
                public const string lowtwr = "TWR too low";
                public const string outsync = "Outside of synchronous altitude";
                public const string final = "Close to ground";
                public const string FLOAT = final;
            }

            public const string nameString = "LandingAP";
        }
    }

    namespace Docking
    {
        public static class Str
        {
            public const string nameString = "DockAP.Active";
            public const string nameStringTarget = "DockAP.Target";
        }
    }

    namespace PreciseControl
    {
        public static class Str
        {
            public const string nameString = "PreciseAP";
        }
    }
}
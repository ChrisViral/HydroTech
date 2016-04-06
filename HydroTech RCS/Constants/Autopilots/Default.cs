using HydroTech_RCS.Autopilots;
using UnityEngine;

namespace HydroTech_RCS.Constants.Autopilots
{
    namespace Translation
    {
        public static class Default
        {
            public static class Bool
            {
                public const bool mainThrottleRespond = true;
                public const bool holdOrient = true;
            }

            public static class Float
            {
                public const float thrustRate = 1.0F;
            }

            public static class Misc
            {
                public const APTranslation.TransDir transMode = APTranslation.TransDir.FORWARD;
                public static readonly Vector3 thrustVector = Vector3.up;
            }
        }
    }

    namespace Landing
    {
        public static class Default
        {
            public static class Bool
            {
                public const bool vabPod = true;
                public const bool engine = false;
                public const bool burnRetro = false;
                public const bool touchdown = true;
                public const bool useTrueAlt = true;
            }

            public static class Float
            {
                public const float safeTouchDownSpeed = 0.5F; // default vertical speed for final touchdown
                public const float maxThrottle = 1.0F;
                public const float altKeep = 10.0F;
            }
        }
    }

    namespace Docking
    {
        public static class Default
        {
            public static class Bool
            {
                public const bool autoOrient = false;
                public const bool camView = false;
                public const bool driveTarget = false;
                public const bool killRelV = false;
                public const bool manual = true;
                public const bool showLine = true;
            }

            public static class Float
            {
                public const float finalStageSpeed = 0.4F;
                public const float angularAcc = 0.5F;
                public const float acc = 0.5F;
            }

            public static class Misc
            {
                public const ModuleDockAssistCam cam = null;
                public const ModuleDockAssistTarget target = null;
            }
        }
    }

    namespace PreciseControl
    {
        public static class Default
        {
            public static class Bool
            {
                public const bool byRate = true;
            }

            public static class Float
            {
                public const float rotationRate = 0.1F;
                public const float translationRate = 0.1F;
                public const float angularAcc = 1.0F;
                public const float acc = 1.0F;
            }
        }
    }
}
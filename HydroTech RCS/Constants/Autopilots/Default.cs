using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HydroTech_RCS.Constants.Autopilots
{
    using UnityEngine;
    using HydroTech_RCS.Autopilots;

    namespace Translation
    {
        static public class Default
        {
            static public class BOOL
            {
                public const bool mainThrottleRespond = true;
                public const bool HoldOrient = true;
            }
            static public class FLOAT
            {
                public const float thrustRate = 1.0F;
            }
            static public class MISC
            {
                public const APTranslation.TransDir Trans_Mode = APTranslation.TransDir.FORWARD;
                static public readonly Vector3 thrustVector = Vector3.up;
            }
        }
    }
    namespace Landing
    {
        static public class Default
        {
            static public class BOOL
            {
                public const bool VABPod = true;
                public const bool Engine = false;
                public const bool burnRetro = false;
                public const bool touchdown = true;
                public const bool useTrueAlt = true;
            }
            static public class FLOAT
            {
                public const float safeTouchDownSpeed = 0.5F; // default vertical speed for final touchdown
                public const float maxThrottle = 1.0F;
                public const float altKeep = 10.0F;
            }
        }
    }
    namespace Docking
    {
        static public class Default
        {
            static public class BOOL
            {
                public const bool AutoOrient = false;
                public const bool CamView = false;
                public const bool DriveTarget = false;
                public const bool KillRelV = false;
                public const bool Manual = true;
                public const bool ShowLine = true;
            }
            static public class FLOAT
            {
                public const float FinalStageSpeed = 0.4F;
                public const float AngularAcc = 0.5F;
                public const float Acc = 0.5F;
            }
            static public class MISC
            {
                public const ModuleDockAssistCam Cam = null;
                public const ModuleDockAssistTarget Target = null;
            }
        }
    }
    namespace PreciseControl
    {
        static public class Default
        {
            static public class BOOL
            {
                public const bool byRate = true;
            }
            static public class FLOAT
            {
                public const float RotationRate = 0.1F;
                public const float TranslationRate = 0.1F;
                public const float AngularAcc = 1.0F;
                public const float Acc = 1.0F;
            }
        }
    }
}

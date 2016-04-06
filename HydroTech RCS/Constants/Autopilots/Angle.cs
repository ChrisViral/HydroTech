namespace HydroTech_RCS.Constants.Autopilots
{
    namespace General
    {
        public static class Angle
        {
            public const float reverseDirSin = 0.1F; // 5.74
        }
    }

    namespace Landing
    {
        public static class Angle
        {
            public const float translationReadyAngleSin = 0.05F; // 2.87
        }
    }

    namespace Docking
    {
        public static class Angle
        {
            public const float translationReadyAngleSin = 0.05F; // 2.87
            public const float maxTranslationErrAngleCos = 0.95F; // 18.19 degrees
        }
    }
}
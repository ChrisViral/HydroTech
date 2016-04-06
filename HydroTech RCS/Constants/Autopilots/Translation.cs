namespace HydroTech_RCS.Constants.Autopilots
{
    namespace Landing
    {
        public static class Translation
        {
            public const float idleThrust = 0.0F;
            public const float vertBrakeThrust = 1.0F; // throttle for vertical braking
            public const float killHoriThrustRate = 0.3F; // max throttle for decelerating (horizontal)
            public const float horiBrakeThrust = 1.0F; // max throttle for horizontal braking
        }
    }
}
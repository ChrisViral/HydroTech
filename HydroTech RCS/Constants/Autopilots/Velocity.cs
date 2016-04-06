namespace HydroTech_RCS.Constants.Autopilots
{
    namespace Landing
    {
        public static class Velocity
        {
            public const float safeHorizontalSpeed = 0.1F; // maximum horizontal speed for touchdown
            // public const float MaxAscentSpeed = 1.0F; // maximum vertical ascending speed when holding altitude
        }
    }

    namespace Docking
    {
        public static class Velocity
        {
            public const float vel0 = 1.0F;
            public const float maxSpeed = 0.7F;
            public const float safeSpeed = 0.5F;
            public const float stopSpeed = 0.05F;
            public const float finalStageSpeedMaxMultiplier = 1.1F;
        }
    }
}
namespace HydroTech_RCS.Constants.Autopilots
{
    namespace General
    {
        public static class Rotation
        {
            public const float rotationHoldRate = 10.0F;

            public const float killAngularDiff = 5.0F;
            public const float killAngularV = 4.0F;
        }
    }

    namespace Landing
    {
        public static class Rotation
        {
            public const float killRotThrustRate = 1.0F;
        }
    }
}
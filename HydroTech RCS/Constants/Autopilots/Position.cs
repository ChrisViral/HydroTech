using UnityEngine;

namespace HydroTech_RCS.Constants.Autopilots
{
    namespace Landing
    {
        public static class Position
        {
            public static class Gcd
            {
                public const float radiusAltAsl = 0.01F;
                public const float radiusMin = 1.0F;
                public const float physicsContactDistanceAdd = 10000.0F;
            }

            public const float startSlopeDetectionHeight = 2.0e5F;
            public const float deployGearHeight = 200.0F;
            public const float minHoverHeight = 10.0F; // hovering pattern: kill horizontal speed before final descent
            public const float maxHoverHeight = 15.0F;
            public const float finalDescentHeight = minHoverHeight;
        }
    }

    namespace Docking
    {
        public static class Position
        {
            // public const float MaxDist = 100.0F;

            public const float minZ = 10.0F;
            public const float minXy = 20.0F;
            public const float finalStageErr = 0.05F;

            public static readonly Vector3 finalStagePos = new Vector3(0, 0, 15);
        }
    }
}
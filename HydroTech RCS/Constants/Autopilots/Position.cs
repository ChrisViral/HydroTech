using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HydroTech_RCS.Constants.Autopilots
{
    using UnityEngine;

    namespace Landing
    {
        static public class Position
        {
            public const float StartSlopeDetectionHeight = 2.0e5F;
            public const float DeployGearHeight = 200.0F;
            public const float MinHoverHeight = 10.0F; // hovering pattern: kill horizontal speed before final descent
            public const float MaxHoverHeight = 15.0F;
            public const float FinalDescentHeight = MinHoverHeight;

            static public class GCD
            {
                public const float Radius_altASL = 0.01F;
                public const float Radius_Min = 1.0F;
                public const float PhysicsContactDistanceAdd = 10000.0F;
            }
        }
    }
    namespace Docking
    {
        static public class Position
        {
            // public const float MaxDist = 100.0F;

            public const float MinZ = 10.0F;
            public const float MinXY = 20.0F;

            static public readonly Vector3 FinalStagePos = new Vector3(0, 0, 15);
            public const float FinalStageErr = 0.05F;
        }
    }
}

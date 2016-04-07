using HydroTech_RCS.Autopilots;
using HydroTech_RCS.PartModules;
using UnityEngine;

namespace HydroTech_RCS.Constants
{
    //TODO: get rid of these classes... honestly this is redundant.
    public static class AutopilotConsts
    {
        #region Docking
        //Angular velocity
        public const float maxAngularV = 0.01f;

        //Defaults
        public const bool autoOrient = false;
        public const bool camView = false;
        public const bool driveTarget = false;
        public const bool killRelV = false;
        public const bool manual = true;
        public const bool showLine = true;
        public const float finalStageSpeed = 0.4f;
        public const float dockingAngularAcc = 0.5f;
        public const float dockingAcc = 0.5f;
        public const ModuleDockAssistCam cam = null;
        public const ModuleDockAssistTarget target = null;

        //Position
        public const float minZ = 10;
        public const float minXy = 20;
        public const float finalStageErr = 0.05f;
        public static readonly Vector3 finalStagePos = new Vector3(0, 0, 15);

        //String
        public const string dockingName = "DockAP.Active";
        public const string dockingTargetName = "DockAP.Target";

        //Velocity
        public const float vel0 = 1;
        public const float maxSpeed = 0.7f;
        public const float safeSpeed = 0.5f;
        public const float stopSpeed = 0.05f;
        public const float finalStageSpeedMaxMultiplier = 1.1f;
        #endregion

        #region General
        //Angle
        public const float reverseDirSin = 0.1f;                //5.74°
        public const float translationReadyAngleSin = 0.05f;    //2.87°
        public const float maxTranslationErrAngleCos = 0.95f;   //18.19°

        //Rotation
        public const float rotationHoldRate = 10;
        public const float killAngularDiff = 5;
        public const float killAngularV = 4;
        #endregion

        #region Landing
        //Acceleration
        public const float maxDeceleration = 5;

        //Angular acceleration
        public const float maxAngularAccHold = 5;
        public const float maxAngularAccSteer = 20;

        //Defaults
        public const bool vabPod = true;
        public const bool engine = false;
        public const bool burnRetro = false;
        public const bool touchdown = true;
        public const bool useTrueAlt = true;
        public const float safeTouchDownSpeed = 0.5f;   //Default vertical speed for final touchdown
        public const float maxThrottle = 1;
        public const float altKeep = 10;

        //Position
        public const float radiusAltAsl = 0.01f;
        public const float radiusMin = 1;
        public const float physicsContactDistanceAdd = 10000;
        public const float startSlopeDetectionHeight = 2e5f;
        public const float deployGearHeight = 200;
        public const float minHoverHeight = 10; //Hovering pattern: kill horizontal speed before final descent
        public const float maxHoverHeight = 15;
        public const float finalDescentHeight = 10;

        //Rotation
        public const float killRotThrustRate = 1;

        //Status
        public const string disengaged = "Disengaged";
        public const string idle = "Idle";
        public const string decelerate = "Decelerating";
        public const string descend = "Final Descent";
        public const string vertical = "Vertical braking";
        public const string horizontal = "Horizontal braking";
        public const string stsWarp = "Warping";
        public const string avoid = "Avoiding contact";
        public const string stsLanded = "Holding orientation";
        public const string stsFloat = "Holding altitude";

        //String
        public const string landingName = "LandingAP";

        //Translation
        public const float idleThrust = 0;
        public const float vertBrakeThrust = 1;         //Throttle for vertical braking
        public const float killHoriThrustRate = 0.3f;   //Max throttle for decelerating (horizontal)
        public const float horiBrakeThrust = 1;         //Max throttle for horizontal braking

        //Velocity
        public const float safeHorizontalSpeed = 0.1f; //Maximum horizontal speed for touchdown

        //Warning
        public const string wrnLanded = "Landed";
        public const string wrnWarp = "Safe to warp";
        public const string safe = "Safe to land";
        public const string ok = "Ready to land";
        public const string danger = "Dangerous to land";
        public const string lowtwr = "TWR too low";
        public const string outsync = "Outside of synchronous altitude";
        public const string final = "Close to ground";
        public const string wrnFloat = "Close to ground";
        #endregion

        #region Precise controls
        //Defaults
        public const bool byRate = true;
        public const float rotationRate = 0.1f;
        public const float translationRate = 0.1f;
        public const float pcAngularAcc = 1;
        public const float pcAcc = 1;

        //String
        public const string pcName = "PreciseAP";
        #endregion

        #region Translation
        //Defaults
        public const bool mainThrottleRespond = true;
        public const bool holdOrient = true;
        public const float thrustRate = 1;
        public const APTranslation.TransDir transMode = APTranslation.TransDir.FORWARD;
        public static readonly Vector3 thrustVector = Vector3.up;

        //Strings
        public const string translationName = "TranslationAP";
        #endregion
    }
}

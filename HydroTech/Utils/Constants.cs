using HydroTech.Autopilots;
using UnityEngine;

namespace HydroTech.Utils
{
    //TODO: get rid of these classes... honestly this is redundant.
    public static class Constants
    {
        #region Docking
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

        //Position
        public const float finalStageErr = 0.05f;
        public static readonly Vector3 finalStagePos = new Vector3(0, 0, 15);

        //Velocity
        public const float vel0 = 1;
        public const float safeSpeed = 0.5f;
        public const float stopSpeed = 0.05f;
        #endregion

        #region General
        //Angle
        public const float translationReadyAngleSin = 0.05f;    //2.87°

        //Rotation
        public const float rotationHoldRate = 10;
        #endregion

        #region Landing
        //Defaults
        public const bool vabPod = true;
        public const bool engine = false;
        public const bool burnRetro = false;
        public const bool touchdown = true;
        public const bool useTrueAlt = true;
        public const float safeTouchDownSpeed = 0.5f;   //Default vertical speed for final touchdown
        public const float maxThrottle = 1;
        public const float altKeep = 10;
        public const float safeHorizontalSpeed = 0.1f; //Maximum horizontal speed for touchdown

        //Position
        public const float finalDescentHeight = 10;
        #endregion

        #region Precise controls
        //Defaults
        public const bool byRate = true;
        public const float rotationRate = 0.1f;
        public const float translationRate = 0.1f;
        public const float pcAngularAcc = 1;
        public const float pcAcc = 1;
        #endregion

        #region Translation
        //Defaults
        public const bool mainThrottleRespond = true;
        public const bool holdOrient = true;
        public const float thrustRate = 1;
        public const APTranslation.TransDir transMode = APTranslation.TransDir.FORWARD;
        public static readonly Vector3 thrustVector = Vector3.up;
        #endregion

        #region Units
        public const string length = "m";
        public const string speedSimple = "m/s";
        public const string acceleration = "m/s²";
        public const string angularAcc = "rad/s²";
        #endregion
    }
}
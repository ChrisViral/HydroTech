using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HydroTech_RCS.Autopilots
{
    using UnityEngine;
    using HydroTech_FC;
    using ASAS;
    using Constants.Core;
    using Constants.Autopilots.Translation;

    public class APTranslation : RCSAutopilot
    {
        public static APTranslation theAutopilot { get { return (APTranslation)HydroJebCore.autopilots[AutopilotIDs.Translation]; } }

        public APTranslation()
        {
            fileName = new FileName("translation", "cfg", HydroJebCore.AutopilotSaveFolder);
        }

        protected override string nameString { get { return Str.nameString; } }

        public enum TransDir { RIGHT, LEFT, DOWN, UP, FORWARD, BACKWARD, ADVANCED }

        #region public variables for user input

        #region bool

        [HydroSLNodeInfo(name = "SETTINGS")]
        [HydroSLField(saveName = "MainThr")]
        public bool mainThrottleRespond = Default.BOOL.mainThrottleRespond;

        [HydroSLNodeInfo(name = "SETTINGS")]
        [HydroSLField(saveName = "HoldDir")]
        public bool _HoldOrient = Default.BOOL.HoldOrient;
        public bool HoldOrient
        {
            get { return _HoldOrient; }
            set
            {
                if (value && !_HoldOrient)
                {
                    curOrient = ActiveVessel.ReferenceTransform.up;
                    curRoll = ActiveVessel.ReferenceTransform.right;
                }
                _HoldOrient = value;
            }
        }

        #endregion

        #region float

        [HydroSLNodeInfo(name = "SETTINGS")]
        [HydroSLField(saveName = "ThrustRate")]
        public float thrustRate = Default.FLOAT.thrustRate;

        #endregion

        #region misc

        [HydroSLNodeInfo(name = "SETTINGS")]
        [HydroSLField(saveName = "TransMode")]
        public TransDir _Trans_Mode = Default.MISC.Trans_Mode;
        public TransDir Trans_Mode
        {
            get { return _Trans_Mode; }
            set
            {
                thrustVector = GetVector(value);
                _Trans_Mode = value;
            }
        }

        [HydroSLNodeInfo(name = "SETTINGS")]
        [HydroSLField(saveName = "Vector")]
        public Vector3 thrustVector = Default.MISC.thrustVector;

        #endregion

        #region override

        public override bool engaged
        {
            set
            {
                if (!active)
                    return;
                if (HoldOrient)
                {
                    HoldOrient = false;
                    HoldOrient = true;
                }
                base.engaged = value;
            }
        }

        #endregion

        #endregion

        protected override void LoadDefault()
        {
            base.LoadDefault();
            HoldOrient = Default.BOOL.HoldOrient;
            mainThrottleRespond = Default.BOOL.mainThrottleRespond;
            thrustRate = Default.FLOAT.thrustRate;
            Trans_Mode = Default.MISC.Trans_Mode;
        }

        protected Vector3 curOrient;
        protected Vector3 curRoll;

        protected override void DriveAutopilot(FlightCtrlState ctrlState)
        {
            base.DriveAutopilot(ctrlState);

            if (HoldOrient)
            {
                HydroActionGroupManager.ActiveVessel.SAS = false;
                HoldDirStateCalculator stateCal = new HoldDirStateCalculator();
                stateCal.Calculate(curOrient, curRoll, ActiveVessel);
                stateCal.SetCtrlStateRotation(ctrlState);
            }

            float RealThrustRate = thrustRate * (mainThrottleRespond ? ctrlState.mainThrottle : 1.0F);
            ctrlState.X = -thrustVector.x * RealThrustRate;
            ctrlState.Y = -thrustVector.y * RealThrustRate;
            ctrlState.Z = -thrustVector.z * RealThrustRate;
        }

        public static Vector3 GetUnitVector(TransDir Dir)
        {
            Vector3 Vec = new Vector3(0.0F, 0.0F, 0.0F);
            switch (Dir)
            {
                case TransDir.RIGHT: Vec.x = 1.0F; break;
                case TransDir.LEFT: Vec.x = -1.0F; break;
                case TransDir.DOWN: Vec.y = 1.0F; break;
                case TransDir.UP: Vec.y = -1.0F; break;
                case TransDir.FORWARD: Vec.z = 1.0F; break;
                case TransDir.BACKWARD: Vec.z = -1.0F; break;
            }
            return Vec;
        }

        protected Vector3 GetVector(TransDir Dir)
        {
            if (Dir != TransDir.ADVANCED)
                return GetUnitVector(Dir);
            else
                return thrustVector;
        }
    }
}

using HydroTech_FC;
using HydroTech_RCS.Autopilots.ASAS;
using HydroTech_RCS.Constants.Autopilots.Translation;
using HydroTech_RCS.Constants.Core;
using UnityEngine;

namespace HydroTech_RCS.Autopilots
{
    public class APTranslation : RcsAutopilot
    {
        public enum TransDir
        {
            RIGHT,
            LEFT,
            DOWN,
            UP,
            FORWARD,
            BACKWARD,
            ADVANCED
        }

        protected Vector3 curOrient;
        protected Vector3 curRoll;

        public static APTranslation TheAutopilot
        {
            get { return (APTranslation)HydroJebCore.autopilots[AutopilotIDs.translation]; }
        }

        protected override string nameString
        {
            get { return Str.nameString; }
        }

        public APTranslation()
        {
            this.fileName = new FileName("translation", "cfg", HydroJebCore.autopilotSaveFolder);
        }

        protected override void LoadDefault()
        {
            base.LoadDefault();
            this.HoldOrient = Default.Bool.holdOrient;
            this.mainThrottleRespond = Default.Bool.mainThrottleRespond;
            this.thrustRate = Default.Float.thrustRate;
            this.TransMode = Default.Misc.transMode;
        }

        protected override void DriveAutopilot(FlightCtrlState ctrlState)
        {
            base.DriveAutopilot(ctrlState);

            if (this.HoldOrient)
            {
                HydroActionGroupManager.ActiveVessel.SAS = false;
                HoldDirStateCalculator stateCal = new HoldDirStateCalculator();
                stateCal.Calculate(this.curOrient, this.curRoll, ActiveVessel);
                stateCal.SetCtrlStateRotation(ctrlState);
            }

            float realThrustRate = this.thrustRate * (this.mainThrottleRespond ? ctrlState.mainThrottle : 1.0F);
            ctrlState.X = -this.thrustVector.x * realThrustRate;
            ctrlState.Y = -this.thrustVector.y * realThrustRate;
            ctrlState.Z = -this.thrustVector.z * realThrustRate;
        }

        public static Vector3 GetUnitVector(TransDir dir)
        {
            Vector3 vec = new Vector3(0.0F, 0.0F, 0.0F);
            switch (dir)
            {
                case TransDir.RIGHT:
                    vec.x = 1.0F;
                    break;
                case TransDir.LEFT:
                    vec.x = -1.0F;
                    break;
                case TransDir.DOWN:
                    vec.y = 1.0F;
                    break;
                case TransDir.UP:
                    vec.y = -1.0F;
                    break;
                case TransDir.FORWARD:
                    vec.z = 1.0F;
                    break;
                case TransDir.BACKWARD:
                    vec.z = -1.0F;
                    break;
            }
            return vec;
        }

        protected Vector3 GetVector(TransDir dir)
        {
            if (dir != TransDir.ADVANCED) { return GetUnitVector(dir); }
            return this.thrustVector;
        }

        #region public variables for user input

        #region bool
        [HydroSLNodeInfo(name = "SETTINGS"), HydroSLField(saveName = "MainThr")]
        public bool mainThrottleRespond = Default.Bool.mainThrottleRespond;

        [HydroSLNodeInfo(name = "SETTINGS"), HydroSLField(saveName = "HoldDir")]
        public bool holdOrient = Default.Bool.holdOrient;

        public bool HoldOrient
        {
            get { return this.holdOrient; }
            set
            {
                if (value && !this.holdOrient)
                {
                    this.curOrient = ActiveVessel.ReferenceTransform.up;
                    this.curRoll = ActiveVessel.ReferenceTransform.right;
                }
                this.holdOrient = value;
            }
        }
        #endregion

        #region float
        [HydroSLNodeInfo(name = "SETTINGS"), HydroSLField(saveName = "ThrustRate")]
        public float thrustRate = Default.Float.thrustRate;
        #endregion

        #region misc
        [HydroSLNodeInfo(name = "SETTINGS"), HydroSLField(saveName = "TransMode")]
        public TransDir transMode = Default.Misc.transMode;

        public TransDir TransMode
        {
            get { return this.transMode; }
            set
            {
                this.thrustVector = GetVector(value);
                this.transMode = value;
            }
        }

        [HydroSLNodeInfo(name = "SETTINGS"), HydroSLField(saveName = "Vector")]
        public Vector3 thrustVector = Default.Misc.thrustVector;
        #endregion

        #region override
        public override bool engaged
        {
            set
            {
                if (!this.Active) { return; }
                if (this.HoldOrient)
                {
                    this.HoldOrient = false;
                    this.HoldOrient = true;
                }
                this.Engaged = value;
            }
        }
        #endregion

        #endregion
    }
}
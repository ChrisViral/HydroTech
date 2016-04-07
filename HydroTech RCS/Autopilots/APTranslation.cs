using HydroTech_FC;
using HydroTech_RCS.Autopilots.Calculators;
using HydroTech_RCS.Constants.Autopilots.Translation;
using HydroTech_RCS.Constants.Core;
using UnityEngine;

namespace HydroTech_RCS.Autopilots
{
    public class APTranslation : RCSAutopilot
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

        #region Fields
        protected Vector3 curOrient;
        protected Vector3 curRoll;
        #endregion

        #region Properties
        public static APTranslation TheAutopilot
        {
            get { return (APTranslation)HydroJebCore.autopilots[AutopilotIDs.translation]; }
        }

        protected override string NameString
        {
            get { return Str.nameString; }
        }
        #endregion

        #region Constructor
        public APTranslation()
        {
            this.fileName = new FileName("translation", "cfg", HydroJebCore.autopilotSaveFolder);
        }
        #endregion

        #region Autopilot
        protected override void DriveAutopilot(FlightCtrlState ctrlState)
        {
            base.DriveAutopilot(ctrlState);

            if (this.HoldOrient)
            {
                HydroActionGroupManager.ActiveVessel.SAS = false;
                HoldDirectionCalculator stateCal = new HoldDirectionCalculator();
                stateCal.Calculate(this.curOrient, this.curRoll, ActiveVessel);
                stateCal.SetCtrlStateRotation(ctrlState);
            }

            float realThrustRate = this.thrustRate * (this.mainThrottleRespond ? ctrlState.mainThrottle : 1);
            ctrlState.X = -this.thrustVector.x * realThrustRate;
            ctrlState.Y = -this.thrustVector.y * realThrustRate;
            ctrlState.Z = -this.thrustVector.z * realThrustRate;
        }
        #endregion

        #region Methods
        protected Vector3 GetVector(TransDir dir)
        {
            return dir != TransDir.ADVANCED ? GetUnitVector(dir) : this.thrustVector;
        }
        #endregion

        #region Static Methods
        public static Vector3 GetUnitVector(TransDir dir)
        {
            Vector3 vec = new Vector3();
            switch (dir)
            {
                case TransDir.RIGHT:
                    vec.x = 1;
                    break;
                case TransDir.LEFT:
                    vec.x = -1;
                    break;
                case TransDir.DOWN:
                    vec.y = 1;
                    break;
                case TransDir.UP:
                    vec.y = -1;
                    break;
                case TransDir.FORWARD:
                    vec.z = 1;
                    break;
                case TransDir.BACKWARD:
                    vec.z = -1;
                    break;
            }
            return vec;
        }
        #endregion

        #region User input vars
        [HydroSLNodeInfo(name = "SETTINGS"), HydroSLField(saveName = "MainThr")]
        public bool mainThrottleRespond = Default.Bool.mainThrottleRespond;

        [HydroSLNodeInfo(name = "SETTINGS"), HydroSLField(saveName = "ThrustRate")]
        public float thrustRate = Default.Float.thrustRate;

        [HydroSLNodeInfo(name = "SETTINGS"), HydroSLField(saveName = "Vector")]
        public Vector3 thrustVector = Default.Misc.thrustVector;

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
        
        public override bool Engaged
        {
            set
            {
                if (!this.Active) { return; }
                if (this.HoldOrient)
                {
                    this.HoldOrient = false;
                    this.HoldOrient = true;
                }
                base.Engaged = value;
            }
        }
        #endregion

        #region Functions
        protected override void LoadDefault()
        {
            base.LoadDefault();
            this.HoldOrient = Default.Bool.holdOrient;
            this.mainThrottleRespond = Default.Bool.mainThrottleRespond;
            this.thrustRate = Default.Float.thrustRate;
            this.TransMode = Default.Misc.transMode;
        }
        #endregion
    }
}
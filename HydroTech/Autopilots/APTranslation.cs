using HydroTech.Autopilots.Calculators;
using HydroTech.Managers;
using HydroTech.Storage;
using HydroTech.Utils;
using UnityEngine;

namespace HydroTech.Autopilots
{
    public class APTranslation : Autopilot
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
        protected override string NameString
        {
            get { return "TranslationAP"; }
        }
        #endregion

        #region User input vars
        [HydroSLNodeInfo(name = "SETTINGS"), HydroSLField(saveName = "MainThr")]
        public bool mainThrottleRespond = Constants.mainThrottleRespond;

        [HydroSLNodeInfo(name = "SETTINGS"), HydroSLField(saveName = "ThrustRate")]
        public float thrustRate = Constants.thrustRate;

        [HydroSLNodeInfo(name = "SETTINGS"), HydroSLField(saveName = "Vector")]
        public Vector3 thrustVector = Constants.thrustVector;

        [HydroSLNodeInfo(name = "SETTINGS"), HydroSLField(saveName = "HoldDir")]
        public bool holdOrient = Constants.holdOrient;
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
        public TransDir transMode = Constants.transMode;
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

        #region Constructor
        public APTranslation()
        {
            this.fileName = new FileName("translation", "cfg", FileName.autopilotSaveFolder);
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

        #region Overrides
        protected override void LoadDefault()
        {
            base.LoadDefault();
            this.HoldOrient = Constants.holdOrient;
            this.mainThrottleRespond = Constants.mainThrottleRespond;
            this.thrustRate = Constants.thrustRate;
            this.TransMode = Constants.transMode;
        }
        #endregion
    }
}
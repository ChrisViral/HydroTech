using HydroTech.Autopilots.Calculators;
using HydroTech.Utils;
using UnityEngine;

namespace HydroTech.Autopilots
{
    public class APTranslation : Autopilot
    {
        public enum TranslationDirection
        {
            FORWARD,
            BACK,
            RIGHT,
            LEFT,
            DOWN,
            UP,
            ADVANCED
        }

        #region Fields
        public bool mainThrottleRespond = true;
        public float thrustRate = 1;
        public Vector3 thrustVector = Vector3.up;
        private Vector3 curOrient, curRoll;
        #endregion

        #region Properties
        private bool holdOrient = true;
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

        private TranslationDirection transMode;
        public TranslationDirection TransMode
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

        protected override string NameString
        {
            get { return "TranslationAP"; }
        }
        #endregion

        #region Methods
        private Vector3 GetVector(TranslationDirection dir)
        {
            return dir != TranslationDirection.ADVANCED ? HTUtils.GetUnitVector(dir) : this.thrustVector;
        }
        #endregion

        #region Overrides
        protected override void DriveAutopilot(FlightCtrlState ctrlState)
        {
            base.DriveAutopilot(ctrlState);

            if (this.HoldOrient)
            {
                HTUtils.SetState(FlightGlobals.ActiveVessel, KSPActionGroup.SAS, false);
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
    }
}
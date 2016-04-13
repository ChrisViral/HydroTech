
namespace HydroTech.Autopilots
{
    public class APPreciseControl : Autopilot
    {
        #region Fields   
        public bool byRate = true;
        public float rotationRate = 0.1f;
        public float translationRate = 0.1f;
        public float angularAcc = 1;
        public float acc = 1;
        #endregion

        #region Properties
        protected override string NameString
        {
            get { return "PreciseControlAP"; }
        }
        #endregion

        #region Overrides
        protected override void DriveAutopilot(FlightCtrlState ctrlState)
        {
            base.DriveAutopilot(ctrlState);
            if (this.byRate)
            {
                ctrlState.yaw *= this.rotationRate;
                ctrlState.roll *= this.rotationRate;
                ctrlState.pitch *= this.rotationRate;
                ctrlState.X *= this.translationRate;
                ctrlState.Y *= this.translationRate;
                ctrlState.Z *= this.translationRate;
            }
            else
            {
                ActiveRCS.MakeRotation(ctrlState, this.angularAcc);
                ActiveRCS.MakeTranslation(ctrlState, this.acc);
            }
        }
        #endregion
    }
}
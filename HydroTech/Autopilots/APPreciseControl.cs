using HydroTech.Storage;

namespace HydroTech.Autopilots
{
    public class APPreciseControl : Autopilot
    {
        #region Properties
        protected override string NameString
        {
            get { return "PreciseControlAP"; }
        }
        #endregion

        #region User input vars     
        [HydroSLNodeInfo(name = "SETTINGS"), HydroSLField(saveName = "byRate")]
        public bool byRate = true;

        [HydroSLNodeInfo(name = "SETTINGS"), HydroSLField(saveName = "RotationRate")]
        public float rotationRate = 0.1f;

        [HydroSLNodeInfo(name = "SETTINGS"), HydroSLField(saveName = "TranslationRate")]
        public float translationRate = 0.1f;

        [HydroSLNodeInfo(name = "SETTINGS"), HydroSLField(saveName = "AngularAcc")]
        public float angularAcc = 1;

        [HydroSLNodeInfo(name = "SETTINGS"), HydroSLField(saveName = "Acc")]
        public float acc = 1;
        #endregion

        #region Constructor
        public APPreciseControl()
        {
            this.fileName = new FileName("precise", "cfg", FileName.autopilotSaveFolder);
        }
        #endregion

        #region Autopilot
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

        #region Overrides
        protected override void LoadDefault()
        {
            base.LoadDefault();
            this.byRate = true;
            this.rotationRate = 0.1f;
            this.translationRate = 0.1f;
            this.angularAcc = 1;
            this.acc = 1;
        }
        #endregion
    }
}
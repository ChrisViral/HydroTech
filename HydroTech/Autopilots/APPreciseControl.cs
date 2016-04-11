using HydroTech.Constants;
using HydroTech.Managers;
using HydroTech.Storage;

namespace HydroTech.Autopilots
{
    public class APPreciseControl : Autopilot
    {
        #region Properties
        public static APPreciseControl PreciseControlAP
        {
            get { return HydroFlightManager.Instance.PreciseControlAutopilot; }
        }

        protected override string NameString
        {
            get { return AutopilotConsts.pcName; }
        }
        #endregion

        #region User input vars     
        [HydroSLNodeInfo(name = "SETTINGS"), HydroSLField(saveName = "byRate")]
        public bool byRate = AutopilotConsts.byRate;

        [HydroSLNodeInfo(name = "SETTINGS"), HydroSLField(saveName = "RotationRate")]
        public float rotationRate = AutopilotConsts.rotationRate;

        [HydroSLNodeInfo(name = "SETTINGS"), HydroSLField(saveName = "TranslationRate")]
        public float translationRate = AutopilotConsts.translationRate;

        [HydroSLNodeInfo(name = "SETTINGS"), HydroSLField(saveName = "AngularAcc")]
        public float angularAcc = AutopilotConsts.pcAngularAcc;

        [HydroSLNodeInfo(name = "SETTINGS"), HydroSLField(saveName = "Acc")]
        public float acc = AutopilotConsts.pcAcc;
        #endregion

        #region Constructor
        public APPreciseControl()
        {
            this.fileName = new FileName("precise", "cfg", HydroJebCore.autopilotSaveFolder);
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
                RcsActive.MakeRotation(ctrlState, this.angularAcc);
                RcsActive.MakeTranslation(ctrlState, this.acc);
            }
        }
        #endregion

        #region Overrides
        protected override void LoadDefault()
        {
            base.LoadDefault();
            this.byRate = AutopilotConsts.byRate;
            this.rotationRate = AutopilotConsts.rotationRate;
            this.translationRate = AutopilotConsts.translationRate;
            this.angularAcc = AutopilotConsts.pcAngularAcc;
            this.acc = AutopilotConsts.pcAcc;
        }
        #endregion
    }
}
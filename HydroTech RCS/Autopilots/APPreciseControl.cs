using HydroTech_FC;
using HydroTech_RCS.Constants.Autopilots.PreciseControl;
using HydroTech_RCS.Constants.Core;

namespace HydroTech_RCS.Autopilots
{
    public class APPreciseControl : RCSAutopilot
    {
        public static APPreciseControl TheAutopilot
        {
            get { return (APPreciseControl)HydroJebCore.autopilots[AutopilotIDs.precise]; }
        }

        protected override string nameString
        {
            get { return Str.nameString; }
        }

        public APPreciseControl()
        {
            this.fileName = new FileName("precise", "cfg", HydroJebCore.autopilotSaveFolder);
        }

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

        #region public variables for user input

        #region bool
        [HydroSLNodeInfo(name = "SETTINGS"), HydroSLField(saveName = "byRate")]
        public bool byRate = Default.Bool.byRate;
        #endregion

        #region float
        [HydroSLNodeInfo(name = "SETTINGS"), HydroSLField(saveName = "RotationRate")]
        public float rotationRate = Default.Float.rotationRate;

        [HydroSLNodeInfo(name = "SETTINGS"), HydroSLField(saveName = "TranslationRate")]
        public float translationRate = Default.Float.translationRate;

        [HydroSLNodeInfo(name = "SETTINGS"), HydroSLField(saveName = "AngularAcc")]
        public float angularAcc = Default.Float.angularAcc;

        [HydroSLNodeInfo(name = "SETTINGS"), HydroSLField(saveName = "Acc")]
        public float acc = Default.Float.acc;
        #endregion

        #region override
        protected override void LoadDefault()
        {
            base.LoadDefault();
            this.byRate = Default.Bool.byRate;
            this.rotationRate = Default.Float.rotationRate;
            this.translationRate = Default.Float.translationRate;
            this.angularAcc = Default.Float.angularAcc;
            this.acc = Default.Float.acc;
        }
        #endregion

        #endregion
    }
}
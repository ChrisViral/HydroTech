using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HydroTech_RCS.Autopilots
{
    using HydroTech_FC;
    using Constants.Core;
    using Constants.Autopilots.PreciseControl;

    public class APPreciseControl : RCSAutopilot
    {
        static public APPreciseControl theAutopilot { get { return (APPreciseControl)HydroJebCore.autopilots[AutopilotIDs.Precise]; } }

        public APPreciseControl()
        {
            fileName = new FileName("precise", "cfg", HydroJebCore.AutopilotSaveFolder);
        }

        protected override string nameString { get { return Str.nameString; } }

        #region public variables for user input

        #region bool

        [HydroSLNodeInfo(name = "SETTINGS")]
        [HydroSLField(saveName = "byRate")]
        public bool byRate = Default.BOOL.byRate;

        #endregion

        #region float

        [HydroSLNodeInfo(name = "SETTINGS")]
        [HydroSLField(saveName = "RotationRate")]
        public float RotationRate = Default.FLOAT.RotationRate;

        [HydroSLNodeInfo(name = "SETTINGS")]
        [HydroSLField(saveName = "TranslationRate")]
        public float TranslationRate = Default.FLOAT.TranslationRate;

        [HydroSLNodeInfo(name = "SETTINGS")]
        [HydroSLField(saveName = "AngularAcc")]
        public float AngularAcc = Default.FLOAT.AngularAcc;

        [HydroSLNodeInfo(name = "SETTINGS")]
        [HydroSLField(saveName = "Acc")]
        public float Acc = Default.FLOAT.Acc;

        #endregion

        #region override

        protected override void LoadDefault()
        {
            base.LoadDefault();
            byRate = Default.BOOL.byRate;
            RotationRate = Default.FLOAT.RotationRate;
            TranslationRate = Default.FLOAT.TranslationRate;
            AngularAcc = Default.FLOAT.AngularAcc;
            Acc = Default.FLOAT.Acc;
        }

        #endregion

        #endregion

        protected override void DriveAutopilot(FlightCtrlState ctrlState)
        {
            base.DriveAutopilot(ctrlState);
            if (byRate)
            {
                ctrlState.yaw *= RotationRate;
                ctrlState.roll *= RotationRate;
                ctrlState.pitch *= RotationRate;
                ctrlState.X *= TranslationRate;
                ctrlState.Y *= TranslationRate;
                ctrlState.Z *= TranslationRate;
            }
            else
            {
                RCSActive.MakeRotation(ctrlState, AngularAcc);
                RCSActive.MakeTranslation(ctrlState, Acc);
            }
        }
    }
}

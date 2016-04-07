using HydroTech_FC;
using HydroTech_RCS.Autopilots;
using HydroTech_RCS.Constants;
using HydroTech_RCS.Constants.Panels;
using UnityEngine;

namespace HydroTech_RCS.Panels
{
    public partial class PanelTranslation : PanelwAP
    {
        protected override int PanelID
        {
            get { return CoreConsts.pTranslation; }
        }

        public override string PanelTitle
        {
            get { return PanelTitles.translation; }
        }

        protected static APTranslation Ta
        {
            get { return APTranslation.TheAutopilot; }
        }

        protected override bool Engaged
        {
            get { return Ta.Engaged; }
            set { Ta.Engaged = value; }
        }

        protected static APTranslation.TransDir TransMode
        {
            get { return Ta.TransMode; }
            set { Ta.TransMode = value; }
        }

        protected static Vector3 ThrustVector
        {
            get { return Ta.thrustVector; }
            set { Ta.thrustVector = value; }
        }

        protected static float ThrustRate
        {
            get { return Ta.thrustRate; }
            set { Ta.thrustRate = value; }
        }

        protected static bool Respond
        {
            get { return Ta.mainThrottleRespond; }
            set { Ta.mainThrottleRespond = value; }
        }

        protected static bool HoldOrient
        {
            get { return Ta.HoldOrient; }
            set { Ta.HoldOrient = value; }
        }

        public PanelTranslation()
        {
            this.fileName = new FileName("translation", "cfg", HydroJebCore.panelSaveFolder);
        }

        protected override void SetDefaultWindowRect()
        {
            this.windowRect = WindowPositions.translation;
        }

        protected override void MakeAPSave()
        {
            Ta.MakeSaveAtNextUpdate();
        }

        protected override void WindowGUI(int windowId)
        {
            if (this.Settings) { DrawSettingsUI(); }
            else
            {
                GUILayout.Label("Translation direction");
                GUILayout.TextArea(TransMode == APTranslation.TransDir.ADVANCED ? ThrustVector.ToString("#0.00") : TransMode.ToString());
                GUILayout.Label("Thrust rate: " + ThrustRate.ToString("#0.00"));
                GUILayout.Label("Respond to main throttle: " + Respond);
                GUILayout.Label("Hold current orientation: " + HoldOrient);

                if (GUILayout.Button("Change settings")) { this.Settings = true; }

                if (LayoutEngageBtn(this.Engaged)) { this.Engaged = !this.Engaged; }
            }

            GUI.DragWindow();
        }
    }
}
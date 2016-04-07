using HydroTech_FC;
using HydroTech_RCS.Autopilots;
using HydroTech_RCS.Constants;
using UnityEngine;

namespace HydroTech_RCS.Panels
{
    public partial class PanelPreciseControl : PanelwAP
    {
        protected override int PanelID
        {
            get { return CoreConsts.preciseControl; }
        }

        public override string PanelTitle
        {
            get { return PanelConsts.preciseControlTitle; }
        }

        protected static APPreciseControl Pc
        {
            get { return APPreciseControl.TheAutopilot; }
        }

        protected override bool Engaged
        {
            get { return Pc.Engaged; }
            set { Pc.Engaged = value; }
        }

        protected static bool ByRate
        {
            get { return Pc.byRate; }
            set { Pc.byRate = value; }
        }

        protected static float RRate
        {
            get { return Pc.rotationRate; }
            set { Pc.rotationRate = value; }
        }

        protected static float Rate
        {
            get { return Pc.translationRate; }
            set { Pc.translationRate = value; }
        }

        protected static float AngA
        {
            get { return Pc.angularAcc; }
            set { Pc.angularAcc = value; }
        }

        protected static float Acc
        {
            get { return Pc.acc; }
            set { Pc.acc = value; }
        }

        public PanelPreciseControl()
        {
            this.fileName = new FileName("precise", "cfg", HydroJebCore.panelSaveFolder);
        }

        protected override void SetDefaultWindowRect()
        {
            this.windowRect = PanelConsts.preciseControl;
        }

        protected override void MakeAPSave()
        {
            Pc.MakeSaveAtNextUpdate();
        }

        protected override void WindowGUI(int windowId)
        {
            if (this.Settings) { DrawSettingsUI(); }
            else
            {
                if (ByRate)
                {
                    GUILayout.Label("Rotation thrust rate: " + RRate.ToString("#0.000"));
                    GUILayout.Label("Translation thrust rate: " + Rate.ToString("#0.000"));
                }
                else
                {
                    GUILayout.Label("Angular Acc: " + AngA.ToString("#0.000") + GeneralConsts.angularAcc);
                    GUILayout.Label("Acceleration: " + Acc.ToString("#0.000") + GeneralConsts.acceleration);
                }
                if (GUILayout.Button("Change settings")) { this.Settings = true; }
                if (LayoutEngageBtn(this.Engaged)) { this.Engaged = !this.Engaged; }
            }

            GUI.DragWindow();
        }
    }
}
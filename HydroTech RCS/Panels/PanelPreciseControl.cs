using HydroTech_FC;
using HydroTech_RCS.Autopilots;
using HydroTech_RCS.Constants;
using UnityEngine;

namespace HydroTech_RCS.Panels
{
    public class PanelPreciseControl : PanelAP
    {
        #region Static properties
        protected static APPreciseControl PC
        {
            get { return APPreciseControl.TheAutopilot; }
        }

        protected static bool ByRate
        {
            get { return PC.byRate; }
            set { PC.byRate = value; }
        }

        protected static float RRate
        {
            get { return PC.rotationRate; }
            set { PC.rotationRate = value; }
        }

        protected static float Rate
        {
            get { return PC.translationRate; }
            set { PC.translationRate = value; }
        }

        protected static float AngA
        {
            get { return PC.angularAcc; }
            set { PC.angularAcc = value; }
        }

        protected static float Acc
        {
            get { return PC.acc; }
            set { PC.acc = value; }
        }
        #endregion

        #region Fields
        protected string accText;
        protected string angAText;
        protected string rateText;
        protected string rRateText;
        protected bool tempByRate;
        #endregion

        #region Properties
        protected override int PanelID
        {
            get { return CoreConsts.preciseControl; }
        }

        public override string PanelTitle
        {
            get { return PanelConsts.preciseControlTitle; }
        }

        protected override bool Engaged
        {
            get { return PC.Engaged; }
            set { PC.Engaged = value; }
        }

        protected override bool Settings
        {
            set
            {
                if (value != this.settings)
                {
                    if (value)
                    {
                        this.tempByRate = ByRate;
                        this.rRateText = RRate.ToString("#0.000");
                        this.rateText = Rate.ToString("#0.000");
                        this.angAText = AngA.ToString("#0.000");
                        this.accText = Acc.ToString("#0.000");
                    }
                    else
                    {
                        ByRate = this.tempByRate;
                        float temp;
                        if (float.TryParse(this.rRateText, out temp) && temp >= 0 && temp <= 1) { RRate = temp; }
                        if (float.TryParse(this.rateText, out temp) && temp >= 0 && temp <= 1) { Rate = temp; }
                        if (float.TryParse(this.angAText, out temp) && temp >= 0) { AngA = temp; }
                        if (float.TryParse(this.accText, out temp) && temp >= 0) { Acc = temp; }
                    }
                }
                base.Settings = value;
            }
        }
        #endregion

        #region Constructor
        public PanelPreciseControl()
        {
            this.fileName = new FileName("precise", "cfg", HydroJebCore.panelSaveFolder);
        }
        #endregion

        #region Overrides
        protected override void MakeAPSave()
        {
            PC.MakeSaveAtNextUpdate();
        }

        protected override void SetDefaultWindowRect()
        {
            this.windowRect = PanelConsts.preciseControl;
        }

        protected override void WindowGUI(int windowId)
        {
            if (this.Settings) { DrawSettingsUI(); }
            else
            {
                if (ByRate)
                {
                    GUILayout.Label(string.Format("Rotation thrust rate: {0:#0.000}", RRate));
                    GUILayout.Label(string.Format("Translation thrust rate: {0:#0.000}", Rate));
                }
                else
                {
                    GUILayout.Label(string.Format("Angular Acc: {0:#0.000}{1}", AngA, GeneralConsts.angularAcc));
                    GUILayout.Label(string.Format("Acceleration: {0:#0.000}{1}", Acc, GeneralConsts.acceleration));
                }
                if (GUILayout.Button("Change settings"))
                {
                    this.Settings = true;
                }
                if (LayoutEngageBtn(this.Engaged)) { this.Engaged = !this.Engaged; }
            }

            GUI.DragWindow();
        }

        protected override void DrawSettingsUI()
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("by Rate", this.tempByRate ? GUIUtils.ButtonStyle(Color.green) : GUIUtils.ButtonStyle()))
            {
                this.tempByRate = true;
            }
            if (GUILayout.Button("by Acceleration", this.tempByRate ? GUIUtils.ButtonStyle() : GUIUtils.ButtonStyle(Color.green)))
            {
                this.tempByRate = false;
            }
            GUILayout.EndHorizontal();
            if (this.tempByRate)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Rotation thrust rate");
                this.rRateText = GUILayout.TextField(this.rRateText);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("Translation thrust rate");
                this.rateText = GUILayout.TextField(this.rateText);
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Angular Acc");
                this.angAText = GUILayout.TextField(this.angAText);
                GUILayout.Label(GeneralConsts.angularAcc);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("Acceleration");
                this.accText = GUILayout.TextField(this.accText);
                GUILayout.Label(GeneralConsts.acceleration);
                GUILayout.EndHorizontal();
            }

            base.DrawSettingsUI();
        }
        #endregion
    }
}
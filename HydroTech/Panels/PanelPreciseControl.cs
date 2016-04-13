using HydroTech.Autopilots;
using HydroTech.Managers;
using HydroTech.Storage;
using HydroTech.Utils;
using UnityEngine;

namespace HydroTech.Panels
{
    public class PanelPreciseControl : PanelAP
    {
        #region Static properties
        private static APPreciseControl PC
        {
            get { return HydroFlightManager.Instance.PreciseControlAutopilot; }
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
        public override string PanelTitle
        {
            get { return "Precise Control"; }
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

        private readonly int id;
        protected override int ID
        {
            get { return this.id; }
        }
        #endregion

        #region Constructor
        public PanelPreciseControl()
        {
            this.fileName = new FileName("precise", "cfg", FileName.panelSaveFolder);
            this.id = GuidProvider.GetGuid<PanelPreciseControl>();
        }
        #endregion

        #region Overrides
        protected override void MakeAPSave()
        {
            PC.MakeSaveAtNextUpdate();
        }

        protected override void SetDefaultWindowRect()
        {
            this.windowRect = new Rect(349, 60, 200, 122);
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
                    GUILayout.Label(string.Format("Angular Acc: {0:#0.000}rad/s²", AngA));
                    GUILayout.Label(string.Format("Acceleration: {0:#0.000}m/s²", Acc));
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
            if (GUILayout.Button("by Rate", this.tempByRate ? GUIUtils.ButtonStyle(Color.green) : GUIUtils.Skin.button))
            {
                this.tempByRate = true;
            }
            if (GUILayout.Button("by Acceleration", this.tempByRate ? GUIUtils.Skin.button : GUIUtils.ButtonStyle(Color.green)))
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
                GUILayout.Label("rad/s²");
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("Acceleration");
                this.accText = GUILayout.TextField(this.accText);
                GUILayout.Label("m/s²");
                GUILayout.EndHorizontal();
            }

            base.DrawSettingsUI();
        }
        #endregion
    }
}
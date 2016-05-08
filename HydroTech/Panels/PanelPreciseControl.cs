using HydroTech.Autopilots;
using HydroTech.Managers;
using HydroTech.Utils;
using UnityEngine;

namespace HydroTech.Panels
{
    public class PanelPreciseControl : PanelAP
    {
        #region Static properties
        private static APPreciseControl PC => HydroFlightManager.Instance.PreciseControlAutopilot;
        #endregion

        #region Fields
        private string accText;
        private string angAText;
        private string rateText;
        private string rRateText;
        private bool tempByRate;
        #endregion

        #region Properties
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
                        this.tempByRate = PC.byRate;
                        this.rRateText = PC.rotationRate.ToString("#0.000");
                        this.rateText = PC.translationRate.ToString("#0.000");
                        this.angAText = PC.angularAcc.ToString("#0.000");
                        this.accText = PC.acc.ToString("#0.000");
                    }
                    else
                    {
                        PC.byRate = this.tempByRate;
                        float temp;
                        if (float.TryParse(this.rRateText, out temp) && temp >= 0 && temp <= 1) { PC.rotationRate = temp; }
                        if (float.TryParse(this.rateText, out temp) && temp >= 0 && temp <= 1) { PC.rotationRate = temp; }
                        if (float.TryParse(this.angAText, out temp) && temp >= 0) { PC.angularAcc = temp; }
                        if (float.TryParse(this.accText, out temp) && temp >= 0) { PC.acc = temp; }
                    }
                }
                base.Settings = value;
            }
        }

        public override string Title => "Precise Control";
        #endregion

        #region Constructor
        public PanelPreciseControl() : base(new Rect(349, 60, 200, 122), IDProvider.GetID<PanelPreciseControl>(), "Precise Control") { }
        #endregion

        #region Overrides
        protected override void Window(int id)
        {
            GUI.DragWindow(this.drag);

            if (this.Settings) { DrawSettingsUI(); }
            else
            {
                if (PC.byRate)
                {
                    GUILayout.Label($"Rotation thrust rate: {PC.rotationRate:#0.000}");
                    GUILayout.Label($"Translation thrust rate: {PC.translationRate:#0.000}");
                }
                else
                {
                    GUILayout.Label($"Angular Acc: {PC.angularAcc:#0.000}rad/s²");
                    GUILayout.Label($"Acceleration: {PC.acc:#0.000}m/s²");
                }
                if (GUILayout.Button("Change settings"))
                {
                    this.Settings = true;
                }
                if (LayoutEngageBtn(this.Engaged)) { this.Engaged = !this.Engaged; }
            }
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
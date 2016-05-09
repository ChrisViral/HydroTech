using HydroTech.Autopilots;
using HydroTech.Managers;
using HydroTech.Utils;
using UnityEngine;
using TranslationDirection = HydroTech.Autopilots.APTranslation.TranslationDirection;

namespace HydroTech.Panels
{
    public class PanelTranslation : PanelAP
    {
        #region Static Properties
        private static APTranslation TA => HydroFlightManager.Instance.TranslationAutopilot;
        #endregion

        #region Fields
        private string rateText;
        private bool tempHoldOrient, tempRespond;
        private TranslationDirection tempTransMode;
        private string xText, yText, zText;
        #endregion

        #region Properties
        protected override bool Engaged
        {
            get { return TA.Engaged; }
            set { TA.Engaged = value; }
        }

        protected override bool Settings
        {
            set
            {
                if (value != this.settings)
                {
                    if (value)
                    {
                        this.tempTransMode = TA.TransMode;
                        this.tempRespond = TA.mainThrottleRespond;
                        this.tempHoldOrient = TA.HoldOrient;
                        this.xText = TA.thrustVector.x.ToString("#0.00");
                        this.yText = TA.thrustVector.y.ToString("#0.00");
                        this.zText = TA.thrustVector.z.ToString("#0.00");
                        this.rateText = TA.thrustRate.ToString("#0.0");
                    }
                    else
                    {
                        TA.TransMode = this.tempTransMode;
                        TA.mainThrottleRespond = this.tempRespond;
                        TA.HoldOrient = this.tempHoldOrient;
                        if (this.tempTransMode == TranslationDirection.ADVANCED)
                        {
                            float x, y, z;
                            if (float.TryParse(this.xText, out x) && float.TryParse(this.yText, out y) && float.TryParse(this.zText, out z) && (x != 0 || y != 0 || z != 0))
                            {
                                TA.thrustVector = new Vector3(x, y, z).normalized;
                            }
                        }
                        float temp;
                        if (float.TryParse(this.rateText, out temp) && temp >= 0 && temp <= 1) { TA.thrustRate = temp; }
                    }
                }
                base.Settings = value;
            }
        }

        public override string Title => "Translation Autopilot";
        #endregion

        #region Constructor
        public PanelTranslation() : base(new Rect(142, 475, 200, 260), IDProvider.GetID<PanelTranslation>()) { }
        #endregion

        #region Overrides
        protected override void Window(int id)
        {
            GUI.DragWindow(this.drag);

            if (this.Settings) { DrawSettings(); }
            else
            {
                GUILayout.Label("Translation direction");
                GUILayout.TextArea(TA.TransMode == TranslationDirection.ADVANCED ? TA.thrustVector.ToString("#0.00") : EnumUtils.GetName(TA.TransMode));
                GUILayout.Label($"Thrust rate: {TA.thrustRate:#0.00}");
                GUILayout.Label("Respond to main throttle: " + TA.mainThrottleRespond);
                GUILayout.Label("Hold current orientation: " + TA.HoldOrient);

                if (GUILayout.Button("Change settings"))
                {
                    this.Settings = true;
                }
                if (EngageButton(this.Engaged)) { this.Engaged = !this.Engaged; }
            }
        }

        protected override void DrawSettings()
        {
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            for (int i = 0; i <= 6; i++)
            {
                TranslationDirection x = (TranslationDirection)i;
                if (GUILayout.Button(x.ToString(), this.tempTransMode == x ? GUIUtils.ButtonStyle(Color.green) : GUIUtils.Skin.button))
                {
                    this.tempTransMode = x;
                    if (i != 6) { ResetHeight(); }
                }
                if (i % 2 == 1)
                {
                    GUILayout.EndVertical();
                    if (i != 5) { GUILayout.BeginVertical(); }
                    else { GUILayout.EndHorizontal(); }
                }
            }
            if (this.tempTransMode == TranslationDirection.ADVANCED)
            {
                GUILayout.BeginVertical();
                GUILayout.Label("X(+RIGHT)=");
                this.xText = GUILayout.TextField(this.xText);
                GUILayout.EndHorizontal();

                GUILayout.BeginVertical();
                GUILayout.Label("Y(+DOWN)=");
                this.yText = GUILayout.TextField(this.yText);
                GUILayout.EndHorizontal();

                GUILayout.BeginVertical();
                GUILayout.Label("Z(+FORWARD)=");
                this.zText = GUILayout.TextField(this.zText);
                GUILayout.EndHorizontal();

                float x, y, z;
                if (float.TryParse(this.xText, out x) && float.TryParse(this.yText, out y) && float.TryParse(this.zText, out z) && (x != 0 || y != 0 || z != 0))
                {
                    Vector3 tempThrustVector = new Vector3(x, y, z).normalized;
                    GUILayout.Label("Normalized vector:");
                    GUILayout.Label(tempThrustVector.ToString("#0.00"));
                }
                else { GUILayout.Label("Invalid input", GUIUtils.ColouredLabel(Color.red)); }
            }
            GUILayout.BeginHorizontal();
            GUILayout.Label("Thrust rate (0-1)");
            this.rateText = GUILayout.TextField(this.rateText);
            GUILayout.EndHorizontal();
            this.tempRespond = GUILayout.Toggle(this.tempRespond, "Respond to main throttle");
            this.tempHoldOrient = GUILayout.Toggle(this.tempHoldOrient, "Hold current orientation");
            base.DrawSettings();
        }
        #endregion
    }
}
using HydroTech.Autopilots;
using HydroTech.Managers;
using HydroTech.Storage;
using HydroTech.Utils;
using UnityEngine;
using TransDir = HydroTech.Autopilots.APTranslation.TransDir;

namespace HydroTech.Panels
{
    public class PanelTranslation : PanelAP
    {
        #region Static Properties
        private static APTranslation TA
        {
            get { return HydroFlightManager.Instance.TranslationAutopilot; }
        }

        protected static TransDir TransMode
        {
            get { return TA.TransMode; }
            set { TA.TransMode = value; }
        }

        protected static Vector3 ThrustVector
        {
            get { return TA.thrustVector; }
            set { TA.thrustVector = value; }
        }

        protected static float ThrustRate
        {
            get { return TA.thrustRate; }
            set { TA.thrustRate = value; }
        }

        protected static bool Respond
        {
            get { return TA.mainThrottleRespond; }
            set { TA.mainThrottleRespond = value; }
        }

        protected static bool HoldOrient
        {
            get { return TA.HoldOrient; }
            set { TA.HoldOrient = value; }
        }
        #endregion

        #region Fields
        protected string rateText;
        protected bool tempHoldOrient;
        protected bool tempRespond;
        protected TransDir tempTransMode;
        protected string xText;
        protected string yText;
        protected string zText;
        #endregion

        #region Properties
        public override string PanelTitle
        {
            get { return "Auto Translation"; }
        }

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
                        this.tempTransMode = TransMode;
                        this.tempRespond = Respond;
                        this.tempHoldOrient = HoldOrient;
                        this.xText = ThrustVector.x.ToString("#0.00");
                        this.yText = ThrustVector.y.ToString("#0.00");
                        this.zText = ThrustVector.z.ToString("#0.00");
                        this.rateText = ThrustRate.ToString("#0.0");
                    }
                    else
                    {
                        TransMode = this.tempTransMode;
                        Respond = this.tempRespond;
                        HoldOrient = this.tempHoldOrient;
                        if (this.tempTransMode == TransDir.ADVANCED)
                        {
                            float x, y, z;
                            if (float.TryParse(this.xText, out x) && float.TryParse(this.yText, out y) && float.TryParse(this.zText, out z) && (x != 0 || y != 0 || z != 0))
                            {
                                ThrustVector = new Vector3(x, y, z).normalized;
                            }
                        }
                        float temp;
                        if (float.TryParse(this.rateText, out temp) && temp >= 0 && temp <= 1) { ThrustRate = temp; }
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
        public PanelTranslation()
        {
            this.fileName = new FileName("translation", "cfg", FileName.panelSaveFolder);
            this.id = GuidProvider.GetGuid<PanelTranslation>();
        }
        #endregion

        #region Overrides
        protected override void SetDefaultWindowRect()
        {
            this.windowRect = new Rect(142, 475, 200, 260);
        }

        protected override void MakeAPSave()
        {
            TA.MakeSaveAtNextUpdate();
        }

        protected override void WindowGUI(int windowId)
        {
            if (this.Settings) { DrawSettingsUI(); }
            else
            {
                GUILayout.Label("Translation direction");
                GUILayout.TextArea(TransMode == TransDir.ADVANCED ? ThrustVector.ToString("#0.00") : TransMode.ToString());
                GUILayout.Label("Thrust rate: " + ThrustRate.ToString("#0.00"));
                GUILayout.Label("Respond to main throttle: " + Respond);
                GUILayout.Label("Hold current orientation: " + HoldOrient);

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
            GUILayout.BeginVertical();
            for (int i = 0; i <= 6; i++)
            {
                TransDir x = (TransDir)i;
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
            if (this.tempTransMode == TransDir.ADVANCED)
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
            base.DrawSettingsUI();
        }
        #endregion
    }
}
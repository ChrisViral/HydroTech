using HydroTech.Autopilots;
using HydroTech.Managers;
using HydroTech.Utils;
using UnityEngine;

namespace HydroTech.Panels
{
    public class PanelLanding : PanelAP
    {
        #region Static properties
        private static APLanding LA
        {
            get { return HydroFlightManager.Instance.LandingAutopilot; }
        }
        #endregion

        #region Fields
        public PanelLandingInfo panelInfo = new PanelLandingInfo();
        protected string altKeepText;
        protected string maxThrText;
        protected string stdsText;
        protected float tempAltKeep;
        protected bool tempBurnRetro;
        protected bool tempEngines;
        protected bool tempTouchdown;
        protected bool tempUseTrueAlt;
        protected bool tempVabPod;
        #endregion

        #region Properties
        private bool TempEngines
        {
            get { return this.tempEngines; }
            set
            {
                if (value != this.tempEngines) { ResetHeight(); }
                this.tempEngines = value;
            }
        }

        private bool TempTouchdown
        {
            get { return this.tempTouchdown; }
            set
            {
                if (value != this.tempTouchdown) { ResetHeight(); }
                this.tempTouchdown = value;
            }
        }

        protected override bool Settings
        {
            set
            {
                if (value != this.settings)
                {
                    if (value) //Start settings
                    {
                        this.tempBurnRetro = LA.burnRetro;
                        this.TempEngines = LA.Engines;
                        this.TempTouchdown = LA.touchdown;
                        this.tempVabPod = LA.vabPod;
                        this.stdsText = LA.safeTouchDownSpeed.ToString("#0.0");
                        this.maxThrText = (LA.maxThrottle * 100).ToString("#0.0");
                        this.tempUseTrueAlt = LA.useTrueAlt;
                        this.tempAltKeep = LA.altKeep;
                        this.altKeepText = this.tempAltKeep.ToString("#0.0");
                    }
                    else //Apply settings
                    {
                        LA.burnRetro = this.tempBurnRetro;
                        LA.Engines = this.TempEngines;
                        LA.touchdown = this.TempTouchdown;
                        LA.vabPod = this.tempVabPod;
                        LA.useTrueAlt = this.tempUseTrueAlt;
                        LA.altKeep = this.tempAltKeep;
                        float tryParse;
                        if (float.TryParse(this.stdsText, out tryParse)) { LA.safeTouchDownSpeed = tryParse; }
                        if (float.TryParse(this.maxThrText, out tryParse) && tryParse > 0 && tryParse <= 100) { LA.maxThrottle = tryParse / 100f; }
                    }
                }
                base.Settings = value;
            }
        }

        public override bool Visible
        {
            set
            {
                base.Visible = value;
                this.panelInfo.Visible = value;
            }
        }

        protected override bool Engaged
        {
            get { return LA.Engaged; }
            set { LA.Engaged = value; }
        }
        #endregion

        #region Constructor
        public PanelLanding() : base(new Rect(548, 80, 200, 184), GuidProvider.GetGuid<PanelLanding>(), "Landing Autopilot") { }
        #endregion

        #region Overrides
        public override void OnFlightStart()
        {
            base.OnFlightStart();
            this.panelInfo.OnFlightStart();
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            this.panelInfo.OnUpdate();
        }

        protected override void Window(int id)
        {
            if (this.Settings) { DrawSettingsUI(); }
            else
            {
                GUILayout.Label(string.Format("Pod orientation: {0}", LA.vabPod ? "Up" : "Horizon"));
                GUILayout.Label(string.Format("Touchdown speed: {0:#0.0}m/s", LA.safeTouchDownSpeed));
                GUILayout.Label(string.Format("Use engines: {0}", LA.Engines ? "true" : "false"));
                if (LA.Engines)
                {
                    GUILayout.Label(string.Format("Max throttle: {0:#0.0}", LA.maxThrottle * 100));
                }
                if (LA.touchdown)
                {
                    GUILayout.Label("Perform touchdown");
                    if (GUILayout.Button(string.Format("Hover at {0:#0.00}m {1}", LA.altKeep, LA.useTrueAlt ? "AGL" : "ASL")))
                    {
                        LA.touchdown = false;
                    }
                }
                else
                {
                    GUILayout.Label(string.Format("Hover at {0:#0.00}m {1}", LA.altKeep, LA.useTrueAlt ? "AGL" : "ASL"));
                    GUILayout.Label(string.Format("Max allowed horizontal speed: {0:#0.0}m/s", LA.AltKeepTrue * 0.01f));
                    if (GUILayout.Button("Switch True/ASL"))
                    {
                        if (LA.useTrueAlt)
                        {
                            LA.altKeep = LA.AltKeepASL;
                            LA.useTrueAlt = false;
                        }
                        else
                        {
                            LA.altKeep = LA.AltKeepTrue;
                            LA.useTrueAlt = true;
                        }
                    }
                    if (GUILayout.Button("Land"))
                    {
                        LA.touchdown = true;
                        ResetHeight();
                    }
                }
                if (GUILayout.Button("Change settings"))
                {
                    this.Settings = true;
                }
                this.panelInfo.Visible = GUILayout.Toggle(this.panelInfo.Visible, "Advanced info");
                if (LayoutEngageBtn(this.Engaged)) { this.Engaged = !this.Engaged; }
                GUILayout.Label("Status: " + LA.StatusString);
                GUILayout.Label(LA.WarningString, GUIUtils.ColouredLabel(LA.WarningColor));
            }

            GUI.DragWindow();
        }

        protected override void DrawSettingsUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Pod orientation: ");
            if (GUILayout.Button(this.tempVabPod ? "Up" : "Horizon"))
            {
                this.tempVabPod = !this.tempVabPod;
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Touchdown speed");
            this.stdsText = GUILayout.TextField(this.stdsText);
            GUILayout.Label("m/s");
            GUILayout.EndHorizontal();
            this.TempEngines = GUILayout.Toggle(this.TempEngines, "Use engines");
            if (this.TempEngines)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Max throttle");
                this.maxThrText = GUILayout.TextField(this.maxThrText);
                GUILayout.EndHorizontal();
            }
            this.TempTouchdown = !GUILayout.Toggle(!this.TempTouchdown, "Hover at");
            if (!this.TempTouchdown)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("True Alt", this.tempUseTrueAlt ? GUIUtils.ButtonStyle(Color.green) : GUIUtils.Skin.button))
                {
                    if (!this.tempUseTrueAlt)
                    {
                        this.tempAltKeep -= LA.TerrainHeight;
                        if (this.tempAltKeep < APLanding.finalDescentHeight) { this.tempAltKeep = APLanding.finalDescentHeight; }
                        this.altKeepText = this.tempAltKeep.ToString("#0.0");
                    }
                    this.tempUseTrueAlt = true;
                }
                if (GUILayout.Button("ASL Alt", this.tempUseTrueAlt ? GUIUtils.Skin.button : GUIUtils.ButtonStyle(Color.green)))
                {
                    if (this.tempUseTrueAlt)
                    {
                        this.tempAltKeep += LA.TerrainHeight;
                        this.altKeepText = this.tempAltKeep.ToString("#0.0");
                    }
                    this.tempUseTrueAlt = false;
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                this.altKeepText = GUILayout.TextField(this.altKeepText);
                GUILayout.Label("m");
                GUILayout.EndHorizontal();
                float temp;
                if (this.tempUseTrueAlt)
                {
                    if (float.TryParse(this.altKeepText, out temp) && temp >= APLanding.finalDescentHeight)
                    {
                        this.tempAltKeep = temp;
                        float tempAltKeepAsl = this.tempAltKeep + LA.TerrainHeight;
                        GUILayout.Label(string.Format("ASL alt: {0:#0.0}m", tempAltKeepAsl));
                        GUILayout.Label(string.Format("Max allowed horizontal speed: {0:#0.0}m/s", 0.01f * this.tempAltKeep));
                    }
                    else { GUILayout.Label("Invalid altitude", GUIUtils.ColouredLabel(Color.red)); }
                }
                else
                {
                    if (float.TryParse(this.altKeepText, out temp))
                    {
                        this.tempAltKeep = temp;
                        float tempAltKeepTrue = Mathf.Max(this.tempAltKeep - LA.TerrainHeight, APLanding.finalDescentHeight);
                        GUILayout.Label(string.Format("True alt: {0:#0.0}m", tempAltKeepTrue));
                        GUILayout.Label(string.Format("Max allowed horizontal speed: {0:#0.0}m/s", 0.01f * tempAltKeepTrue));
                    }
                    else { GUILayout.Label("Invalid altitude", GUIUtils.ColouredLabel(Color.red)); }
                }
            }

            base.DrawSettingsUI();
        }
        #endregion
    }
}
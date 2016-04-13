using HydroTech.Autopilots;
using HydroTech.Managers;
using HydroTech.Storage;
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

        protected static float SafeTouchDownSpeed
        {
            get { return LA.safeTouchDownSpeed; }
            set { LA.safeTouchDownSpeed = value; }
        }

        protected static bool VabPod
        {
            get { return LA.vabPod; }
            set { LA.vabPod = value; }
        }

        protected static bool Engines
        {
            get { return LA.engines; }
            set { LA.engines = value; }
        }

        protected static bool BurnRetro
        {
            get { return LA.burnRetro; }
            set { LA.burnRetro = value; }
        }

        protected static float MaxThrottle
        {
            get { return LA.MaxThrottle; }
            set { LA.MaxThrottle = value; }
        }

        protected static bool Touchdown
        {
            get { return LA.touchdown; }
            set { LA.touchdown = value; }
        }

        protected static bool UseTrueAlt
        {
            get { return LA.useTrueAlt; }
            set { LA.useTrueAlt = value; }
        }

        protected static float AltKeep
        {
            get { return LA.altKeep; }
            set { LA.altKeep = value; }
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
        protected bool panelAdvInfoShown;
        protected bool PanelAdvInfoShown
        {
            set
            {
                if (value && this.panelAdvInfoShown)
                {
                    if (this.panelAdvInfoShown)
                    {
                        this.panelAdvInfoShown = false;
                        this.panelInfo.PanelShown = true;
                    }
                }
                else
                {
                    this.panelAdvInfoShown = this.panelInfo.PanelShown;
                    this.panelInfo.PanelShown = false;
                }
            }
        }

        public override bool PanelShown
        {
            set
            {
                if (value != this.PanelShown) { this.PanelAdvInfoShown = value; }
                base.PanelShown = value;
            }
        }

        protected string StatusString
        {
            get { return LA.StatusString; }
        }

        protected string WarningString
        {
            get { return LA.WarningString; }
        }

        protected Color WarningColor
        {
            get { return LA.WarningColor; }
        }

        protected float AllowedHori
        {
            get { return LA.AllowedHori; }
        }

        protected float TerrainHeight
        {
            get { return LA.TerrainHeight; }
        }

        protected float AltKeepTrue
        {
            get { return LA.AltKeepTrue; }
        }

        protected float AltKeepAsl
        {
            get { return LA.AltKeepAsl; }
        }

        protected string HoverAtString
        {
            get { return string.Format("Hover at {0:#0.00}{1} {2}", AltKeep, Constants.length, UseTrueAlt ? "AGL" : "ASL"); }
        }

        protected bool TempEngines
        {
            get { return this.tempEngines; }
            set
            {
                if (value != this.tempEngines) { ResetHeight(); }
                this.tempEngines = value;
            }
        }

        protected bool TempTouchdown
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
                        this.tempBurnRetro = BurnRetro;
                        this.TempEngines = Engines;
                        this.TempTouchdown = Touchdown;
                        this.tempVabPod = VabPod;
                        this.stdsText = SafeTouchDownSpeed.ToString("#0.0");
                        this.maxThrText = MaxThrottle.ToString("#0.0");
                        this.tempUseTrueAlt = UseTrueAlt;
                        this.tempAltKeep = AltKeep;
                        this.altKeepText = this.tempAltKeep.ToString("#0.0");
                    }
                    else //Apply settings
                    {
                        BurnRetro = this.tempBurnRetro;
                        Engines = this.TempEngines;
                        Touchdown = this.TempTouchdown;
                        VabPod = this.tempVabPod;
                        UseTrueAlt = this.tempUseTrueAlt;
                        AltKeep = this.tempAltKeep;
                        float tryParse;
                        if (float.TryParse(this.stdsText, out tryParse)) { SafeTouchDownSpeed = tryParse; }
                        if (float.TryParse(this.maxThrText, out tryParse) && tryParse > 0 && tryParse <= 100) { MaxThrottle = tryParse; }
                    }
                }
                base.Settings = value;
            }
        }

        public override bool Active
        {
            set
            {
                base.Active = value;
                this.panelInfo.Active = value;
            }
        }

        public override string PanelTitle
        {
            get { return "Landing Autopilot"; }
        }

        protected override bool Engaged
        {
            get { return LA.Engaged; }
            set { LA.Engaged = value; }
        }

        private readonly int id;
        protected override int ID
        {
            get { return this.id; }
        }
        #endregion

        #region Constructor
        public PanelLanding()
        {
            this.fileName = new FileName("landing", "cfg", FileName.panelSaveFolder);
            this.id = GuidProvider.GetGuid<PanelLanding>();
        }
        #endregion

        #region Overrides
        protected override void MakeAPSave()
        {
            LA.MakeSaveAtNextUpdate();
        }

        public override void OnFlightStart()
        {
            base.OnFlightStart();
            this.panelInfo.OnFlightStart();
            this.panelAdvInfoShown = false;
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            this.panelInfo.OnUpdate();
        }

        protected override void SetDefaultWindowRect()
        {
            this.windowRect = new Rect(548, 80, 200, 184);
        }

        protected override void WindowGUI(int windowId)
        {
            if (this.Settings) { DrawSettingsUI(); }
            else
            {
                GUILayout.Label("Pod orientation: " + (VabPod ? "Up" : "Horizon"));
                GUILayout.Label(string.Format("Touchdown speed: {0:#0.0}{1}", SafeTouchDownSpeed, Constants.speedSimple));
                GUILayout.Label("Use engines: " + (Engines ? "true" : "false"));
                if (Engines)
                {
                    GUILayout.Label("Max throttle: " + MaxThrottle.ToString("#0.0"));
                }
                if (Touchdown)
                {
                    GUILayout.Label("Perform touchdown");
                    if (GUILayout.Button(this.HoverAtString))
                    {
                        Touchdown = false;
                    }
                }
                else
                {
                    GUILayout.Label(this.HoverAtString);
                    GUILayout.Label("Max allowed horizontal speed: " + this.AllowedHori.ToString("#0.0") + Constants.speedSimple);
                    if (GUILayout.Button("Switch True/ASL"))
                    {
                        if (UseTrueAlt)
                        {
                            AltKeep = this.AltKeepAsl;
                            UseTrueAlt = false;
                        }
                        else
                        {
                            AltKeep = this.AltKeepTrue;
                            UseTrueAlt = true;
                        }
                    }
                    if (GUILayout.Button("Land"))
                    {
                        Touchdown = true;
                        ResetHeight();
                    }
                }
                if (GUILayout.Button("Change settings"))
                {
                    this.Settings = true;
                }
                this.panelInfo.PanelShown = GUILayout.Toggle(this.panelInfo.PanelShown, "Advanced info");
                if (LayoutEngageBtn(this.Engaged)) { this.Engaged = !this.Engaged; }
                GUILayout.Label("Status: " + this.StatusString);
                GUILayout.Label(this.WarningString, GUIUtils.ColouredLabel(this.WarningColor));
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
            GUILayout.Label(Constants.speedSimple);
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
                        this.tempAltKeep -= this.TerrainHeight;
                        if (this.tempAltKeep < Constants.finalDescentHeight) { this.tempAltKeep = Constants.finalDescentHeight; }
                        this.altKeepText = this.tempAltKeep.ToString("#0.0");
                    }
                    this.tempUseTrueAlt = true;
                }
                if (GUILayout.Button("ASL Alt", this.tempUseTrueAlt ? GUIUtils.Skin.button : GUIUtils.ButtonStyle(Color.green)))
                {
                    if (this.tempUseTrueAlt)
                    {
                        this.tempAltKeep += this.TerrainHeight;
                        this.altKeepText = this.tempAltKeep.ToString("#0.0");
                    }
                    this.tempUseTrueAlt = false;
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                this.altKeepText = GUILayout.TextField(this.altKeepText);
                GUILayout.Label(Constants.length);
                GUILayout.EndHorizontal();
                float temp;
                if (this.tempUseTrueAlt)
                {
                    if (float.TryParse(this.altKeepText, out temp) && temp >= Constants.finalDescentHeight)
                    {
                        this.tempAltKeep = temp;
                        float tempAltKeepAsl = this.tempAltKeep + this.TerrainHeight;
                        GUILayout.Label(string.Format("ASL alt: {0:#0.0}{1}", tempAltKeepAsl, Constants.length));
                        GUILayout.Label(string.Format("Max allowed horizontal speed: {0:#0.0}{1}", LA.AllowedHoriSpeed(this.tempAltKeep), Constants.speedSimple));
                    }
                    else { GUILayout.Label("Invalid altitude", GUIUtils.ColouredLabel(Color.red)); }
                }
                else
                {
                    if (float.TryParse(this.altKeepText, out temp))
                    {
                        this.tempAltKeep = temp;
                        float tempAltKeepTrue = Mathf.Max(this.tempAltKeep - this.TerrainHeight, Constants.finalDescentHeight);
                        GUILayout.Label(string.Format("True alt: {0:#0.0}{1}", tempAltKeepTrue, Constants.length));
                        GUILayout.Label(string.Format("Max allowed horizontal speed: {0:#0.0}{1}", LA.AllowedHoriSpeed(tempAltKeepTrue), Constants.speedSimple));
                    }
                    else { GUILayout.Label("Invalid altitude", GUIUtils.ColouredLabel(Color.red)); }
                }
            }

            base.DrawSettingsUI();
        }
        #endregion
    }
}
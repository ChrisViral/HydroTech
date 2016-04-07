using HydroTech_FC;
using HydroTech_RCS.Autopilots;
using HydroTech_RCS.Constants.Core;
using HydroTech_RCS.Constants.Panels;
using HydroTech_RCS.Constants.Units;
using UnityEngine;

namespace HydroTech_RCS.Panels
{
    public partial class PanelLanding : PanelwAP
    {
        public PanelLandingAdvInfo panelAdvInfo = new PanelLandingAdvInfo();
        protected bool panelAdvInfoShown;

        public static PanelLanding ThePanel
        {
            get { return (PanelLanding)HydroJebCore.panels[PanelIDs.landing]; }
        }

        protected override int PanelID
        {
            get { return PanelIDs.landing; }
        }

        public override string PanelTitle
        {
            get { return PanelTitles.landing; }
        }

        protected static APLanding La
        {
            get { return APLanding.TheAutopilot; }
        }

        protected override bool Engaged
        {
            get { return La.Engaged; }
            set { La.Engaged = value; }
        }

        protected static float SafeTouchDownSpeed
        {
            get { return La.safeTouchDownSpeed; }
            set { La.safeTouchDownSpeed = value; }
        }

        protected static bool VabPod
        {
            get { return La.vabPod; }
            set { La.vabPod = value; }
        }

        protected static bool Engines
        {
            get { return La.engines; }
            set { La.engines = value; }
        }

        protected static bool BurnRetro
        {
            get { return La.burnRetro; }
            set { La.burnRetro = value; }
        }

        protected static float MaxThrottle
        {
            get { return La.MaxThrottle; }
            set { La.MaxThrottle = value; }
        }

        protected static bool Touchdown
        {
            get { return La.touchdown; }
            set { La.touchdown = value; }
        }

        protected static bool UseTrueAlt
        {
            get { return La.useTrueAlt; }
            set { La.useTrueAlt = value; }
        }

        protected static float AltKeep
        {
            get { return La.altKeep; }
            set { La.altKeep = value; }
        }

        protected string StatusString
        {
            get { return La.StatusString; }
        }

        protected string WarningString
        {
            get { return La.WarningString; }
        }

        protected Color WarningColor
        {
            get { return La.WarningColor; }
        }

        protected float AllowedHori
        {
            get { return La.AllowedHori; }
        }

        protected float TerrainHeight
        {
            get { return La.TerrainHeight; }
        }

        protected float AltKeepTrue
        {
            get { return La.AltKeepTrue; }
        }

        protected float AltKeepAsl
        {
            get { return La.AltKeepAsl; }
        }

        protected string HoverAtString
        {
            get { return "Hover at " + AltKeep.ToString("#0.0") + UnitStrings.length + " " + (UseTrueAlt ? "True" : "ASL"); }
        }

        protected bool PanelAdvInfoShown
        {
            set
            {
                if (value)
                {
                    if (this.panelAdvInfoShown)
                    {
                        this.panelAdvInfoShown = false;
                        this.panelAdvInfo.PanelShown = true;
                    }
                }
                else
                {
                    this.panelAdvInfoShown = this.panelAdvInfo.PanelShown;
                    this.panelAdvInfo.PanelShown = false;
                }
            }
        }

        public override bool Active
        {
            set
            {
                base.Active = value;
                this.panelAdvInfo.Active = value;
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

        public PanelLanding()
        {
            this.fileName = new FileName("landing", "cfg", HydroJebCore.panelSaveFolder);
        }

        protected override void SetDefaultWindowRect()
        {
            this.windowRect = WindowPositions.landing;
        }

        protected override void MakeAPSave()
        {
            La.MakeSaveAtNextUpdate();
        }

        protected override void WindowGUI(int windowId)
        {
            if (this.Settings) { DrawSettingsUI(); }
            else
            {
                GUILayout.Label("Pod orientation: " + (VabPod ? "Up" : "Horizon"));
                GUILayout.Label("Touchdown speed: " + SafeTouchDownSpeed.ToString("#0.0") + UnitStrings.speedSimple);
                GUILayout.Label("Use engines: " + (Engines ? "true" : "false"));
                if (Engines)
                {
                    GUILayout.Label("Max throttle: " + MaxThrottle.ToString("#0.0"));
                    //GUILayout.Label("Burn retrograde: " + (burnRetro ? "true" : "false"));
                }
                if (Touchdown)
                {
                    GUILayout.Label("Perform touchdown");
                    if (GUILayout.Button(this.HoverAtString)) { Touchdown = false; }
                }
                else
                {
                    GUILayout.Label(this.HoverAtString);
                    GUILayout.Label("Max allowed horizontal speed: " + this.AllowedHori.ToString("#0.0") + UnitStrings.speedSimple);
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
                if (GUILayout.Button("Change settings")) { this.Settings = true; }
                this.panelAdvInfo.PanelShown = GUILayout.Toggle(this.panelAdvInfo.PanelShown, "Advanced info");
                if (LayoutEngageBtn(this.Engaged)) { this.Engaged = !this.Engaged; }
                GUILayout.Label("Status: " + this.StatusString);
                GUILayout.Label(this.WarningString, LabelStyle(this.WarningColor));
            }

            GUI.DragWindow();
        }

        public override void onFlightStart()
        {
            base.onFlightStart();
            this.panelAdvInfo.OnFlightStart();
            this.panelAdvInfoShown = false;
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            this.panelAdvInfo.OnUpdate();
        }
    }
}
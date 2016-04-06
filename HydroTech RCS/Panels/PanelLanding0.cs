using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HydroTech_RCS.Panels
{
    using UnityEngine;
    using HydroTech_FC;
    using Autopilots;
    using Constants.Core;
    using Constants.Panels;
    using Constants.Units;

    public partial class PanelLanding : PanelwAP
    {
        public static PanelLanding thePanel { get { return (PanelLanding)HydroJebCore.panels[PanelIDs.Landing]; } }

        public PanelLanding()
        {
            fileName = new FileName("landing", "cfg", HydroJebCore.PanelSaveFolder);
        }

        protected override int PanelID { get { return PanelIDs.Landing; } }
        public override string PanelTitle { get { return PanelTitles.Landing; } }

        protected override void SetDefaultWindowRect() { windowRect = WindowPositions.Landing; }

        protected static APLanding LA { get { return APLanding.theAutopilot; } }

        protected override void MakeAPSave() { LA.MakeSaveAtNextUpdate(); }

        protected override bool Engaged
        {
            get { return LA.engaged; }
            set { LA.engaged = value; }
        }
        protected static float safeTouchDownSpeed
        {
            get { return LA.safeTouchDownSpeed; }
            set { LA.safeTouchDownSpeed = value; }
        }
        protected static bool VABPod
        {
            get { return LA.VABPod; }
            set { LA.VABPod = value; }
        }
        protected static bool Engines
        {
            get { return LA._Engines; }
            set { LA._Engines = value; }
        }
        protected static bool burnRetro
        {
            get { return LA.burnRetro; }
            set { LA.burnRetro = value; }
        }
        protected static float MaxThrottle
        {
            get { return LA.MaxThrottle; }
            set { LA.MaxThrottle = value; }
        }
        protected static bool touchdown
        {
            get { return LA.touchdown; }
            set { LA.touchdown = value; }
        }
        protected static bool useTrueAlt
        {
            get { return LA.useTrueAlt; }
            set { LA.useTrueAlt = value; }
        }
        protected static float altKeep
        {
            get { return LA.altKeep; }
            set { LA.altKeep = value; }
        }

        protected String StatusString { get { return LA.StatusString; } }
        protected String WarningString { get { return LA.WarningString; } }
        protected Color WarningColor { get { return LA.WarningColor; } }
        protected float AllowedHori { get { return LA.AllowedHori; } }
        protected float TerrainHeight { get { return LA.TerrainHeight; } }
        protected float AltKeepTrue { get { return LA.AltKeepTrue; } }
        protected float AltKeepASL { get { return LA.AltKeepASL; } }

        protected String HoverAtString { get { return "Hover at " + altKeep.ToString("#0.0") + UnitStrings.Length + " " + (useTrueAlt ? "True" : "ASL"); } }

        protected override void WindowGUI(int windowID)
        {
            if (Settings)
                DrawSettingsUI();
            else
            {
                GUILayout.Label("Pod orientation: " + (VABPod ? "Up" : "Horizon"));
                GUILayout.Label("Touchdown speed: " + safeTouchDownSpeed.ToString("#0.0") + UnitStrings.Speed_Simple);
                GUILayout.Label("Use engines: " + (Engines ? "true" : "false"));
                if (Engines)
                {
                    GUILayout.Label("Max throttle: " + MaxThrottle.ToString("#0.0"));
                    //GUILayout.Label("Burn retrograde: " + (burnRetro ? "true" : "false"));
                }
                if (touchdown)
                {
                    GUILayout.Label("Perform touchdown");
                    if (GUILayout.Button(HoverAtString))
                        touchdown = false;
                }
                else
                {
                    GUILayout.Label(HoverAtString);
                    GUILayout.Label("Max allowed horizontal speed: " + AllowedHori.ToString("#0.0") + UnitStrings.Speed_Simple);
                    if (GUILayout.Button("Switch True/ASL"))
                    {
                        if (useTrueAlt)
                        {
                            altKeep = AltKeepASL;
                            useTrueAlt = false;
                        }
                        else
                        {
                            altKeep = AltKeepTrue;
                            useTrueAlt = true;
                        }
                    }
                    if (GUILayout.Button("Land"))
                    {
                        touchdown = true;
                        ResetHeight();
                    }
                }
                if (GUILayout.Button("Change settings"))
                    Settings = true;
                panelAdvInfo.PanelShown = GUILayout.Toggle(panelAdvInfo.PanelShown, "Advanced info");
                if (LayoutEngageBtn(Engaged))
                    Engaged = !Engaged;
                GUILayout.Label("Status: " + StatusString);
                GUILayout.Label(WarningString, LabelStyle(WarningColor));
            }

            GUI.DragWindow();
        }

        public PanelLandingAdvInfo panelAdvInfo = new PanelLandingAdvInfo();
        protected bool _PanelAdvInfoShown = false;
        protected bool PanelAdvInfoShown
        {
            set
            {
                if (value)
                {
                    if (_PanelAdvInfoShown)
                    {
                        _PanelAdvInfoShown = false;
                        panelAdvInfo.PanelShown = true;
                    }
                }
                else
                {
                    _PanelAdvInfoShown = panelAdvInfo.PanelShown;
                    panelAdvInfo.PanelShown = false;
                }
            }
        }

        public override bool Active
        {
            set
            {
                base.Active = value;
                panelAdvInfo.Active = value;
            }
        }
        public override bool PanelShown
        {
            set
            {
                if (value != PanelShown)
                    PanelAdvInfoShown = value;
                base.PanelShown = value;
            }
        }
        public override void onFlightStart()
        {
            base.onFlightStart();
            panelAdvInfo.onFlightStart();
            _PanelAdvInfoShown = false;
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
            panelAdvInfo.OnUpdate();
        }
    }
}
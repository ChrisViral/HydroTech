using System.Collections.Generic;
using HydroTech.Managers;
using HydroTech.Utils;
using UnityEngine;

namespace HydroTech.Panels
{
    public class FlightMainPanel : MonoBehaviour
    {
        #region Instance
        public static FlightMainPanel Instance { get; private set; }
        #endregion

        #region Fields
        private Rect pos, drag;
        private bool visible, hid;
        private int id;
        #endregion

        #region Properties
        public PanelDockAssist DockAssist { get; private set; }

        public PanelLanding Landing { get; private set; }

        public PanelLandingInfo LandingInfo { get; private set; }

        public PanelMainThrottle MainThrottle { get; private set; }

        public PanelPreciseControl PreciseControl { get; private set; }

        public PanelRCSThrustInfo RCSInfo { get; private set; }

        public PanelTranslation Translation { get; private set; }
#if DEBUG
        public PanelDebug Debug { get; private set; }
#endif
        public List<Panel> Panels { get; private set; }
        #endregion

        #region Methods
        internal void ShowPanel()
        {
            if (!this.visible) { this.visible = true; }
        }

        internal void HidePanel()
        {
            if (this.visible) { this.visible = false; }
        }

        private void ShowUI()
        {
            this.hid = false;
        }

        private void HideUI()
        {
            this.hid = true;
        }

        private void Window(int id)
        {
            GUI.DragWindow(this.drag);

            GUILayout.BeginVertical(GUI.skin.box);
            this.DockAssist.Visible = GUILayout.Toggle(this.DockAssist.Visible, "Docking Autopilot");
            this.Landing.Visible = GUILayout.Toggle(this.Landing.Visible, "Landing Autopilot");
            this.MainThrottle.Visible = GUILayout.Toggle(this.MainThrottle.Visible, "Main Throttle Control");
            this.PreciseControl.Visible = GUILayout.Toggle(this.PreciseControl.Visible, "RCS Precise Control");
            this.RCSInfo.Visible = GUILayout.Toggle(this.RCSInfo.Visible, "RCS Info");
            this.Translation.Visible = GUILayout.Toggle(this.Translation.Visible, "Translation Autopilot");
#if DEBUG
            this.Debug.Visible = GUILayout.Toggle(this.Debug.Visible, "Debug");
#endif
            GUILayout.EndVertical();
            
            GUIUtils.CenteredButton("Close", HydroToolbarManager.CloseFlight, GUILayout.MaxWidth(80), GUILayout.MaxHeight(25));
        }
        #endregion

        #region Functions
        private void Awake()
        {
            if (Instance != null) { Destroy(this); return; }

            Instance = this;
            this.DockAssist = new PanelDockAssist();
            this.Landing = new PanelLanding();
            this.LandingInfo = new PanelLandingInfo();
            this.MainThrottle = new PanelMainThrottle();
            this.PreciseControl = new PanelPreciseControl();
            this.RCSInfo = new PanelRCSThrustInfo(false);
            this.Translation = new PanelTranslation();
            this.Panels = new List<Panel>(7)
            {
                this.DockAssist,
                this.Landing,
                this.LandingInfo,
                this.MainThrottle,
                this.PreciseControl,
                this.RCSInfo,
                this.Translation
            };
#if DEBUG
            this.Debug = new PanelDebug();
            this.Panels.Add(this.Debug);
#endif

            this.pos = new Rect(Screen.width * 0.2f, Screen.height * 0.2f, 250, 100);
            this.drag = new Rect(0, 0, 250, 30);
            this.id = GuidProvider.GetGuid<FlightMainPanel>();
            GameEvents.onShowUI.Add(ShowUI);
            GameEvents.onHideUI.Add(HideUI);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
                GameEvents.onShowUI.Remove(ShowUI);
                GameEvents.onHideUI.Remove(HideUI);
            }
        }

        private void OnGUI()
        {
            if (this.visible && !this.hid)
            {
                GUI.skin = GUIUtils.Skin;

                this.pos = KSPUtil.ClampRectToScreen(GUILayout.Window(this.id, this.pos, Window, "HydroTech Flight Window", GUILayout.ExpandHeight(true)));

                foreach (Panel p in this.Panels)
                {
                    if (p.Visible) { p.DrawGUI(); }
                }
            }
        }
        #endregion
    }
}

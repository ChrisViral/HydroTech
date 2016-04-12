using System.Collections.Generic;
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
        private readonly Rect pos = new Rect(Screen.width / 2f, Screen.height / 2f, 20, 20);
        private bool visible, active, hid;
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
            if (this.active && !this.visible) { this.visible = true; }
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

        internal void SetActive(bool active)
        {
            this.active = active;
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

            this.active = true;
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
                GUI.Label(this.pos, ":D", GUIUtils.Skin.label);
            }
        }
        #endregion
    }
}

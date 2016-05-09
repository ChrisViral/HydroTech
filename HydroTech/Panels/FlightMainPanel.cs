using HydroTech.Managers;
using HydroTech.Utils;
using UnityEngine;

namespace HydroTech.Panels
{
    /// <summary>
    /// The main flight GUI panel, controlling the visibility of the specific panels
    /// </summary>
    public class FlightMainPanel : MainPanel
    {
        #region Instance
        /// <summary>
        /// The current instance of the main panel
        /// </summary>
        public static FlightMainPanel Instance { get; private set; }
        #endregion

        #region Properties
        /// <summary>
        /// The current instance of the Dock Assist panel
        /// </summary>
        public PanelDockAssist DockAssist { get; private set; }

        /// <summary>
        /// The current instance of the Landing panel
        /// </summary>
        public PanelLanding Landing { get; private set; }

        /// <summary>
        /// The current instance of the Landing Info panel
        /// </summary>
        public PanelLandingInfo LandingInfo { get; private set; }

        /// <summary>
        /// The current instance of the Main Throttle panel
        /// </summary>
        public PanelMainThrottle MainThrottle { get; private set; }

        /// <summary>
        /// The current instance of the PreciseControl panel
        /// </summary>
        public PanelPreciseControl PreciseControl { get; private set; }

        /// <summary>
        /// The current instance of the RCS Info panel
        /// </summary>
        public PanelRCSThrustInfo RCSInfo { get; private set; }

        /// <summary>
        /// The current instance of the Translation panel
        /// </summary>
        public PanelTranslation Translation { get; private set; }
#if DEBUG
        /// <summary>
        /// The current instance of the Debug panel
        /// </summary>
        public PanelDebug Debug { get; private set; }
#endif
        private bool hid;
        /// <summary>
        /// This MainPanel's visibility
        /// </summary>
        protected override bool Visible => base.Visible && !this.hid;

        /// <summary>
        /// Title of this MainPanel
        /// </summary>
        protected override string Title => "HydroTech Flight Window";
        #endregion

        #region Methods
        /// <summary>
        /// Shows the flight UI (F2)
        /// </summary>
        private void ShowUI() => this.hid = false;

        /// <summary>
        /// Hides the flight UI (F2)
        /// </summary>
        private void HideUI() => this.hid = true;
        #endregion

        #region Functions
        /// <summary>
        /// Unity Awake function
        /// </summary>
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

            this.Panels.Add(this.DockAssist);
            this.Panels.Add(this.Landing);
            this.Panels.Add(this.LandingInfo);
            this.Panels.Add(this.MainThrottle);
            this.Panels.Add(this.PreciseControl);
            this.Panels.Add(this.RCSInfo);
            this.Panels.Add(this.Translation);
            this.Panels.Add(this.DockAssist);
#if DEBUG
            this.Debug = new PanelDebug();
            this.Panels.Add(this.Debug);
#endif
            this.pos = new Rect(Screen.width * 0.2f, Screen.height * 0.2f, 250, 100);
            this.drag = new Rect(0, 0, 250, 30);
            this.id = GUIUtils.GetID<FlightMainPanel>();
            this.close = HydroToolbarManager.CloseFlight;

            GameEvents.onShowUI.Add(ShowUI);
            GameEvents.onHideUI.Add(HideUI);
        }

        /// <summary>
        /// Unity OnDestroy function
        /// </summary>
        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
                GameEvents.onShowUI.Remove(ShowUI);
                GameEvents.onHideUI.Remove(HideUI);
            }
        }
        #endregion
    }
}

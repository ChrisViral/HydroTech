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
        private Rect pos = new Rect(Screen.width / 2, Screen.height / 2, 20, 20);
        private HydroJebModule module;
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
        public void SetModule(HydroJebModule module)
        {
            this.module = module;
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
            this.RCSInfo = new PanelRCSThrustInfo();
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

        }

        private void OnGUI()
        {
            GUI.Label(this.pos, ":D", GUIUtils.Skin.label);
        }
        #endregion
    }
}

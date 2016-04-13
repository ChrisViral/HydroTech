using HydroTech.Autopilots;
using HydroTech.Managers;
using HydroTech.Utils;
using UnityEngine;
using Direction = HydroTech.Autopilots.Calculators.GroundContactCalculator.Direction;

namespace HydroTech.Panels
{
    public class PanelLandingInfo : Panel
    {
        #region Static properties
        private static APLanding LA
        {
            get { return HydroFlightManager.Instance.LandingAutopilot; }
        }

        private static float SlopeN
        {
            get { return LA.groundCalc.Slope(Direction.NORTH) * HTUtils.radToDeg; }
        }

        private static float SlopeS
        {
            get { return LA.groundCalc.Slope(Direction.SOUTH) * HTUtils.radToDeg; }
        }

        private static float SlopeW
        {
            get { return LA.groundCalc.Slope(Direction.WEST) * HTUtils.radToDeg; }
        }

        private static float SlopeE
        {
            get { return LA.groundCalc.Slope(Direction.EAST) * HTUtils.radToDeg; }
        }

        private static string MainBodyName
        {
            get { return FlightGlobals.currentMainBody.bodyName; }
        }

        private static float TwrTotal
        {
            get { return LA.TwrRCS + LA.TwrEng; }
        }

        private static GUIStyle TwrLabelStyle
        {
            get
            {
                if (TwrTotal < LA.GeeASL) { return GUIUtils.ColouredLabel(Color.red); }
                return TwrTotal < LA.GeeASL * 1.5f ? GUIUtils.ColouredLabel(Color.yellow) : GUI.skin.label;
            }
        }
        #endregion

        #region Properties
        private bool slopeDetection;
        private bool SlopeDetection
        {
            get
            {
                bool detect = LA.AltASL <= 2E5f;
                if (detect != this.slopeDetection)
                {
                    ResetHeight();
                    this.slopeDetection = detect;
                }
                return this.slopeDetection;
            }
        }

        public override string PanelTitle
        {
            get { return "Surface Information"; }
        }

        private readonly int id;
        protected override int ID
        {
            get { return this.id; }
        }
        #endregion

        #region Constructor
        public PanelLandingInfo()
        {
            this.id = GuidProvider.GetGuid<PanelLandingInfo>();
        }
        #endregion

        #region Methods
        private void TripleLabel(string text = "")
        {
            GUIStyle tripleLabel = new GUIStyle(GUI.skin.label);
            tripleLabel.fixedWidth = (this.windowRect.width / 3) - tripleLabel.margin.horizontal;
            GUILayout.Label(text, tripleLabel);
        }
        #endregion

        #region Overrides
        protected override void SetDefaultWindowRect()
        {
            this.windowRect = new Rect(548, 300, 200, 0);
        }

        protected override void WindowGUI(int windowId)
        {
            GUILayout.Label("Orbiting body: " + MainBodyName);
            GUILayout.Label(string.Format("Surface g: {0:#0.00}m/s²", LA.GeeASL));
            GUILayout.Label(string.Format("RCS TWR: {0:#0.00}m/s²", LA.TwrRCS), TwrLabelStyle);
            GUILayout.Label(string.Format("Engine TWR: {0:#0.00}m/s²", LA.TwrEng), TwrLabelStyle);
            GUILayout.Label(string.Format("Altitude (AGL): {0:#0.00}m", LA.AltTrue));
            GUILayout.Label(string.Format("Vertical speed: {0:#0.00}m/s", FlightGlobals.ActiveVessel.verticalSpeed));
            GUILayout.Label(string.Format("Horizontal speed: {0:#0.00}m/s", FlightGlobals.ActiveVessel.horizontalSrfSpeed));
            if (!this.SlopeDetection)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Slope detection: ");
                GUILayout.Label("out of range", GUIUtils.ColouredLabel(Color.red));
                GUILayout.EndHorizontal();
            }
            else if (!LA.groundCalc.terrain)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Slope detection: ");
                GUILayout.Label("not available", GUIUtils.ColouredLabel(Color.red));
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.Label("Slope detection:");
                GUILayout.BeginHorizontal();
                TripleLabel();
                TripleLabel(string.Format("N {0:#0.0}°", SlopeN));
                TripleLabel();
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                TripleLabel(string.Format("W {0:#0.0}°", SlopeW));
                TripleLabel(string.Format("({0:#0}m)", LA.groundCalc.Radius));
                TripleLabel(string.Format("E {0:#0.0}°", SlopeE));
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                TripleLabel();
                TripleLabel(string.Format("S {0:#0.0}°", SlopeS));
                TripleLabel();
                GUILayout.EndHorizontal();
            }

            GUI.DragWindow();
        }
        #endregion
    }
}
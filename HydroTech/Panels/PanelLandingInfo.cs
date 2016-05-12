using HydroTech.Autopilots;
using HydroTech.Managers;
using UnityEngine;
using Direction = HydroTech.Autopilots.Calculators.GroundContactCalculator.Direction;
using static HydroTech.Utils.GUIUtils;
using static HydroTech.Utils.HTUtils;

namespace HydroTech.Panels
{
    public class PanelLandingInfo : Panel
    {
        #region Static properties
        private static APLanding LA => HydroFlightManager.Instance.LandingAutopilot;

        private static float SlopeN => LA.groundCalc.Slope(Direction.NORTH) * radToDeg;

        private static float SlopeS => LA.groundCalc.Slope(Direction.SOUTH) * radToDeg;

        private static float SlopeW => LA.groundCalc.Slope(Direction.WEST) * radToDeg;

        private static float SlopeE => LA.groundCalc.Slope(Direction.EAST) * radToDeg;

        private static float TwrTotal => LA.TwrRCS + LA.TwrEng;

        private static GUIStyle TwrLabelStyle
        {
            get
            {
                if (TwrTotal < LA.GeeASL) { return ColouredLabel(Color.red); }
                return TwrTotal < LA.GeeASL * 1.5f ? ColouredLabel(Color.yellow) : GUI.skin.label;
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

        public override string Title => "Landing Info";
        #endregion

        #region Constructor
        public PanelLandingInfo() : base(new Rect(548, 300, 200, 0), GetID<PanelLandingInfo>()) { }
        #endregion

        #region Methods
        private void TripleLabel(string text = "")
        {
            GUIStyle tripleLabel = new GUIStyle(GUI.skin.label);
            tripleLabel.fixedWidth = (this.window.width / 3) - tripleLabel.margin.horizontal;
            GUILayout.Label(text, tripleLabel);
        }
        #endregion

        #region Overrides
        protected override void Window(int id)
        {
            GUI.DragWindow(this.drag);

            GUILayout.Label("Orbiting body: " + FlightGlobals.currentMainBody.bodyName);
            GUILayout.Label($"Surface g: {LA.GeeASL:#0.00}m/s²");
            GUILayout.Label($"RCS TWR: {LA.TwrRCS:#0.00}m/s²", TwrLabelStyle);
            GUILayout.Label($"Engine TWR: {LA.TwrEng:#0.00}m/s²", TwrLabelStyle);
            GUILayout.Label($"Altitude (AGL): {LA.AltTrue:#0.00}m");
            GUILayout.Label($"Vertical speed: {FlightGlobals.ActiveVessel.verticalSpeed:#0.00}m/s");
            GUILayout.Label($"Horizontal speed: {FlightGlobals.ActiveVessel.horizontalSrfSpeed:#0.00}m/s");
            if (!this.SlopeDetection)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Slope detection: ");
                GUILayout.Label("out of range", ColouredLabel(Color.red));
                GUILayout.EndHorizontal();
            }
            else if (!LA.groundCalc.terrain)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Slope detection: ");
                GUILayout.Label("not available", ColouredLabel(Color.red));
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.Label("Slope detection:");
                GUILayout.BeginHorizontal();
                TripleLabel();
                TripleLabel($"N {SlopeN:#0.0}°");
                TripleLabel();
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                TripleLabel($"W {SlopeW:#0.0}°");
                TripleLabel($"({LA.groundCalc.Radius:#0}m)");
                TripleLabel($"E {SlopeE:#0.0}°");
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                TripleLabel();
                TripleLabel($"S {SlopeS:#0.0}°");
                TripleLabel();
                GUILayout.EndHorizontal();
            }
        }
        #endregion
    }
}
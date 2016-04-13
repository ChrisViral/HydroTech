using HydroTech.Autopilots;
using HydroTech.Autopilots.Calculators;
using HydroTech.Managers;
using HydroTech.Storage;
using HydroTech.Utils;
using UnityEngine;

namespace HydroTech.Panels
{
    public class PanelLandingInfo : Panel
    {
        #region Static properties
        private static APLanding LA
        {
            get { return HydroFlightManager.Instance.LandingAutopilot; }
        }
        #endregion

        #region Properties
        protected bool slopeDetection;
        protected bool SlopeDetection
        {
            get
            {
                if (LA.SlopeDetection != this.slopeDetection)
                {
                    ResetHeight();
                    this.slopeDetection = LA.SlopeDetection;
                }
                return this.slopeDetection;
            }
        }

        protected float AltTrue
        {
            get { return LA.AltTrue; }
        }

        protected bool Terrain
        {
            get { return LA.Terrain; }
        }

        protected float DetectRadius
        {
            get { return LA.DetectRadius; }
        }

        protected float SlopeN
        {
            get { return LA.Slope(GroundContactCalculator.Direction.NORTH) * HTUtils.radToDeg; }
        }

        protected float SlopeS
        {
            get { return LA.Slope(GroundContactCalculator.Direction.SOUTH) * HTUtils.radToDeg; }
        }

        protected float SlopeW
        {
            get { return LA.Slope(GroundContactCalculator.Direction.WEST) * HTUtils.radToDeg; }
        }

        protected float SlopeE
        {
            get { return LA.Slope(GroundContactCalculator.Direction.EAST) * HTUtils.radToDeg; }
        }

        protected float VertSpeed
        {
            get { return LA.VertSpeed; }
        }

        protected float HorSpeed
        {
            get { return LA.HoriSpeed; }
        }

        protected string MainBodyName
        {
            get { return LA.MainBodyName; }
        }

        protected float GeeASL
        {
            get { return LA.GAsl; }
        }

        protected float TwrRCS
        {
            get { return LA.TwrRcs; }
        }

        protected float TwrEng
        {
            get { return LA.TwrEng; }
        }

        protected float TwrTotal
        {
            get { return this.TwrRCS + this.TwrEng; }
        }

        protected GUIStyle TwrLabelStyle
        {
            get
            {
                if (this.TwrTotal < this.GeeASL) { return GUIUtils.ColouredLabel(Color.red); }
                return this.TwrTotal < this.GeeASL * 1.5f ? GUIUtils.ColouredLabel(Color.yellow) : GUIUtils.Skin.label;
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
            this.fileName = new FileName("landinfo", "cfg", FileName.panelSaveFolder);
            this.id = GuidProvider.GetGuid<PanelLandingInfo>();
        }
        #endregion

        #region Methods
        protected void TripleLabel(string text = "")
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
            GUILayout.Label("Orbiting body: " + this.MainBodyName);
            GUILayout.Label(string.Format("Surface g: {0:#0.00}{1}", this.GeeASL, HTUtils.acceleration));
            GUILayout.Label(string.Format("RCS TWR: {0:#0.00}{1}", this.TwrRCS, HTUtils.acceleration), this.TwrLabelStyle);
            GUILayout.Label(string.Format("Engine TWR: {0:#0.00}{1}", this.TwrEng, HTUtils.acceleration), this.TwrLabelStyle);
            GUILayout.Label(string.Format("Altitude (AGL): {0:#0.00}{1}", this.AltTrue, HTUtils.length));
            GUILayout.Label(string.Format("Vertical speed: {0:#0.00}{1}", this.VertSpeed, HTUtils.speedSimple));
            GUILayout.Label(string.Format("Horizontal speed: {0:#0.00}{1}", this.HorSpeed, HTUtils.speedSimple));
            if (!this.SlopeDetection)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Slope detection: ");
                GUILayout.Label("out of range", GUIUtils.ColouredLabel(Color.red));
                GUILayout.EndHorizontal();
            }
            else if (!this.Terrain)
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
                TripleLabel(string.Format("N {0:#0.0}°", this.SlopeN));
                TripleLabel();
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                TripleLabel(string.Format("W {0:#0.0}°", this.SlopeW));
                TripleLabel(string.Format("({0:#0}{1})", this.DetectRadius, HTUtils.length));
                TripleLabel(string.Format("E {0:#0.0}°", this.SlopeE));
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                TripleLabel();
                TripleLabel(string.Format("S {0:#0.0}°", this.SlopeS));
                TripleLabel();
                GUILayout.EndHorizontal();
            }

            GUI.DragWindow();
        }
        #endregion
    }
}
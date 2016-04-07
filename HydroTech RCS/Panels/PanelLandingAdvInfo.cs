using HydroTech_FC;
using HydroTech_RCS.Autopilots;
using HydroTech_RCS.Autopilots.Calculators;
using HydroTech_RCS.Constants;
using UnityEngine;

namespace HydroTech_RCS.Panels
{
    public class PanelLandingAdvInfo : Panel
    {
        #region Static properties
        protected static APLanding LA
        {
            get { return APLanding.TheAutopilot; }
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
            get { return HMaths.RadToDeg(LA.Slope(GroundContactCalculator.Direction.NORTH)); }
        }

        protected float SlopeS
        {
            get { return HMaths.RadToDeg(LA.Slope(GroundContactCalculator.Direction.SOUTH)); }
        }

        protected float SlopeW
        {
            get { return HMaths.RadToDeg(LA.Slope(GroundContactCalculator.Direction.WEST)); }
        }

        protected float SlopeE
        {
            get { return HMaths.RadToDeg(LA.Slope(GroundContactCalculator.Direction.EAST)); }
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
                if (this.TwrTotal < this.GeeASL) { return LabelStyle(Color.red); }
                return this.TwrTotal < this.GeeASL * 1.5f ? LabelStyle(Color.yellow) : LabelStyle();
            }
        }

        protected override int PanelID
        {
            get { return CoreConsts.landingInfo; }
        }

        public override string PanelTitle
        {
            get { return PanelConsts.landingInfoTitle; }
        }
        #endregion

        #region Constructor
        public PanelLandingAdvInfo()
        {
            this.fileName = new FileName("landinfo", "cfg", HydroJebCore.panelSaveFolder);
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
            this.windowRect = PanelConsts.landingInfo;
        }

        protected override void WindowGUI(int windowId)
        {
            GUILayout.Label("Orbiting body: " + this.MainBodyName);
            GUILayout.Label(string.Format("Surface g: {0:#0.00}{1}", this.GeeASL, GeneralConsts.acceleration));
            GUILayout.Label(string.Format("RCS TWR: {0:#0.00}{1}", this.TwrRCS, GeneralConsts.acceleration), this.TwrLabelStyle);
            GUILayout.Label(string.Format("Engine TWR: {0:#0.00}{1}", this.TwrEng, GeneralConsts.acceleration), this.TwrLabelStyle);
            GUILayout.Label(string.Format("Altitude (AGL): {0:#0.00}{1}", this.AltTrue, GeneralConsts.length));
            GUILayout.Label(string.Format("Vertical speed: {0:#0.00}{1}", this.VertSpeed, GeneralConsts.speedSimple));
            GUILayout.Label(string.Format("Horizontal speed: {0:#0.00}{1}", this.HorSpeed, GeneralConsts.speedSimple));
            if (!this.SlopeDetection)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Slope detection: ");
                GUILayout.Label("out of range", LabelStyle(Color.red));
                GUILayout.EndHorizontal();
            }
            else if (!this.Terrain)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Slope detection: ");
                GUILayout.Label("not available", LabelStyle(Color.red));
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
                TripleLabel(string.Format("({0:#0}{1})", this.DetectRadius, GeneralConsts.length));
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
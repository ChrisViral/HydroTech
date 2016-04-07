using HydroTech_FC;
using HydroTech_RCS.Autopilots;
using HydroTech_RCS.Autopilots.Calculators;
using HydroTech_RCS.Constants;
using UnityEngine;

namespace HydroTech_RCS.Panels
{
    public class PanelLandingAdvInfo : Panel
    {
        protected bool slopeDetection;

        protected override int PanelID
        {
            get { return CoreConsts.landingInfo; }
        }

        public override string PanelTitle
        {
            get { return PanelConsts.landingInfoTitle; }
        }

        protected static APLanding La
        {
            get { return APLanding.TheAutopilot; }
        }

        protected float AltTrue
        {
            get { return La.AltTrue; }
        }

        protected bool Terrain
        {
            get { return La.Terrain; }
        }

        protected float DetectRadius
        {
            get { return La.DetectRadius; }
        }

        protected float SlopeN
        {
            get { return HMaths.RadToDeg(La.Slope(GroundContactCalculator.Direction.NORTH)); }
        }

        protected float SlopeS
        {
            get { return HMaths.RadToDeg(La.Slope(GroundContactCalculator.Direction.SOUTH)); }
        }

        protected float SlopeW
        {
            get { return HMaths.RadToDeg(La.Slope(GroundContactCalculator.Direction.WEST)); }
        }

        protected float SlopeE
        {
            get { return HMaths.RadToDeg(La.Slope(GroundContactCalculator.Direction.EAST)); }
        }

        protected float VertSpeed
        {
            get { return La.VertSpeed; }
        }

        protected float HoriSpeed
        {
            get { return La.HoriSpeed; }
        }

        protected string MainBodyName
        {
            get { return La.MainBodyName; }
        }

        protected float GAsl
        {
            get { return La.GAsl; }
        }

        protected float TwrRcs
        {
            get { return La.TwrRcs; }
        }

        protected float TwrEng
        {
            get { return La.TwrEng; }
        }

        protected float TwrTotal
        {
            get { return this.TwrRcs + this.TwrEng; }
        }

        protected bool SlopeDetection
        {
            get
            {
                if (La.SlopeDetection != this.slopeDetection)
                {
                    ResetHeight();
                    this.slopeDetection = La.SlopeDetection;
                }
                return this.slopeDetection;
            }
        }

        public PanelLandingAdvInfo()
        {
            this.fileName = new FileName("landinfo", "cfg", HydroJebCore.panelSaveFolder);
        }

        protected override void SetDefaultWindowRect()
        {
            this.windowRect = PanelConsts.landingInfo;
        }

        protected void TripleLabel(string text = "")
        {
            GUIStyle tripleLabel = new GUIStyle(GUI.skin.label);
            tripleLabel.fixedWidth = (this.windowRect.width / 3) - tripleLabel.margin.horizontal;
            GUILayout.Label(text, tripleLabel);
        }

        protected GUIStyle TwrLabelStyle()
        {
            if (this.TwrTotal < this.GAsl) { return LabelStyle(Color.red); }
            return this.TwrTotal < this.GAsl * 1.5F ? LabelStyle(Color.yellow) : LabelStyle();
        }

        protected override void WindowGUI(int windowId)
        {
            GUILayout.Label("Orbiting body: " + this.MainBodyName);
            GUILayout.Label("Surface g: " + this.GAsl.ToString("#0.00") + GeneralConsts.acceleration);
            GUILayout.Label("RCS Twr: " + this.TwrRcs.ToString("#0.00") + GeneralConsts.acceleration, TwrLabelStyle());
            GUILayout.Label("Engine Twr: " + this.TwrEng.ToString("#0.00") + GeneralConsts.acceleration, TwrLabelStyle());
            GUILayout.Label("Altitude (true): " + this.AltTrue.ToString("#0.0") + GeneralConsts.length);
            GUILayout.Label("Vertical speed: " + this.VertSpeed.ToString("#0.00") + GeneralConsts.speedSimple);
            GUILayout.Label("Horizontal speed: " + this.HoriSpeed.ToString("#0.00") + GeneralConsts.speedSimple);
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
                TripleLabel("N " + this.SlopeN.ToString("#0.0") + "°");
                TripleLabel();
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                TripleLabel("w " + this.SlopeW.ToString("#0.0") + "°");
                TripleLabel("(" + this.DetectRadius.ToString("#0") + GeneralConsts.length + ")");
                TripleLabel("E " + this.SlopeE.ToString("#0.0") + "°");
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                TripleLabel();
                TripleLabel("S " + this.SlopeS.ToString("#0.0") + "°");
                TripleLabel();
                GUILayout.EndHorizontal();
            }

            GUI.DragWindow();
        }
    }
}
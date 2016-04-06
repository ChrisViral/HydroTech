using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HydroTech_RCS.Panels
{
    using UnityEngine;
    using HydroTech_FC;
    using Constants.Core;
    using Constants.Panels;
    using Constants.Units;
    using Autopilots;
    using Autopilots.Modules;

    public class PanelLandingAdvInfo : Panel
    {
        public PanelLandingAdvInfo()
        {
            fileName = new FileName("landinfo", "cfg", HydroJebCore.PanelSaveFolder);
        }

        protected override int PanelID { get { return PanelIDs.LandingInfo; } }
        public override string PanelTitle { get { return PanelTitles.LandingInfo; } }

        protected override void SetDefaultWindowRect() { windowRect = WindowPositions.LandingInfo; }

        static protected APLanding LA { get { return APLanding.theAutopilot; } }
        protected float AltTrue { get { return LA.AltTrue; } }
        protected bool Terrain { get { return LA.Terrain; } }
        protected float DetectRadius { get { return LA.DetectRadius; } }
        protected float SlopeN { get { return HMaths.RadToDeg(LA.Slope(DetectorGroundContact.DIRECTION.NORTH)); } }
        protected float SlopeS { get { return HMaths.RadToDeg(LA.Slope(DetectorGroundContact.DIRECTION.SOUTH)); } }
        protected float SlopeW { get { return HMaths.RadToDeg(LA.Slope(DetectorGroundContact.DIRECTION.WEST)); } }
        protected float SlopeE { get { return HMaths.RadToDeg(LA.Slope(DetectorGroundContact.DIRECTION.EAST)); } }
        protected float VertSpeed { get { return LA.VertSpeed; } }
        protected float HoriSpeed { get { return LA.HoriSpeed; } }
        protected String MainBodyName { get { return LA.MainBodyName; } }
        protected float gASL { get { return LA.gASL; } }
        protected float TWR_RCS { get { return LA.TWR_RCS; } }
        protected float TWR_Eng { get { return LA.TWR_Eng; } }
        protected float TWR_Total { get { return TWR_RCS + TWR_Eng; } }

        protected bool _SlopeDetection = false;
        protected bool SlopeDetection
        {
            get
            {
                if (LA.SlopeDetection != _SlopeDetection)
                {
                    ResetHeight();
                    _SlopeDetection = LA.SlopeDetection;
                }
                return _SlopeDetection;
            }
        }

        protected void TripleLabel(String text = "")
        {
            GUIStyle tripleLabel = new GUIStyle(GUI.skin.label);
            tripleLabel.fixedWidth = windowRect.width / 3 - tripleLabel.margin.horizontal;
            GUILayout.Label(text, tripleLabel);
        }
        protected GUIStyle TWRLabelStyle()
        {
            if (TWR_Total < gASL)
                return LabelStyle(Color.red);
            else if (TWR_Total < gASL * 1.5F)
                return LabelStyle(Color.yellow);
            else
                return LabelStyle();
        }

        protected override void WindowGUI(int WindowID)
        {
            GUILayout.Label("Orbiting body: " + MainBodyName);
            GUILayout.Label("Surface g: " + gASL.ToString("#0.00") + UnitStrings.Acceleration);
            GUILayout.Label("RCS Twr: "+TWR_RCS.ToString("#0.00") + UnitStrings.Acceleration, TWRLabelStyle());
            GUILayout.Label("Engine Twr: " + TWR_Eng.ToString("#0.00") + UnitStrings.Acceleration, TWRLabelStyle());
            GUILayout.Label("Altitude (true): " + AltTrue.ToString("#0.0") + UnitStrings.Length);
            GUILayout.Label("Vertical speed: " + VertSpeed.ToString("#0.00") + UnitStrings.Speed_Simple);
            GUILayout.Label("Horizontal speed: " + HoriSpeed.ToString("#0.00") + UnitStrings.Speed_Simple);
            if (!SlopeDetection)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Slope detection: ");
                GUILayout.Label("out of range", LabelStyle(Color.red));
                GUILayout.EndHorizontal();
            }
            else if (!Terrain)
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
                TripleLabel("N " + SlopeN.ToString("#0.0") + "°");
                TripleLabel();
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                TripleLabel("w " + SlopeW.ToString("#0.0") + "°");
                TripleLabel("(" + DetectRadius.ToString("#0") + UnitStrings.Length + ")");
                TripleLabel("E " + SlopeE.ToString("#0.0") + "°");
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                TripleLabel();
                TripleLabel("S " + SlopeS.ToString("#0.0") + "°");
                TripleLabel();
                GUILayout.EndHorizontal();
            }

            GUI.DragWindow();
        }
    }
}
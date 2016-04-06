using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HydroTech_RCS.Panels
{
    using UnityEngine;
    using HydroTech_FC;
    using Autopilots;
    using Constants.Core;
    using Constants.Panels;
    using Constants.Units;

    public partial class PanelPreciseControl : PanelwAP
    {
        public PanelPreciseControl()
        {
            fileName = new FileName("precise", "cfg", HydroJebCore.PanelSaveFolder);
        }
        protected override int PanelID { get { return PanelIDs.PreciseControl; } }
        public override string PanelTitle { get { return PanelTitles.PreciseControl; } }

        protected override void SetDefaultWindowRect() { windowRect = WindowPositions.PreciseControl; }

        protected static APPreciseControl PC { get { return APPreciseControl.theAutopilot; } }

        protected override void MakeAPSave() { PC.MakeSaveAtNextUpdate(); }

        protected override bool Engaged
        {
            get { return PC.engaged; }
            set { PC.engaged = value; }
        }
        protected static bool byRate
        {
            get { return PC.byRate; }
            set { PC.byRate = value; }
        }
        protected static float RRate
        {
            get { return PC.RotationRate; }
            set { PC.RotationRate = value; }
        }
        protected static float TRate
        {
            get { return PC.TranslationRate; }
            set { PC.TranslationRate = value; }
        }
        protected static float AngA
        {
            get { return PC.AngularAcc; }
            set { PC.AngularAcc = value; }
        }
        protected static float Acc
        {
            get { return PC.Acc; }
            set { PC.Acc = value; }
        }

        protected override void WindowGUI(int WindowID)
        {
            if (Settings)
                DrawSettingsUI();
            else
            {
                if (byRate)
                {
                    GUILayout.Label("Rotation thrust rate: " + RRate.ToString("#0.000"));
                    GUILayout.Label("Translation thrust rate: " + TRate.ToString("#0.000"));
                }
                else
                {
                    GUILayout.Label("Angular Acc: " + AngA.ToString("#0.000") + UnitStrings.AngularAcc);
                    GUILayout.Label("Acceleration: " + Acc.ToString("#0.000") + UnitStrings.Acceleration);
                }
                if (GUILayout.Button("Change settings"))
                    Settings = true;
                if (LayoutEngageBtn(Engaged))
                    Engaged = !Engaged;
            }

            GUI.DragWindow();
        }
    }
}

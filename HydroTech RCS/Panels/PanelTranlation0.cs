﻿using System;
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

    public partial class PanelTranslation : PanelwAP
    {
        public PanelTranslation()
        {
            fileName = new FileName("translation", "cfg", HydroJebCore.PanelSaveFolder);
        }

        protected override int PanelID { get { return PanelIDs.Translation; } }
        public override string PanelTitle { get { return PanelTitles.Translation; } }

        protected override void SetDefaultWindowRect() { windowRect = WindowPositions.Translation; }

        protected static APTranslation TA { get { return APTranslation.theAutopilot; } }

        protected override void MakeAPSave() { TA.MakeSaveAtNextUpdate(); }

        protected override bool Engaged
        {
            get { return TA.engaged; }
            set { TA.engaged = value; }
        }
        protected static APTranslation.TransDir Trans_Mode
        {
            get { return TA.Trans_Mode; }
            set { TA.Trans_Mode = value; }
        }
        protected static Vector3 thrustVector
        {
            get { return TA.thrustVector; }
            set { TA.thrustVector = value; }
        }
        protected static float thrustRate
        {
            get { return TA.thrustRate; }
            set { TA.thrustRate = value; }
        }
        protected static bool respond
        {
            get { return TA.mainThrottleRespond; }
            set { TA.mainThrottleRespond = value; }
        }
        protected static bool HoldOrient
        {
            get { return TA.HoldOrient; }
            set { TA.HoldOrient = value; }
        }

        protected override void WindowGUI(int WindowID)
        {
            if (Settings)
                DrawSettingsUI();
            else
            {
                GUILayout.Label("Translation direction");
                GUILayout.TextArea(
                    Trans_Mode == APTranslation.TransDir.ADVANCED ?
                    thrustVector.ToString("#0.00") :
                    Trans_Mode.ToString()
                    );
                GUILayout.Label("Thrust rate: " + thrustRate.ToString("#0.00"));
                GUILayout.Label("Respond to main throttle: " + respond);
                GUILayout.Label("Hold current orientation: " + HoldOrient);

                if (GUILayout.Button("Change settings"))
                    Settings = true;

                if (LayoutEngageBtn(Engaged))
                    Engaged = !Engaged;
            }

            GUI.DragWindow();
        }
    }
}

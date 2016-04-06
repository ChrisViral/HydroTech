using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HydroTech_RCS.Panels
{
    using UnityEngine;

    public partial class PanelDockAssist // Defines the WindowGUI
    {
        protected override void WindowGUI(int WindowID)
        {
            GUILayout.BeginVertical();
            if (ChoosingCamera)
                DrawChoosingCameraUI();
            else if (ChoosingVessel)
                DrawChoosingVesselUI();
            else if (ChoosingTarget)
                DrawChoosingTargetUI();
            else if (Settings)
                DrawSettingsUI();
            else
            {
                GUILayout.Label("Camera:");
                if (Cam == null ?
                    GUILayout.Button("Choose camera") :
                    GUILayout.Button(
                        Cam.ToString(),
                        Cam.IsOnActiveVessel() ? BtnStyle_Wrap(Color.green) : BtnStyle_Wrap(Color.red)
                        )
                    )
                    ChoosingCamera = true;
                GUILayout.Label("Target:");
                if (Target == null ?
                    GUILayout.Button("Choose target") :
                    GUILayout.Button(
                        Target.vessel.vesselName + "\n" + Target.ToString(),
                        Target.IsNear() ? BtnStyle_Wrap(Color.green) : BtnStyle_Wrap(Color.red)
                        )
                    )
                    ChoosingTarget = true;
                GUILayout.Label("Settings:");
                if (GUILayout.Button(Manual ? "Manual docking" : "Automated docking"))
                    Settings = true;
                if (LayoutEngageBtn(Engaged))
                    Engaged = !Engaged;
            }
            GUILayout.EndVertical();

            GUI.DragWindow();
        }
    }
}
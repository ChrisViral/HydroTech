using UnityEngine;

namespace HydroTech_RCS.Panels
{
    public partial class PanelDockAssist // Defines the WindowGUI
    {
        protected override void WindowGUI(int windowId)
        {
            GUILayout.BeginVertical();
            if (this.ChoosingCamera) { DrawChoosingCameraUi(); }
            else if (this.ChoosingVessel) { DrawChoosingVesselUi(); }
            else if (this.ChoosingTarget) { DrawChoosingTargetUi(); }
            else if (this.Settings) { DrawSettingsUI(); }
            else
            {
                GUILayout.Label("Camera:");
                if (Cam == null ? GUILayout.Button("Choose camera") : GUILayout.Button(Cam.ToString(), Cam.IsOnActiveVessel() ? BtnStyle_Wrap(Color.green) : BtnStyle_Wrap(Color.red))) { this.ChoosingCamera = true; }
                GUILayout.Label("Target:");
                if (Target == null ? GUILayout.Button("Choose target") : GUILayout.Button(Target.vessel.vesselName + "\n" + Target, Target.IsNear() ? BtnStyle_Wrap(Color.green) : BtnStyle_Wrap(Color.red))) { this.ChoosingTarget = true; }
                GUILayout.Label("Settings:");
                if (GUILayout.Button(Manual ? "Manual docking" : "Automated docking")) { this.Settings = true; }
                if (LayoutEngageBtn(this.Engaged)) { this.Engaged = !this.Engaged; }
            }
            GUILayout.EndVertical();

            GUI.DragWindow();
        }
    }
}
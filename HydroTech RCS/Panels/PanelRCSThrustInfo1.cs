using HydroTech_RCS.Constants.Units;
using UnityEngine;

namespace HydroTech_RCS.Panels
{
    public partial class PanelRcsThrustInfo
    {
        public override void drawGUI()
        {
            if (this.editor)
            {
                GUI.skin = HighLogic.Skin;
                Rect newWindowRect = GUILayout.Window(this.QueueSpot, this.windowRectEditor, WindowGUI, this.PanelTitle);
                if ((newWindowRect.yMin != this.windowRectEditor.yMin) || (newWindowRect.yMin != this.windowRectEditor.yMin)) { this.needSave = true; }
                this.windowRectEditor = newWindowRect;
            }
            else
            { DrawGui(); }
        }

        protected override void WindowGUI(int windowId)
        {
            GUILayout.BeginHorizontal();
            if (this.editor)
            {
                if (GUILayout.Button(this.editorHide ? "Maximize" : "Minimize"))
                {
                    this.editorHide = !this.editorHide;
                    this.needSave = true;
                    this.windowRectEditor.width = this.editorHide ? 100 : 250;
                    this.windowRectEditor.height = 0;
                }
                if (this.editorHide)
                {
                    GUILayout.EndHorizontal();
                    GUI.DragWindow();
                    return;
                }
            }
            if (GUILayout.Button("Rotation", this.showRotation ? BtnStyle(Color.green) : BtnStyle()))
            {
                this.showRotation = true;
                this.needSave = true;
            }
            if (GUILayout.Button("Translation", this.showRotation ? BtnStyle() : BtnStyle(Color.green)))
            {
                this.showRotation = false;
                this.needSave = true;
            }
            GUILayout.EndHorizontal();
            if (this.showRotation)
            {
                GUILayout.Label("Max torque (" + UnitStrings.torque + ") and" + "\nangular acceleration (" + UnitStrings.angularAcc + ")");
                GUILayout.Label("Pitch down : " + TheCalculator.maxTorque.xp.ToString("#0.00") + " , " + TheCalculator.maxAngularAcc.xp.ToString("#0.00"));
                GUILayout.Label("Pitch up : " + TheCalculator.maxTorque.xn.ToString("#0.00") + " , " + TheCalculator.maxAngularAcc.xn.ToString("#0.00"));
                GUILayout.Label("yaw left : " + TheCalculator.maxTorque.yp.ToString("#0.00") + " , " + TheCalculator.maxAngularAcc.yp.ToString("#0.00"));
                GUILayout.Label("yaw right : " + TheCalculator.maxTorque.yn.ToString("#0.00") + " , " + TheCalculator.maxAngularAcc.yn.ToString("#0.00"));
                GUILayout.Label("Roll left : " + TheCalculator.maxTorque.zp.ToString("#0.00") + " , " + TheCalculator.maxAngularAcc.zp.ToString("#0.00"));
                GUILayout.Label("Roll right : " + TheCalculator.maxTorque.zn.ToString("#0.00") + " , " + TheCalculator.maxAngularAcc.zn.ToString("#0.00"));
            }
            else
            {
                GUILayout.Label("Max thrust (" + UnitStrings.force + ") and" + "\nacceleration (" + UnitStrings.acceleration + ")");
                GUILayout.Label("Translate left : " + TheCalculator.maxForce.xp.ToString("#0.00") + " , " + TheCalculator.maxAcc.xp.ToString("#0.00"));
                GUILayout.Label("Translate right : " + TheCalculator.maxForce.xn.ToString("#0.00") + " , " + TheCalculator.maxAcc.xn.ToString("#0.00"));
                GUILayout.Label("Translate up : " + TheCalculator.maxForce.yp.ToString("#0.00") + " , " + TheCalculator.maxAcc.yp.ToString("#0.00"));
                GUILayout.Label("Translate down : " + TheCalculator.maxForce.yn.ToString("#0.00") + " , " + TheCalculator.maxAcc.yn.ToString("#0.00"));
                GUILayout.Label("Translate backward : " + TheCalculator.maxForce.zp.ToString("#0.00") + " , " + TheCalculator.maxAcc.zp.ToString("#0.00"));
                GUILayout.Label("Translate forward : " + TheCalculator.maxForce.zn.ToString("#0.00") + " , " + TheCalculator.maxAcc.zn.ToString("#0.00"));
            }
            if (!this.editor && !TheCalculator.AllRcsEnabled)
            {
                GUILayout.Label("Some RCS thrusters are not enabled.", LabelStyle(Color.red));
                if (GUILayout.Button("Enable all")) { TheCalculator.EnableAllRcs(); }
            }
            GUI.DragWindow();
        }
    }
}
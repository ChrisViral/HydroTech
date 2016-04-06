using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HydroTech_RCS.Panels
{
    using UnityEngine;
    using Constants.Units;

    public partial class PanelRCSThrustInfo
    {
        public override void drawGUI()
        {
            if (editor)
            {
                GUI.skin = HighLogic.Skin;
                Rect newWindowRect = GUILayout.Window(
                    QueueSpot,
                    windowRectEditor, WindowGUI, PanelTitle);
                if (newWindowRect.yMin != windowRectEditor.yMin || newWindowRect.yMin != windowRectEditor.yMin)
                    needSave = true;
                windowRectEditor = newWindowRect;
            }
            else
                base.drawGUI();
        }

        protected override void WindowGUI(int WindowID)
        {
            GUILayout.BeginHorizontal();
            if (editor)
            {
                if (GUILayout.Button(editorHide ? "Maximize" : "Minimize"))
                {
                    editorHide = !editorHide;
                    needSave = true;
                    windowRectEditor.width = editorHide ? 100 : 250;
                    windowRectEditor.height = 0;
                }
                if (editorHide)
                {
                    GUILayout.EndHorizontal();
                    GUI.DragWindow();
                    return;
                }
            }
            if (GUILayout.Button("Rotation", showRotation ? BtnStyle(Color.green) : BtnStyle()))
            {
                showRotation = true;
                needSave = true;
            }
            if (GUILayout.Button("Translation", showRotation ? BtnStyle() : BtnStyle(Color.green)))
            {
                showRotation = false;
                needSave = true;
            }
            GUILayout.EndHorizontal();
            if (showRotation)
            {
                GUILayout.Label(
                    "Max torque (" + UnitStrings.Torque + ") and"
                    + "\nangular acceleration (" + UnitStrings.AngularAcc + ")"
                    );
                GUILayout.Label(
                    "Pitch down : " + theCalculator.MaxTorque.xp.ToString("#0.00")
                    + " , " + theCalculator.MaxAngularAcc.xp.ToString("#0.00")
                    );
                GUILayout.Label(
                    "Pitch up : " + theCalculator.MaxTorque.xn.ToString("#0.00")
                    + " , " + theCalculator.MaxAngularAcc.xn.ToString("#0.00")
                    );
                GUILayout.Label(
                    "yaw left : " + theCalculator.MaxTorque.yp.ToString("#0.00")
                    + " , " + theCalculator.MaxAngularAcc.yp.ToString("#0.00")
                    );
                GUILayout.Label(
                    "yaw right : " + theCalculator.MaxTorque.yn.ToString("#0.00")
                    + " , " + theCalculator.MaxAngularAcc.yn.ToString("#0.00")
                    );
                GUILayout.Label(
                    "Roll left : " + theCalculator.MaxTorque.zp.ToString("#0.00")
                    + " , " + theCalculator.MaxAngularAcc.zp.ToString("#0.00")
                    );
                GUILayout.Label(
                    "Roll right : " + theCalculator.MaxTorque.zn.ToString("#0.00")
                    + " , " + theCalculator.MaxAngularAcc.zn.ToString("#0.00")
                    );
            }
            else
            {
                GUILayout.Label(
                    "Max thrust (" + UnitStrings.Force + ") and"
                    + "\nacceleration (" + UnitStrings.Acceleration + ")"
                    );
                GUILayout.Label(
                    "Translate left : " + theCalculator.MaxForce.xp.ToString("#0.00")
                    + " , " + theCalculator.MaxAcc.xp.ToString("#0.00")
                    );
                GUILayout.Label(
                    "Translate right : " + theCalculator.MaxForce.xn.ToString("#0.00")
                    + " , " + theCalculator.MaxAcc.xn.ToString("#0.00")
                    );
                GUILayout.Label(
                    "Translate up : " + theCalculator.MaxForce.yp.ToString("#0.00")
                    + " , " + theCalculator.MaxAcc.yp.ToString("#0.00")
                    );
                GUILayout.Label(
                    "Translate down : " + theCalculator.MaxForce.yn.ToString("#0.00")
                    + " , " + theCalculator.MaxAcc.yn.ToString("#0.00")
                    );
                GUILayout.Label(
                    "Translate backward : " + theCalculator.MaxForce.zp.ToString("#0.00")
                    + " , " + theCalculator.MaxAcc.zp.ToString("#0.00")
                    );
                GUILayout.Label(
                    "Translate forward : " + theCalculator.MaxForce.zn.ToString("#0.00")
                    + " , " + theCalculator.MaxAcc.zn.ToString("#0.00")
                    );
            }
            if (!editor && !theCalculator.AllRCSEnabled)
            {
                GUILayout.Label("Some RCS thrusters are not enabled.", LabelStyle(Color.red));
                if (GUILayout.Button("Enable all"))
                    theCalculator.EnableAllRCS();
            }
            GUI.DragWindow();
        }
    }
}
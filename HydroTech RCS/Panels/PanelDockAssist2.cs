using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HydroTech_RCS.Panels
{
    using UnityEngine;
    using Constants.Units;

    public partial class PanelDockAssist // User settings
    {
        protected bool _tempManual;
        protected bool tempManual
        {
            get { return _tempManual; }
            set
            {
                if (value != _tempManual)
                    ResetHeight();
                _tempManual = value;
            }
        }
        protected bool tempShowLine;
        protected bool tempAutoOrient;
        protected bool tempKillRelV;
        protected bool _tempCamView;
        protected bool tempCamView
        {
            get { return _tempCamView; }
            set
            {
                if (!value && _tempCamView)
                    ResetHeight();
                _tempCamView = value;
            }
        }
        protected int tempCamMag;
        protected String AngularAcc_Text;
        protected String Acc_Text;
        protected String FSS_Text;
        protected bool tempDriveTarget;

        protected override bool Settings
        {
            set
            {
                if (value != _Settings)
                {
                    if (value) // Starting settings
                    {
                        tempAutoOrient = AutoOrient;
                        if (Cam != null)
                            tempCamMag = CamMag;
                        tempCamView = CamView;
                        tempDriveTarget = DriveTarget;
                        tempKillRelV = KillRelV;
                        tempManual = Manual;
                        tempShowLine = ShowLine;
                        AngularAcc_Text = AngularAcc.ToString("#0.000");
                        Acc_Text = Acc.ToString("#0.000");
                        FSS_Text = FSSpeed.ToString("#0.000");
                    }
                    else // Applying settings
                    {
                        AutoOrient = tempAutoOrient;
                        if (Cam != null)
                            CamMag = tempCamMag;
                        CamView = tempCamView;
                        DriveTarget = tempDriveTarget;
                        KillRelV = tempKillRelV;
                        Manual = tempManual;
                        ShowLine = tempShowLine;
                        float tryParse;
                        if (float.TryParse(AngularAcc_Text, out tryParse) && tryParse >= 0.0F)
                            AngularAcc = tryParse;
                        if (float.TryParse(Acc_Text, out tryParse) && tryParse >= 0.0F)
                            Acc = tryParse;
                        if (float.TryParse(FSS_Text, out tryParse) && tryParse > 0)
                            FSSpeed = tryParse;
                    }
                }
                base.Settings = value;
            }
        }

        protected override void DrawSettingsUI()
        {
            GUILayout.Label("Angular acceleration (" + UnitStrings.AngularAcc + ")");
            AngularAcc_Text = GUILayout.TextField(AngularAcc_Text);
            GUILayout.Label("Acceleration (" + UnitStrings.Acceleration + ")");
            Acc_Text = GUILayout.TextField(Acc_Text);

            if (!NullCamera() && !NullTarget())
            {
                tempShowLine = GUILayout.Toggle(tempShowLine, "Guidance line");
                tempManual = !GUILayout.Toggle(!tempManual, "Automated docking");
                if (tempManual)
                {
                    tempAutoOrient = GUILayout.Toggle(tempAutoOrient, "Automatic orientation");
                    tempKillRelV = GUILayout.Toggle(tempKillRelV, "Kill relative v");
                }
                else
                {
                    GUILayout.Label("Final approach speed");
                    GUILayout.BeginHorizontal();
                    FSS_Text = GUILayout.TextField(FSS_Text);
                    GUILayout.Label(UnitStrings.Speed_Simple);
                    GUILayout.EndHorizontal();
                    if (TargetHasJeb())
                        tempDriveTarget = GUILayout.Toggle(tempDriveTarget, "Rotate target");
                }
            }
            if (!NullCamera())
            {
                tempCamView = GUILayout.Toggle(tempCamView, "Camera view");
                if (tempCamView)
                {
                    GUILayout.BeginHorizontal();
                    GUIStyle LblStyle = new GUIStyle(GUI.skin.label);
                    LblStyle.fixedWidth = windowRect.width / 2;
                    GUILayout.Label("Mag: ×" + tempCamMag, LblStyle);
                    if (GUILayout.Button("-"))
                    {
                        if (tempCamMag > 1)
                            tempCamMag /= 2;
                    }
                    if (GUILayout.Button("+"))
                    {
                        if (tempCamMag < 32)
                            tempCamMag *= 2;
                    }
                    GUILayout.EndHorizontal();
                }
            }

            base.DrawSettingsUI();
        }
    }
}
using HydroTech_RCS.Constants.Units;
using UnityEngine;

namespace HydroTech_RCS.Panels
{
    public partial class PanelDockAssist // User settings
    {
        protected string accText;
        protected string angularAccText;
        protected string fssText;
        protected bool tempAutoOrient;
        protected int tempCamMag;
        protected bool tempCamView;
        protected bool tempDriveTarget;
        protected bool tempKillRelV;
        protected bool tempManual;
        protected bool tempShowLine;

        protected bool TempManual
        {
            get { return this.tempManual; }
            set
            {
                if (value != this.tempManual) { ResetHeight(); }
                this.tempManual = value;
            }
        }

        protected bool TempCamView
        {
            get { return this.tempCamView; }
            set
            {
                if (!value && this.tempCamView) { ResetHeight(); }
                this.tempCamView = value;
            }
        }

        protected override bool Settings
        {
            set
            {
                if (value != this.settings)
                {
                    if (value) // Starting settings
                    {
                        this.tempAutoOrient = AutoOrient;
                        if (Cam != null) { this.tempCamMag = CamMag; }
                        this.TempCamView = CamView;
                        this.tempDriveTarget = DriveTarget;
                        this.tempKillRelV = KillRelV;
                        this.TempManual = Manual;
                        this.tempShowLine = ShowLine;
                        this.angularAccText = AngularAcc.ToString("#0.000");
                        this.accText = Acc.ToString("#0.000");
                        this.fssText = FsSpeed.ToString("#0.000");
                    }
                    else // Applying settings
                    {
                        AutoOrient = this.tempAutoOrient;
                        if (Cam != null) { CamMag = this.tempCamMag; }
                        CamView = this.TempCamView;
                        DriveTarget = this.tempDriveTarget;
                        KillRelV = this.tempKillRelV;
                        Manual = this.TempManual;
                        ShowLine = this.tempShowLine;
                        float tryParse;
                        if (float.TryParse(this.angularAccText, out tryParse) && tryParse >= 0.0F) { AngularAcc = tryParse; }
                        if (float.TryParse(this.accText, out tryParse) && tryParse >= 0.0F) { Acc = tryParse; }
                        if (float.TryParse(this.fssText, out tryParse) && tryParse > 0) { FsSpeed = tryParse; }
                    }
                }
                base.Settings = value;
            }
        }

        protected override void DrawSettingsUI()
        {
            GUILayout.Label("Angular acceleration (" + UnitStrings.angularAcc + ")");
            this.angularAccText = GUILayout.TextField(this.angularAccText);
            GUILayout.Label("Acceleration (" + UnitStrings.acceleration + ")");
            this.accText = GUILayout.TextField(this.accText);

            if (!NullCamera() && !NullTarget())
            {
                this.tempShowLine = GUILayout.Toggle(this.tempShowLine, "Guidance line");
                this.TempManual = !GUILayout.Toggle(!this.TempManual, "Automated docking");
                if (this.TempManual)
                {
                    this.tempAutoOrient = GUILayout.Toggle(this.tempAutoOrient, "Automatic orientation");
                    this.tempKillRelV = GUILayout.Toggle(this.tempKillRelV, "Kill relative v");
                }
                else
                {
                    GUILayout.Label("Final approach speed");
                    GUILayout.BeginHorizontal();
                    this.fssText = GUILayout.TextField(this.fssText);
                    GUILayout.Label(UnitStrings.speedSimple);
                    GUILayout.EndHorizontal();
                    if (TargetHasJeb()) { this.tempDriveTarget = GUILayout.Toggle(this.tempDriveTarget, "Rotate target"); }
                }
            }
            if (!NullCamera())
            {
                this.TempCamView = GUILayout.Toggle(this.TempCamView, "Camera view");
                if (this.TempCamView)
                {
                    GUILayout.BeginHorizontal();
                    GUIStyle lblStyle = new GUIStyle(GUI.skin.label);
                    lblStyle.fixedWidth = this.windowRect.width / 2;
                    GUILayout.Label("Mag: ×" + this.tempCamMag, lblStyle);
                    if (GUILayout.Button("-")) { if (this.tempCamMag > 1) { this.tempCamMag /= 2; } }
                    if (GUILayout.Button("+")) { if (this.tempCamMag < 32) { this.tempCamMag *= 2; } }
                    GUILayout.EndHorizontal();
                }
            }

            base.DrawSettingsUI();
        }
    }
}
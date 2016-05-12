using System;
using HydroTech.Autopilots;
using HydroTech.Managers;
using HydroTech.UI;
using UnityEngine;
using static HydroTech.Utils.GUIUtils;

namespace HydroTech.Panels
{
    public class PanelDockAssist : PanelAP
    {
        #region Static properties
        private static APDockAssist DA => HydroFlightManager.Instance.DockingAutopilot;

        private static int CamMag
        {
            get
            {
                if (DA.Cam == null) { throw new NullReferenceException("PanelDockAssist.CamMag<get> before a camera is selected"); }
                return DA.Cam.Mag;
            }
            set
            {
                if (DA.Cam == null) { throw new NullReferenceException("PanelDockAssist.CamMag<set> before a camera is selected"); }
                DA.Cam.Mag = value;
            }
        }
        #endregion

        #region Fields
        protected Vessel targetVessel;
        protected string accText;
        protected string angularAccText;
        protected string fssText;
        protected int tempCamMag;
        protected bool tempAutoOrient;
        protected bool tempCamView;
        protected bool tempDriveTarget;
        protected bool tempKillRelV;
        protected bool tempManual;
        protected bool tempShowLine;
        #endregion

        #region Properties
        public UILinkedToggles<Vessel> TargetVessels { get; }

        public UILinkedToggles<ModuleDockAssistCam> Cameras { get; }

        public UILinkedToggles<ModuleDockAssistTarget> Targets { get; }

        private bool choosingCamera;
        public bool ChoosingCamera
        {
            get { return this.choosingCamera; }
            private set
            {
                if (value != this.choosingCamera)
                {
                    ResetHeight();
                    DA.CameraPaused = value;
                    this.choosingCamera = value;
                }
            }
        }

        private bool choosingVessel;
        public bool ChoosingVessel
        {
            get { return this.choosingVessel; }
            private set
            {
                if (value != this.choosingVessel)
                {
                    ResetHeight();
                    this.choosingVessel = value;
                }
            }
        }

        private bool choosingTarget;
        public bool ChoosingTarget
        {
            get { return this.choosingTarget; }
            private set
            {
                if (value != this.choosingTarget)
                {
                    ResetHeight();
                    if (value)
                    {
                        DA.CameraPaused = true;
                        if (this.targetVessel == null) { this.ChoosingVessel = true; }
                        else { this.PreviewVessel = this.targetVessel; }
                    }
                    else
                    {
                        this.PreviewAssist = null;
                        DA.CameraPaused = false;
                    }
                    this.choosingTarget = value;
                }
            }
        }

        private Vessel previewVessel;
        public Vessel PreviewVessel
        {
            get { return this.previewVessel; }
            private set
            {
                if (value != this.previewVessel)
                {
                    if (value == null) { HydroFlightManager.Instance.CameraManager.RetrieveLast(); }
                    else
                    {
                        if (this.previewVessel == null && this.PreviewAssist == null) { HydroFlightManager.Instance.CameraManager.SaveCurrent(); }
                        HydroFlightManager.Instance.CameraManager.CamCallback = ShowPreview;
                    }
                    this.previewVessel = value;
                }
            }
        }

        private ModuleDockAssist previewAssist;
        public ModuleDockAssist PreviewAssist
        {
            get { return this.previewAssist; }
            private set
            {
                if (value != this.previewAssist)
                {
                    if (value == null) { HydroFlightManager.Instance.CameraManager.RetrieveLast(); }
                    else
                    {
                        if (this.previewAssist == null && this.PreviewVessel == null) { HydroFlightManager.Instance.CameraManager.SaveCurrent(); }
                        HydroFlightManager.Instance.CameraManager.CamCallback = value.ShowPreview;
                    }
                    this.previewAssist = value;
                }
            }
        }

        private bool TempManual
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

        protected override bool Engaged
        {
            get { return DA.Engaged; }
            set { DA.Engaged = value; }
        }

        protected override bool Settings
        {
            set
            {
                if (value != this.settings)
                {
                    if (value) //Starting settings
                    {
                        this.tempAutoOrient = DA.AutoOrient;
                        if (DA.Cam != null) { this.tempCamMag = CamMag; }
                        this.TempCamView = DA.CamView;
                        this.tempDriveTarget = DA.DriveTarget;
                        this.tempKillRelV = DA.KillRelV;
                        this.TempManual = DA.Manual;
                        this.tempShowLine = DA.ShowLine;
                        this.angularAccText = DA.angularAcc.ToString("#0.000");
                        this.accText = DA.acc.ToString("#0.000");
                        this.fssText = DA.finalStageSpeed.ToString("#0.000");
                    }
                    else //Applying settings
                    {
                        DA.AutoOrient = this.tempAutoOrient;
                        if (DA.Cam != null) { CamMag = this.tempCamMag; }
                        DA.CamView = this.TempCamView;
                        DA.DriveTarget = this.tempDriveTarget;
                        DA.KillRelV = this.tempKillRelV;
                        DA.Manual = this.TempManual;
                        DA.ShowLine = this.tempShowLine;
                        float tryParse;
                        if (float.TryParse(this.angularAccText, out tryParse) && tryParse >= 0) { DA.angularAcc = tryParse; }
                        if (float.TryParse(this.accText, out tryParse) && tryParse >= 0) { DA.acc = tryParse; }
                        if (float.TryParse(this.fssText, out tryParse) && tryParse > 0) { DA.finalStageSpeed = tryParse; }
                    }
                }
                base.Settings = value;
            }
        }

        public override string Title => "Docking Assistant";
        #endregion

        #region Constructor
        public PanelDockAssist() : base(new Rect(349, 215, 200, 252), GetID<PanelDockAssist>())
        {
            this.Cameras = new UILinkedToggles<ModuleDockAssistCam>(c => c.assistName, Skin.button);
            this.TargetVessels = new UILinkedToggles<Vessel>(v => v.vesselName, Skin.button);
            this.Targets = new UILinkedToggles<ModuleDockAssistTarget>(t => t.assistName, Skin.button);
        }
        #endregion

        #region Methods
        private void DrawChooseCamera()
        {
            
        }

        private void DrawChooseVessel()
        {
            
        }

        private void DrawChooseTarget()
        {
            
        }

        public void ShowPreview()
        {
            HydroFlightManager.Instance.CameraManager.FoV = 60;
            HydroFlightManager.Instance.CameraManager.Target = this.targetVessel.transform;
        }
        #endregion

        #region Overrides
        protected override void Window(int id)
        {
            GUI.DragWindow(this.drag);

            GUILayout.BeginVertical();
            if (this.ChoosingCamera) { DrawChooseCamera(); }
            else if (this.ChoosingVessel) { DrawChooseVessel(); }
            else if (this.ChoosingTarget) { DrawChooseTarget(); }
            else if (this.Settings) { DrawSettings(); }
            else
            {
                GUILayout.Label("Camera:");
                ModuleDockAssistCam cam = DA.Cam;
                if (cam == null ? GUILayout.Button("Choose camera") : GUILayout.Button(cam.ToString(), ButtonStyle(cam.IsOnActiveVessel ? XKCDColors.Green : XKCDColors.Red, true)))
                {
                    this.ChoosingCamera = true;
                }
                GUILayout.Label("Target:");
                ModuleDockAssistTarget tgt = DA.Target;
                if (tgt == null ? GUILayout.Button("Choose target") : GUILayout.Button(tgt.vessel.vesselName + "\n" + tgt, ButtonStyle(tgt.IsNear ? XKCDColors.Green : XKCDColors.Red, true)))
                {
                    this.ChoosingTarget = true;
                }
                GUILayout.Label("Settings:");
                if (GUILayout.Button(DA.Manual ? "Manual docking" : "Automated docking"))
                {
                    this.Settings = true;
                }
                if (EngageButton(this.Engaged)) { this.Engaged = !this.Engaged; }
            }
            GUILayout.EndVertical();
        }

        protected override void DrawSettings()
        {
            GUILayout.Label("Angular acceleration (rad/s²):");
            this.angularAccText = GUILayout.TextField(this.angularAccText);
            GUILayout.Label("Acceleration (m/s²):");
            this.accText = GUILayout.TextField(this.accText);

            if (!DA.NullCamera)
            {
                if (!DA.NullTarget)
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
                        GUILayout.Label("m/s");
                        GUILayout.EndHorizontal();
                        if (DA.TargetHasJeb) { this.tempDriveTarget = GUILayout.Toggle(this.tempDriveTarget, "Rotate target"); }
                    }
                }

                this.TempCamView = GUILayout.Toggle(this.TempCamView, "Camera view");
                if (this.TempCamView)
                {
                    GUILayout.BeginHorizontal();
                    GUIStyle lblStyle = new GUIStyle(GUI.skin.label) { fixedWidth = this.window.width / 2 };
                    GUILayout.Label("Mag: ×" + this.tempCamMag, lblStyle);
                    if (GUILayout.Button("-")) { if (this.tempCamMag > 1) { this.tempCamMag /= 2; } }
                    if (GUILayout.Button("+")) { if (this.tempCamMag < 32) { this.tempCamMag *= 2; } }
                    GUILayout.EndHorizontal();
                }
            }

            base.DrawSettings();
        }
        #endregion
    }
}
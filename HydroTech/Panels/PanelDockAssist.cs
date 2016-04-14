using System;
using HydroTech.Autopilots;
using HydroTech.Data;
using HydroTech.Managers;
using HydroTech.Panels.UI;
using HydroTech.Utils;
using UnityEngine;

namespace HydroTech.Panels
{
    public class PanelDockAssist : PanelAP
    {
        #region Static properties
        private static APDockAssist DA
        {
            get { return HydroFlightManager.Instance.DockingAutopilot; }
        }

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
        protected AffiliationList<ModuleDockAssistTarget, Vessel> targetVesselList;
        protected SubList<ModuleDockAssistTarget> targetList;
        protected UISingleSelectionList<ModuleDockAssistCam> camListUI;
        protected UISingleSelectionList<ModuleDockAssistTarget> targetListUI;
        protected UISingleSelectionList<Vessel> targetVesselListUI;
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
        private ModuleDockAssist previewPart;
        private ModuleDockAssist PreviewPart
        {
            get { return this.previewPart; }
            set
            {
                if (value != this.previewPart)
                {
                    if (value == null) { HydroFlightManager.Instance.CameraManager.RetrieveLast(); }
                    else
                    {
                        if (this.previewPart == null && this.PreviewVessel == null) { HydroFlightManager.Instance.CameraManager.SaveCurrent(); }
                        HydroFlightManager.Instance.CameraManager.CamCallback = value.ShowPreview;
                    }
                    this.previewPart = value;
                }
            }
        }

        private bool choosingCamera;
        private bool ChoosingCamera
        {
            get { return this.choosingCamera; }
            set
            {
                if (value != this.choosingCamera)
                {
                    ResetHeight();
                    if (value)
                    {
                        DA.CameraPaused = true;
                        this.camListUI.SetSelectionToItem(DA.Cam);
                        this.camListUI.SetToCurSelPage();
                        this.camListUI.SetSelectionToItem(null);
                    }
                    else
                    {
                        this.PreviewPart = null;
                        this.camListUI.SetSelectionToItem(null);
                        DA.CameraPaused = false;
                    }
                }
                this.choosingCamera = value;
            }
        }

        private Vessel previewVessel;
        private Vessel PreviewVessel
        {
            get { return this.previewVessel; }
            set
            {
                if (value != this.previewVessel)
                {
                    if (value == null) { HydroFlightManager.Instance.CameraManager.RetrieveLast(); }
                    else
                    {
                        if (this.previewVessel == null && this.PreviewPart == null) { HydroFlightManager.Instance.CameraManager.SaveCurrent(); }
                        HydroFlightManager.Instance.CameraManager.CamCallback = DoPreviewVessel;
                    }
                    this.previewVessel = value;
                }
            }
        }

        private bool choosingVessel;
        private bool ChoosingVessel
        {
            get { return this.choosingVessel; }
            set
            {
                if (value != this.choosingVessel)
                {
                    ResetHeight();
                    if (value)
                    {
                        this.targetVesselListUI.SetSelectionToItem(this.targetVessel);
                        this.targetVesselListUI.SetToCurSelPage();
                    }
                }
                this.choosingVessel = value;
            }
        }

        private bool choosingTarget;
        private bool ChoosingTarget
        {
            get { return this.choosingTarget; }
            set
            {
                if (value != this.choosingTarget)
                {
                    ResetHeight();
                    if (value)
                    {
                        DA.CameraPaused = true;
                        if (this.targetVessel == null) { this.ChoosingVessel = true; }
                        else { this.PreviewVessel = this.targetVessel; }
                        this.targetListUI.SetSelectionToItem(DA.target);
                        this.targetListUI.SetToCurSelPage();
                        this.targetListUI.SetSelectionToItem(null);
                    }
                    else
                    {
                        this.PreviewPart = null;
                        this.targetListUI.SetSelectionToItem(null);
                        DA.CameraPaused = false;
                    }
                }
                this.choosingTarget = value;
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
        #endregion

        #region Constructor
        public PanelDockAssist() : base(new Rect(349, 215, 200, 252), GuidProvider.GetGuid<PanelDockAssist>(), "Docking Assistant") { }
        #endregion

        #region Methods
        private Vessel GetTargetVessel(ModuleDockAssistTarget mtgt)
        {
            return mtgt.vessel;
        }

        private bool IsOnTargetVessel(ModuleDockAssistTarget pm)
        {
            return pm.vessel == this.targetVessel;
        }

        private void DoPreviewVessel()
        {
            HydroFlightManager.Instance.CameraManager.FoV = 60;
            HydroFlightManager.Instance.CameraManager.Target = this.PreviewVessel.transform;
        }

        private void DrawChoosingCameraUI()
        {
            bool pageChanged, noCam;
            GUILayout.Label("Camera:");
            this.camListUI.OnDrawUI(DrawCamBtn, out pageChanged, out noCam);
            this.PreviewPart = this.camListUI.CurSelect;
            if (pageChanged) { ResetHeight(); }
            if (noCam) { GUILayout.Label("Not installed"); }
            GUILayout.BeginHorizontal();
            if (this.PreviewPart == null) { GUILayout.Button("OK", GUIUtils.ButtonStyle(Color.red)); }
            else if (GUILayout.Button("OK"))
            {
                DA.Cam = (ModuleDockAssistCam)this.PreviewPart;
                this.ChoosingCamera = false;
            }
            if (GUILayout.Button("Cancel")) { this.ChoosingCamera = false; }
            if (DA.Cam == null) { GUILayout.Button("Clear choice", GUIUtils.ButtonStyle(Color.red)); }
            else if (GUILayout.Button("Clear choice"))
            {
                DA.Cam = null;
                this.ChoosingCamera = false;
            }
            GUILayout.EndHorizontal();
        }

        private void DrawChoosingVesselUI()
        {
            GUILayout.Label("Vessel:");
            bool pageChanged;
            bool noTargetVessel;
            this.targetVesselListUI.OnDrawUI(DrawVesselBtn, out pageChanged, out noTargetVessel);
            this.PreviewVessel = this.targetVesselListUI.CurSelect;
            if (pageChanged) { ResetHeight(); }
            if (noTargetVessel) { GUILayout.Label("Nothing in sight"); }
            GUILayout.BeginHorizontal();
            if (this.PreviewVessel == null) { GUILayout.Button("OK", GUIUtils.ButtonStyle(Color.red)); }
            else if (GUILayout.Button("OK"))
            {
                this.targetVessel = this.PreviewVessel;
                this.ChoosingVessel = false;
            }
            if (GUILayout.Button("Cancel"))
            {
                this.ChoosingVessel = false;
                if (this.targetVessel == null)
                {
                    this.ChoosingTarget = false;
                    this.PreviewVessel = null;
                }
            }
            GUILayout.EndHorizontal();
        }

        private void DrawChoosingTargetUI()
        {
            GUILayout.Label("Vessel");
            if (GUILayout.Button(this.targetVessel.vesselName, this.targetVesselList.Contains(this.targetVessel) ? GUIUtils.ButtonStyle(Color.green, true) : GUIUtils.ButtonStyle(Color.red, true)))
            {
                this.ChoosingVessel = true;
            }
            GUILayout.Label("Target:");
            bool pageChanged;
            bool noTarget;
            this.targetListUI.OnDrawUI(DrawTargetBtn, out pageChanged, out noTarget);
            this.PreviewPart = this.targetListUI.CurSelect;
            if (this.PreviewPart != null) { this.previewVessel = null; }
            if (pageChanged) { ResetHeight(); }
            if (noTarget) { GUILayout.Label("Not installed"); }
            GUILayout.BeginHorizontal();
            if (this.PreviewPart == null) { GUILayout.Button("OK", GUIUtils.ButtonStyle(Color.red)); }
            else if (GUILayout.Button("OK"))
            {
                DA.target = (ModuleDockAssistTarget)this.PreviewPart;
                this.ChoosingTarget = false;
            }
            if (GUILayout.Button("Cancel"))
            {
                this.ChoosingTarget = false;
                this.PreviewVessel = null;
            }
            if (DA.target == null) { GUILayout.Button("Clear choice", GUIUtils.ButtonStyle(Color.red)); }
            else if (GUILayout.Button("Clear choice"))
            {
                this.targetVessel = null;
                DA.target = null;
                this.ChoosingTarget = false;
                this.PreviewVessel = null;
            }
            GUILayout.EndHorizontal();
        }
        #endregion

        #region Overrides
        public override void OnFlightStart()
        {
            base.OnFlightStart();
            this.camListUI = new UISingleSelectionList<ModuleDockAssistCam>(HydroFlightManager.Instance.ActiveCams);
            this.targetVesselList = new AffiliationList<ModuleDockAssistTarget, Vessel>(HydroFlightManager.Instance.NearbyTargets, (AffiliationList<ModuleDockAssistTarget, Vessel>.GetItemFunctionSingle)GetTargetVessel);
            this.targetVesselListUI = new UISingleSelectionList<Vessel>(this.targetVesselList);
            this.targetList = new SubList<ModuleDockAssistTarget>(HydroFlightManager.Instance.NearbyTargets, IsOnTargetVessel);
            this.targetListUI = new UISingleSelectionList<ModuleDockAssistTarget>(this.targetList);
            this.ChoosingCamera = false;
            this.ChoosingTarget = false;
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            this.targetVesselList.Update();
            this.targetList.OnUpdate();
            this.camListUI.OnUpdate();
            this.targetVesselListUI.OnUpdate();
            this.targetListUI.OnUpdate();
        }

        protected override void Window(int id)
        {
            GUI.DragWindow(this.drag);

            GUILayout.BeginVertical();
            if (this.ChoosingCamera) { DrawChoosingCameraUI(); }
            else if (this.ChoosingVessel) { DrawChoosingVesselUI(); }
            else if (this.ChoosingTarget) { DrawChoosingTargetUI(); }
            else if (this.Settings) { DrawSettingsUI(); }
            else
            {
                GUILayout.Label("Camera:");
                if (DA.Cam == null ? GUILayout.Button("Choose camera") : GUILayout.Button(DA.Cam.ToString(), DA.Cam.IsOnActiveVessel ? GUIUtils.ButtonStyle(Color.green, true) : GUIUtils.ButtonStyle(Color.red, true)))
                {
                    this.ChoosingCamera = true;
                }
                GUILayout.Label("Target:");
                if (DA.target == null ? GUILayout.Button("Choose target") : GUILayout.Button(DA.target.vessel.vesselName + "\n" + DA.target, DA.target.IsNear ? GUIUtils.ButtonStyle(Color.green, true) : GUIUtils.ButtonStyle(Color.red, true)))
                {
                    this.ChoosingTarget = true;
                }
                GUILayout.Label("Settings:");
                if (GUILayout.Button(DA.Manual ? "Manual docking" : "Automated docking"))
                {
                    this.Settings = true;
                }
                if (LayoutEngageBtn(this.Engaged)) { this.Engaged = !this.Engaged; }
            }
            GUILayout.EndVertical();
        }

        protected override void DrawSettingsUI()
        {
            GUILayout.Label("Angular acceleration (rad/s²)");
            this.angularAccText = GUILayout.TextField(this.angularAccText);
            GUILayout.Label("Acceleration (m/s²)");
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

            base.DrawSettingsUI();
        }

        protected virtual void DrawCamBtn(ModuleDockAssistCam mcam)
        {
            if (mcam == null) { GUILayout.Button(string.Empty); }
            else
            {
                if (GUILayout.Button(mcam.ToString(), mcam == DA.Cam ? ReferenceEquals(mcam, this.PreviewPart) ? GUIUtils.ButtonStyle(Color.blue, true) : GUIUtils.ButtonStyle(Color.green, true) : ReferenceEquals(mcam, this.PreviewPart) ? GUIUtils.ButtonStyle(Color.yellow, true) : GUIUtils.WrapButton))
                {
                    this.camListUI.SetSelectionToItem(mcam);
                }
            }
        }

        protected virtual void DrawVesselBtn(Vessel v)
        {
            if (v == null) { GUILayout.Button(""); }
            else
            {
                if (GUILayout.Button(v.vesselName, v == this.targetVessel ? v == this.PreviewVessel ? GUIUtils.ButtonStyle(Color.blue, true) : GUIUtils.ButtonStyle(Color.green, true) : v == this.PreviewVessel ? GUIUtils.ButtonStyle(Color.yellow, true) : GUIUtils.WrapButton))
                {
                    this.targetVesselListUI.SetSelectionToItem(v);
                }
            }
        }

        protected virtual void DrawTargetBtn(ModuleDockAssistTarget mtgt)
        {
            if (mtgt == null) { GUILayout.Button(""); }
            else
            {
                if (GUILayout.Button(mtgt.ToString(), mtgt == DA.target ? ReferenceEquals(mtgt, this.PreviewPart) ? GUIUtils.ButtonStyle(Color.blue, true) : GUIUtils.ButtonStyle(Color.green, true) : ReferenceEquals(mtgt, this.PreviewPart) ? GUIUtils.ButtonStyle(Color.yellow, true) : GUIUtils.WrapButton))
                {
                    this.targetListUI.SetSelectionToItem(mtgt);
                }
            }
        }
        #endregion
    }
}
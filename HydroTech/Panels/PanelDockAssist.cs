using System;
using HydroTech.Autopilots;
using HydroTech.Data;
using HydroTech.Managers;
using HydroTech.Panels.UI;
using HydroTech.Storage;
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

        private static ModuleDockAssistCam Cam
        {
            get { return DA.Cam; }
            set { DA.Cam = value; }
        }

        private static ModuleDockAssistTarget Target
        {
            get { return DA.target; }
            set { DA.target = value; }
        }

        protected static bool Manual
        {
            get { return DA.Manual; }
            set { DA.Manual = value; }
        }

        protected static bool ShowLine
        {
            get { return DA.ShowLine; }
            set { DA.ShowLine = value; }
        }

        protected static bool AutoOrient
        {
            get { return DA.AutoOrient; }
            set { DA.AutoOrient = value; }
        }

        protected static bool KillRelV
        {
            get { return DA.KillRelV; }
            set { DA.KillRelV = value; }
        }

        protected static bool CamView
        {
            get { return DA.CamView; }
            set { DA.CamView = value; }
        }

        protected static int CamMag
        {
            get
            {
                if (Cam == null) { throw new NullReferenceException("PanelDockAssist.CamMag<get> before a camera is selected"); }
                return Cam.Mag;
            }
            set
            {
                if (Cam == null) { throw new NullReferenceException("PanelDockAssist.CamMag<set> before a camera is selected"); }
                Cam.Mag = value;
            }
        }

        protected static float AngularAcc
        {
            get { return DA.angularAcc; }
            set { DA.angularAcc = value; }
        }

        protected static float Acc
        {
            get { return DA.acc; }
            set { DA.acc = value; }
        }

        protected static float FSpeed
        {
            get { return DA.finalStageSpeed; }
            set { DA.finalStageSpeed = value; }
        }

        protected static bool DriveTarget
        {
            get { return DA.DriveTarget; }
            set { DA.DriveTarget = value; }
        }

        protected static bool NullCamera
        {
            get { return DA.NullCamera; }
        }

        protected static bool NullTarget
        {
            get { return DA.NullTarget;}
        }

        protected static bool TargetHasJeb
        {
            get { return DA.TargetHasJeb;}
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
        protected ModuleDockAssist previewPart;
        protected ModuleDockAssist PreviewPart
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

        protected bool choosingCamera;
        protected bool ChoosingCamera
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
                        this.camListUI.SetSelectionToItem(Cam);
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

        protected Vessel previewVessel;
        protected Vessel PreviewVessel
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

        protected bool choosingVessel;
        protected bool ChoosingVessel
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

        protected bool choosingTarget;
        protected bool ChoosingTarget
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
                        this.targetListUI.SetSelectionToItem(Target);
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
        
        public override string PanelTitle
        {
            get { return "Docking Assistant"; }
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
                        this.tempAutoOrient = AutoOrient;
                        if (Cam != null) { this.tempCamMag = CamMag; }
                        this.TempCamView = CamView;
                        this.tempDriveTarget = DriveTarget;
                        this.tempKillRelV = KillRelV;
                        this.TempManual = Manual;
                        this.tempShowLine = ShowLine;
                        this.angularAccText = AngularAcc.ToString("#0.000");
                        this.accText = Acc.ToString("#0.000");
                        this.fssText = FSpeed.ToString("#0.000");
                    }
                    else //Applying settings
                    {
                        AutoOrient = this.tempAutoOrient;
                        if (Cam != null) { CamMag = this.tempCamMag; }
                        CamView = this.TempCamView;
                        DriveTarget = this.tempDriveTarget;
                        KillRelV = this.tempKillRelV;
                        Manual = this.TempManual;
                        ShowLine = this.tempShowLine;
                        float tryParse;
                        if (float.TryParse(this.angularAccText, out tryParse) && tryParse >= 0) { AngularAcc = tryParse; }
                        if (float.TryParse(this.accText, out tryParse) && tryParse >= 0) { Acc = tryParse; }
                        if (float.TryParse(this.fssText, out tryParse) && tryParse > 0) { FSpeed = tryParse; }
                    }
                }
                base.Settings = value;
            }
        }

        private readonly int id;
        protected override int ID
        {
            get { return this.id; }
        }
        #endregion

        #region Constructor
        public PanelDockAssist()
        {
            this.fileName = new FileName("dock", "cfg", FileName.panelSaveFolder);
            this.id = GuidProvider.GetGuid<PanelDockAssist>();
        }
        #endregion

        #region Methods
        protected Vessel GetTargetVessel(ModuleDockAssistTarget mtgt)
        {
            return mtgt.vessel;
        }

        protected bool IsOnTargetVessel(ModuleDockAssistTarget pm)
        {
            return pm.vessel == this.targetVessel;
        }

        protected void DoPreviewVessel()
        {
            HydroFlightManager.Instance.CameraManager.FoV = 60;
            HydroFlightManager.Instance.CameraManager.Target = this.PreviewVessel.transform;
        }

        protected void DrawChoosingCameraUI()
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
                Cam = (ModuleDockAssistCam)this.PreviewPart;
                this.ChoosingCamera = false;
            }
            if (GUILayout.Button("Cancel")) { this.ChoosingCamera = false; }
            if (Cam == null) { GUILayout.Button("Clear choice", GUIUtils.ButtonStyle(Color.red)); }
            else if (GUILayout.Button("Clear choice"))
            {
                Cam = null;
                this.ChoosingCamera = false;
            }
            GUILayout.EndHorizontal();
        }

        protected void DrawChoosingVesselUI()
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

        protected void DrawChoosingTargetUI()
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
                Target = (ModuleDockAssistTarget)this.PreviewPart;
                this.ChoosingTarget = false;
            }
            if (GUILayout.Button("Cancel"))
            {
                this.ChoosingTarget = false;
                this.PreviewVessel = null;
            }
            if (Target == null) { GUILayout.Button("Clear choice", GUIUtils.ButtonStyle(Color.red)); }
            else if (GUILayout.Button("Clear choice"))
            {
                this.targetVessel = null;
                Target = null;
                this.ChoosingTarget = false;
                this.PreviewVessel = null;
            }
            GUILayout.EndHorizontal();
        }
        #endregion

        #region Overrides
        protected override void SetDefaultWindowRect()
        {
            this.windowRect = new Rect(349, 215, 200, 252);
        }

        protected override void MakeAPSave()
        {
            DA.MakeSaveAtNextUpdate();
        }

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

        protected override void WindowGUI(int windowId)
        {
            GUILayout.BeginVertical();
            if (this.ChoosingCamera) { DrawChoosingCameraUI(); }
            else if (this.ChoosingVessel) { DrawChoosingVesselUI(); }
            else if (this.ChoosingTarget) { DrawChoosingTargetUI(); }
            else if (this.Settings) { DrawSettingsUI(); }
            else
            {
                GUILayout.Label("Camera:");
                if (Cam == null ? GUILayout.Button("Choose camera") : GUILayout.Button(Cam.ToString(), Cam.IsOnActiveVessel ? GUIUtils.ButtonStyle(Color.green, true) : GUIUtils.ButtonStyle(Color.red, true)))
                {
                    this.ChoosingCamera = true;
                }
                GUILayout.Label("Target:");
                if (Target == null ? GUILayout.Button("Choose target") : GUILayout.Button(Target.vessel.vesselName + "\n" + Target, Target.IsNear ? GUIUtils.ButtonStyle(Color.green, true) : GUIUtils.ButtonStyle(Color.red, true)))
                {
                    this.ChoosingTarget = true;
                }
                GUILayout.Label("Settings:");
                if (GUILayout.Button(Manual ? "Manual docking" : "Automated docking"))
                {
                    this.Settings = true;
                }
                if (LayoutEngageBtn(this.Engaged)) { this.Engaged = !this.Engaged; }
            }
            GUILayout.EndVertical();
            GUI.DragWindow();
        }

        protected override void DrawSettingsUI()
        {
            GUILayout.Label("Angular acceleration (" + HTUtils.angularAcc + ")");
            this.angularAccText = GUILayout.TextField(this.angularAccText);
            GUILayout.Label("Acceleration (" + HTUtils.acceleration + ")");
            this.accText = GUILayout.TextField(this.accText);

            if (!NullCamera)
            {
                if (!NullTarget)
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
                        GUILayout.Label(HTUtils.speedSimple);
                        GUILayout.EndHorizontal();
                        if (TargetHasJeb) { this.tempDriveTarget = GUILayout.Toggle(this.tempDriveTarget, "Rotate target"); }
                    }
                }

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

        protected virtual void DrawCamBtn(ModuleDockAssistCam mcam)
        {
            if (mcam == null) { GUILayout.Button(string.Empty); }
            else
            {
                if (GUILayout.Button(mcam.ToString(), mcam == Cam ? ReferenceEquals(mcam, this.PreviewPart) ? GUIUtils.ButtonStyle(Color.blue, true) : GUIUtils.ButtonStyle(Color.green, true) : ReferenceEquals(mcam, this.PreviewPart) ? GUIUtils.ButtonStyle(Color.yellow, true) : GUIUtils.WrapButton))
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
                if (GUILayout.Button(mtgt.ToString(), mtgt == Target ? ReferenceEquals(mtgt, this.PreviewPart) ? GUIUtils.ButtonStyle(Color.blue, true) : GUIUtils.ButtonStyle(Color.green, true) : ReferenceEquals(mtgt, this.PreviewPart) ? GUIUtils.ButtonStyle(Color.yellow, true) : GUIUtils.WrapButton))
                {
                    this.targetListUI.SetSelectionToItem(mtgt);
                }
            }
        }
        #endregion
    }
}
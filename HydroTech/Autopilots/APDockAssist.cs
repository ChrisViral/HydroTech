using HydroTech.Autopilots.Calculators;
using HydroTech.Constants;
using HydroTech.Data;
using HydroTech.File;
using HydroTech.InGame;
using HydroTech.Managers;
using HydroTech.Panels;
using HydroTech.PartModules;
using UnityEngine;

namespace HydroTech.Autopilots
{
    public class APDockAssist : RCSAutopilot
    {
        #region Static properties
        public static APDockAssist TheAutopilot
        {
            get { return (APDockAssist)HydroJebCore.autopilots[CoreConsts.apDock]; }
        }

        protected static Panel Panel
        {
            get { return PanelDockAssist.ThePanel; }
        }
        #endregion

        #region Fields
        protected RCSCalculator rcsTarget = new RCSCalculator();
        protected Vessel drivingTargetVessel;
        #endregion

        #region Properties
        protected override string NameString
        {
            get { return AutopilotConsts.dockingName; }
        }

        protected virtual string NameStringTarget
        {
            get { return AutopilotConsts.dockingTargetName; }
        }

        public bool NullCamera
        {
            get { return this.Cam == null || !this.Cam.IsOnActiveVessel;}
        }

        public bool NullTarget
        {
            get { return this.target == null || !this.target.IsNear;}
        }
    
        public bool TargetHasJeb
        {
            get { return this.jebsTargetVessel.Count != 0;}
        }

        public Vector3 RelV
        {
            get { return this.Cam.VectorTransform(this.target.vessel.obt_velocity - this.Cam.vessel.obt_velocity); }
        }
        #endregion

        #region User input vars
        [HydroSLNodeInfo(name = "SETTINGS"), HydroSLField(saveName = "ShowLine")]
        public bool showLine = AutopilotConsts.showLine;
        public bool ShowLine
        {
            get { return !this.NullCamera && !this.NullTarget && this.showLine; }
            set { this.showLine = value; }
        }

        [HydroSLNodeInfo(name = "SETTINGS"), HydroSLField(saveName = "AutoOrient")]
        public bool autoOrient = AutopilotConsts.autoOrient;
        public bool AutoOrient
        {
            get { return !this.NullCamera && !this.NullTarget && this.autoOrient; }
            set { this.autoOrient = value; }
        }

        [HydroSLNodeInfo(name = "SETTINGS"), HydroSLField(saveName = "KillRelV")]
        public bool killRelV = AutopilotConsts.killRelV;
        public bool KillRelV
        {
            get { return !this.NullCamera && !this.NullTarget && this.killRelV; }
            set { this.killRelV = value; }
        }

        [HydroSLNodeInfo(name = "SETTINGS"), HydroSLField(saveName = "CamView")]
        public bool camView = AutopilotConsts.camView;
        public bool CamView
        {
            get { return !this.NullCamera && this.camView; }
            set
            {
                if (!this.NullCamera && this.Engaged && !this.CameraPaused) { this.Cam.CamActivate = value; }
                this.camView = value;
            }
        }

        [HydroSLNodeInfo(name = "SETTINGS"), HydroSLField(saveName = "Manual")]
        public bool manual = AutopilotConsts.manual;
        public bool Manual
        {
            get { return this.NullCamera || this.NullTarget || this.manual; }
            set
            {
                if (this.manual != value && this.Engaged && this.DriveTarget)
                {
                    if (value) { RemoveDriveTarget(); }
                    else
                    {
                        AddDriveTarget();
                    }
                }
                this.manual = value;
            }
        }

        [HydroSLNodeInfo(name = "SETTINGS"), HydroSLField(saveName = "DriveTarget")]
        public bool driveTarget = AutopilotConsts.driveTarget;
        public bool DriveTarget
        {
            get { return !this.Manual && this.TargetHasJeb && this.driveTarget; }
            set
            {
                if (!this.Active || this.Manual || !this.TargetHasJeb) { return; }
                if (value && !this.driveTarget) { this.drivingTargetVessel = this.target.vessel; }
                if (this.Engaged)
                {
                    if (value && !this.driveTarget) { AddDriveTarget(); }
                    else if (!value && this.driveTarget) { RemoveDriveTarget(); }
                }
                this.driveTarget = value;
            }
        }

        [HydroSLNodeInfo(name = "SETTINGS"), HydroSLField(saveName = "AngularAcc")]
        public float angularAcc = AutopilotConsts.dockingAngularAcc;

        [HydroSLNodeInfo(name = "SETTINGS"), HydroSLField(saveName = "Acc")]
        public float acc = AutopilotConsts.dockingAcc;

        [HydroSLNodeInfo(name = "SETTINGS"), HydroSLField(saveName = "FinalSpeed")]
        public float finalStageSpeed = AutopilotConsts.finalStageSpeed;

        protected ModuleDockAssistCam cam = AutopilotConsts.cam;
        public ModuleDockAssistCam Cam
        {
            get { return this.cam; }
            set
            {
                if (this.cam != null && value != this.cam) { this.cam.CamActivate = false; }
                this.cam = value;
            }
        }

        public override bool Engaged
        {
            set
            {
                if (!this.Active) { return; }
                if (this.DriveTarget)
                {
                    if (value) { AddDriveTarget(); }
                    else
                    {
                        RemoveDriveTarget();
                    }
                }
                if (value) { if (this.CamView && !this.CameraPaused) { this.Cam.CamActivate = true; } }
                else
                {
                    Panel.ResetHeight();
                    if (!this.NullCamera) { this.Cam.CamActivate = false; }
                }
                base.Engaged = value;
            }
        }

        public ModuleDockAssistTarget target = AutopilotConsts.target;
        #endregion

        #region Autopilot vars
        protected bool cameraPaused;
        public bool CameraPaused
        {
            get { return this.Engaged && this.CamView && this.cameraPaused; }
            set
            {
                if (this.Engaged && this.CamView)
                {
                    if (value)
                    {
                        HydroFlightCameraManager.SaveCurrent();
                        HydroFlightCameraManager.ResetToActiveVessel();
                    }
                    else
                    {
                        HydroFlightCameraManager.RetrieveLast();
                    }
                }
                this.cameraPaused = value;
            }
        }

        public override bool Active
        {
            set
            {
                if (value != base.Active && this.Engaged && !this.Manual && this.driveTarget) //TargetHasJeb() may return false
                {
                    if (value) { AddDriveTarget(); }
                    else
                    {
                        RemoveDriveTarget();
                    }
                }
                base.Active = value;
            }
        }

        protected bool targetOrientReady = true;
        protected LineRenderer line;
        public SubList<Part> jebsTargetVessel;
        #endregion

        #region Constructor
        public APDockAssist()
        {
            this.fileName = new FileName("dock", "cfg", HydroJebCore.autopilotSaveFolder);
            this.jebsTargetVessel = new SubList<Part>(HydroJebCore.jebs.ListInactiveVessel, IsJebTargetVessel);
        }
        #endregion

        #region Autopilot
        protected virtual void DriveAutoOrient(FlightCtrlState ctrlState)
        {
            DockingAssistCalculator stateCal = new DockingAssistCalculator();
            stateCal.Calculate(this.Cam, this.target);
            stateCal.SetCtrlStateRotation(ctrlState);
        }

        protected virtual void DriveTargetAutopilot(FlightCtrlState ctrlState)
        {
            TurnOnRcs(this.target.vessel);
            TurnOffSas(this.target.vessel);
            Vector3 dir = (this.target.Pos - this.Cam.Pos).normalized;
            HoldDirectionCalculator stateCal = new HoldDirectionCalculator();
            stateCal.Calculate(dir, Vector3.zero, this.target.Dir, this.target.Right, this.target.vessel);
            stateCal.SetCtrlStateRotation(ctrlState);
            this.targetOrientReady = stateCal.Steer(AutopilotConsts.translationReadyAngleSin);
            this.rcsTarget.OnUpdate(this.target.vessel);
            this.rcsTarget.MakeRotation(ctrlState, this.angularAcc);
        }

        protected virtual void DriveKillRelV(FlightCtrlState ctrlState)
        {
            Vector3 relVCam = RelV;
            ctrlState.X = -relVCam.x / AutopilotConsts.vel0;
            ctrlState.Y = -relVCam.y / AutopilotConsts.vel0;
            ctrlState.Z = -relVCam.z / AutopilotConsts.vel0;
            Vector3 translationRate = new Vector3(RcsActive.GetThrustRateFromAcc6(ctrlState.X >= 0 ? 0 : 1, this.acc), RcsActive.GetThrustRateFromAcc6(ctrlState.Y >= 0 ? 2 : 3, this.acc), RcsActive.GetThrustRateFromAcc6(ctrlState.Z >= 0 ? 4 : 5, this.acc));
            ctrlState.X /= translationRate.x;
            ctrlState.Y /= translationRate.y;
            ctrlState.Z /= translationRate.z;
            if (Mathf.Abs(ctrlState.X) > 1) { ctrlState.X = ctrlState.X > 0 ? 1 : -1; }
            if (Mathf.Abs(ctrlState.Y) > 1) { ctrlState.Y = ctrlState.Y > 0 ? 1 : -1; }
            if (Mathf.Abs(ctrlState.Z) > 1) { ctrlState.Z = ctrlState.Z > 0 ? 1 : -1; }
        }

        protected virtual void DriveFinalStage(FlightCtrlState ctrlState, Vector3 relPTarget, Vector3 relVCam)
        {
            if (relVCam.z > AutopilotConsts.stopSpeed || relVCam.z < -this.finalStageSpeed * AutopilotConsts.finalStageSpeedMaxMultiplier) { DriveKillRelV(ctrlState); }
            else
            {
                ctrlState.X = -relPTarget.x / AutopilotConsts.finalStageErr;
                ctrlState.Y = -relPTarget.y / AutopilotConsts.finalStageErr;
                if (relVCam.x * relPTarget.x < 0) { ctrlState.X /= 2; }
                if (relVCam.y * relPTarget.y < 0) { ctrlState.Y /= 2; }
                if (relVCam.z < -this.finalStageSpeed) { ctrlState.Z = 0; }
                else
                {
                    ctrlState.Z = -1;
                }
            }
        }

        protected virtual void DriveAutoDocking(FlightCtrlState ctrlState)
        {
            DockingAssistCalculator stateCal = new DockingAssistCalculator();
            stateCal.Calculate(this.Cam, this.target);
            DriveAutoOrient(ctrlState);
            if (!stateCal.Steer(AutopilotConsts.translationReadyAngleSin)) { DriveKillRelV(ctrlState); }
            else //HoldErr
            {
                Vector3 relPTarget = new Vector3(stateCal.x, stateCal.y, stateCal.z);
                Vector2 relPTargetXy = new Vector2(stateCal.x, stateCal.y);
                Vector3 relVCam = RelV;
                Vector2 relVCamXy = new Vector2(relVCam.x, relVCam.y);
                if (relPTargetXy.magnitude < AutopilotConsts.finalStageErr && stateCal.z < AutopilotConsts.finalStagePos.z + AutopilotConsts.finalStageErr && stateCal.z > 0) { DriveFinalStage(ctrlState, relPTarget, relVCam); }
                else if (relVCam.magnitude > AutopilotConsts.maxSpeed) { DriveKillRelV(ctrlState); }
                else //< MaxSpeed
                {
                    if (stateCal.z < AutopilotConsts.minZ)
                    {
                        if (relPTargetXy.magnitude < AutopilotConsts.minXy)
                        {
                            if (Mathf.Abs(relVCam.z) > AutopilotConsts.stopSpeed) //Still moving in Z
                            {
                                DriveKillRelV(ctrlState);
                            }
                            else
                            {
                                if (Vector3.Dot(relPTargetXy.normalized, relVCamXy.normalized) < AutopilotConsts.maxTranslationErrAngleCos && relVCamXy.magnitude > AutopilotConsts.stopSpeed) //Moving inwards
                                {
                                    DriveKillRelV(ctrlState);
                                }
                                else if (relVCamXy.magnitude > AutopilotConsts.safeSpeed)
                                {
                                    ctrlState.X = 0;
                                    ctrlState.Y = 0;
                                    ctrlState.Z = 0;
                                }
                                else
                                {
                                    ctrlState.X = relPTargetXy.normalized.x;
                                    ctrlState.Y = relPTargetXy.normalized.y;
                                    ctrlState.Z = 0;
                                }
                            }
                        }
                        else //>= MinXY
                        {
                            if (relVCamXy.magnitude > AutopilotConsts.stopSpeed) //Moving in XY
                            {
                                DriveKillRelV(ctrlState);
                            }
                            else
                            {
                                ctrlState.X = 0;
                                ctrlState.Y = 0;
                                ctrlState.Z = relVCam.z > AutopilotConsts.safeSpeed ? 0 : 1;
                            }
                        }
                    }
                    else // >= MinZ
                    {
                        Vector3 diff = (AutopilotConsts.finalStagePos - relPTarget).normalized;
                        if (Vector3.Dot(relVCam.normalized, diff) < AutopilotConsts.maxTranslationErrAngleCos && relVCam.magnitude > AutopilotConsts.stopSpeed) { DriveKillRelV(ctrlState); }
                        else if (relVCam.magnitude > AutopilotConsts.safeSpeed)
                        {
                            ctrlState.X = 0;
                            ctrlState.Y = 0;
                            ctrlState.Z = 0;
                        }
                        else
                        {
                            ctrlState.X = diff.x;
                            ctrlState.Y = diff.y;
                            ctrlState.Z = diff.z;
                        }
                    }
                }
            }
            CamToVessel_Trans(ctrlState, this.Cam);
        }

        protected override void DriveAutopilot(FlightCtrlState ctrlState)
        {
            base.DriveAutopilot(ctrlState);
            if (this.Manual)
            {
                if (this.AutoOrient)
                {
                    HydroActionGroupManager.ActiveVessel.SAS = false;
                    DriveAutoOrient(ctrlState);
                }
                if (this.CamView && !this.AutoOrient)
                {
                    HydroActionGroupManager.ActiveVessel.SAS = false;
                    CamToVessel_Rot(ctrlState, this.Cam);
                }
                bool killingRelV = this.KillRelV && ctrlState.X == 0 && ctrlState.Y == 0 && ctrlState.Z == 0;
                if (killingRelV)
                {
                    DriveKillRelV(ctrlState);
                    CamToVessel_Trans(ctrlState, this.Cam);
                }
                else if (this.CamView) { CamToVessel_Trans(ctrlState, this.Cam); }
            }
            else //!Manual
            {
                HydroActionGroupManager.ActiveVessel.SAS = false;
                if (this.DriveTarget)
                {
                    if (this.targetOrientReady)
                    {
                        Vector3 r = this.target.Pos - this.Cam.Pos;
                        HoldDirectionCalculator stateCal = new HoldDirectionCalculator();
                        stateCal.Calculate(r.normalized, this.target.Right, this.Cam.Dir, this.Cam.Right, ActiveVessel);
                        stateCal.SetCtrlStateRotation(ctrlState);
                        bool orientReady = stateCal.Steer(AutopilotConsts.translationReadyAngleSin);
                        if (orientReady && ActiveVessel.GetComponent<Rigidbody>().angularVelocity.magnitude < AutopilotConsts.maxAngularV)
                        {
                            DriveFinalStage(ctrlState, VectorTransform(r, this.target.Right, this.target.Down, this.target.Dir), RelV);
                            CamToVessel_Trans(ctrlState, this.Cam);
                        }
                        else
                        {
                            DriveKillRelV(ctrlState);
                            CamToVessel_Trans(ctrlState, this.Cam);
                        }
                    }
                    else
                    {
                        DriveKillRelV(ctrlState);
                        CamToVessel_Trans(ctrlState, this.Cam);
                        ctrlState.yaw = 0;
                        ctrlState.roll = 0;
                        ctrlState.pitch = 0;
                    }
                }
                else
                {
                    DriveAutoDocking(ctrlState);
                }
            }
            RcsActive.MakeRotation(ctrlState, this.angularAcc);
            RcsActive.MakeTranslation(ctrlState, this.acc);
        }
        #endregion

        #region Methods
        protected bool IsJebTargetVessel(Part jeb)
        {
            return !this.NullTarget && jeb.vessel == this.target.vessel;
        }

        protected void AddDriveTarget()
        {
            HydroFlightInputManager.AddOnFlyByWire(this.target.vessel, this.NameStringTarget, DriveTargetAutopilot);
            this.drivingTargetVessel = this.target.vessel;
        }

        protected void RemoveDriveTarget()
        {
            HydroFlightInputManager.RemoveOnFlyByWire(this.target.vessel, this.NameStringTarget, DriveTargetAutopilot);
            this.drivingTargetVessel = null;
        }

        protected void ClearDockAssistLine()
        {
            this.line.SetWidth(0, 0);
            this.line.SetPosition(0, Vector3.zero);
            this.line.SetPosition(1, Vector3.zero);
        }
        #endregion

        #region Static Methods
        public static void CamToVessel_Rot(FlightCtrlState ctrlState, ModuleDockAssistCam mcam)
        {
            SwitchTransformCalculator sCal = new SwitchTransformCalculator();
            sCal.GetRotation(ctrlState);
            sCal.ChangeTransformRotation(mcam.Right, mcam.Down, mcam.Dir, mcam.VesselTransform);
            sCal.SetRotation(ctrlState);
        }

        public static void CamToVessel_Trans(FlightCtrlState ctrlState, ModuleDockAssistCam mcam)
        {
            SwitchTransformCalculator sCal = new SwitchTransformCalculator();
            sCal.GetTranslation(ctrlState);
            sCal.ChangeTransformTranslation(mcam.Right, mcam.Down, mcam.Dir, mcam.VesselTransform);
            sCal.SetTranslation(ctrlState);
        }
        #endregion

        #region Overrides
        public override void OnFlightStart()
        {
            base.OnFlightStart();

            //Drawing line learned from Kethane (there was a pump line before 0.18) |> stupid_chris: damn I feel old.
            GameObject obj = new GameObject("Docking Assist Line");
            this.line = obj.AddComponent<LineRenderer>();
            this.line.useWorldSpace = false;
            this.line.material = new Material(Shader.Find("Particles/Additive"));
            this.line.SetColors(Color.green, Color.blue);
            this.line.SetVertexCount(2);
            ClearDockAssistLine();
            this.drivingTargetVessel = null;
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            if (!this.ShowLine) { ClearDockAssistLine(); }
            else
            {
                this.line.SetWidth(0.01f, 0.01f);
                this.line.SetPosition(0, this.Cam.CrossPos);
                this.line.SetPosition(1, this.target.Pos);
            }

            if (this.Engaged)
            {
                if (this.DriveTarget)
                {
                    if (this.drivingTargetVessel != this.target.vessel) //Vessel change detected
                    {
                        if (this.drivingTargetVessel != null) { HydroFlightInputManager.RemoveOnFlyByWire(this.drivingTargetVessel, this.NameStringTarget, DriveTargetAutopilot); }
                        AddDriveTarget();
                    }
                }
                else if (this.drivingTargetVessel != null) // ActiveVessel docked to TargetVessel
                {
                    RemoveDriveTarget();
                }
            }

            if (!this.NullTarget) { this.jebsTargetVessel.OnUpdate(); }
        }

        protected override void LoadDefault()
        {
            base.LoadDefault();
            this.AutoOrient = AutopilotConsts.autoOrient;
            this.CamView = AutopilotConsts.camView;
            this.driveTarget = AutopilotConsts.driveTarget;
            this.KillRelV = AutopilotConsts.killRelV;
            this.Manual = AutopilotConsts.manual;
            this.ShowLine = AutopilotConsts.showLine;
            this.finalStageSpeed = AutopilotConsts.finalStageSpeed;
            this.angularAcc = AutopilotConsts.dockingAngularAcc;
            this.acc = AutopilotConsts.dockingAcc;
            this.Cam = AutopilotConsts.cam;
            this.target = AutopilotConsts.target;
        }
        #endregion
    }
}
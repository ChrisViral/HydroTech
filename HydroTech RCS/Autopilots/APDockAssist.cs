using UnityEngine;
using HydroTech_FC;
using HydroTech_RCS.Autopilots.ASAS;
using HydroTech_RCS.Autopilots.Modules;
using HydroTech_RCS.Constants.Autopilots.Docking;
using HydroTech_RCS.Constants.Core;
using HydroTech_RCS.Panels;

namespace HydroTech_RCS.Autopilots
{
    public class APDockAssist : RCSAutopilot
    {
        #region Static properties
        public static APDockAssist theAutopilot
        {
            get { return (APDockAssist)HydroJebCore.autopilots[AutopilotIDs.Dock]; }
        }

        protected static Panel panel
        {
            get { return PanelDockAssist.thePanel; }
        }
        #endregion

        #region Constructor
        public APDockAssist()
        {
            this.fileName = new FileName("dock", "cfg", HydroJebCore.AutopilotSaveFolder);
            this.jebsTargetVessel = new SubList<Part>(HydroJebCore.jebs.listInactiveVessel, this.isJebTargetVessel);
        }
        #endregion

        #region Properties
        protected override string nameString
        {
            get { return Str.nameString; }
        }

        protected virtual string nameString_Target
        {
            get { return Str.nameString_Target; }
        }
        #endregion

        #region Fields
        protected CalculatorRCSThrust RCSTarget = new CalculatorRCSThrust();
        protected Vessel drivingTargetVessel = null;
        #endregion

        #region User input vars
        [HydroSLNodeInfo(name = "SETTINGS")]
        [HydroSLField(saveName = "ShowLine")]
        public bool _showLine = Default.BOOL.ShowLine;
        public bool showLine
        {
            get { return NullCamera() || NullTarget() ? false : this._showLine; }
            set { this._showLine = value; }
        }

        [HydroSLNodeInfo(name = "SETTINGS")]
        [HydroSLField(saveName = "AutoOrient")]
        public bool _autoOrient = Default.BOOL.AutoOrient;
        public bool autoOrient
        {
            get { return NullCamera() || NullTarget() ? false : this._autoOrient; }
            set { this._autoOrient = value; }
        }

        [HydroSLNodeInfo(name = "SETTINGS")]
        [HydroSLField(saveName = "KillRelV")]
        public bool _killRelV = Default.BOOL.KillRelV;
        public bool killRelV
        {
            get { return NullCamera() || NullTarget() ? false : this._killRelV; }
            set { this._killRelV = value; }
        }

        [HydroSLNodeInfo(name = "SETTINGS")]
        [HydroSLField(saveName = "CamView")]
        public bool _camView = Default.BOOL.CamView;
        public bool camView
        {
            get { return NullCamera() ? false : this._camView; }
            set
            {
                if (!NullCamera() && this.engaged && !this.cameraPaused) { this.cam.CamActivate = value; }
                this._camView = value;
            }
        }

        [HydroSLNodeInfo(name = "SETTINGS")]
        [HydroSLField(saveName = "Manual")]
        public bool _manual = Default.BOOL.Manual;
        public bool manual
        {

            get { return NullCamera() || NullTarget() ? true : this._manual; }
            set
            {
                if (this._manual != value && this.engaged && this.driveTarget)
                {
                    if (value) { RemoveDriveTarget(); }
                    else { AddDriveTarget(); }
                }
                this._manual = value;
            }
        }

        [HydroSLNodeInfo(name = "SETTINGS")]
        [HydroSLField(saveName = "DriveTarget")]
        public bool _driveTarget = Default.BOOL.DriveTarget;
        public bool driveTarget
        {
            get { return this.manual || !TargetHasJeb() ? false : this._driveTarget; }
            set
            {
                if (!this.active || this.manual || !TargetHasJeb()) { return; }
                if (value && !this._driveTarget) { this.drivingTargetVessel = this.target.vessel; }
                if (this.engaged)
                {
                    if (value && !this._driveTarget) { AddDriveTarget(); }
                    else if (!value && this._driveTarget) { RemoveDriveTarget(); }
                }
                this._driveTarget = value;
            }
        }
        
        [HydroSLNodeInfo(name = "SETTINGS")]
        [HydroSLField(saveName = "AngularAcc")]
        public float angularAcc = Default.FLOAT.AngularAcc;

        [HydroSLNodeInfo(name = "SETTINGS")]
        [HydroSLField(saveName = "Acc")]
        public float acc = Default.FLOAT.Acc;

        [HydroSLNodeInfo(name = "SETTINGS")]
        [HydroSLField(saveName = "FinalSpeed")]
        public float finalStageSpeed = Default.FLOAT.FinalStageSpeed;
        
        protected ModuleDockAssistCam _cam = Default.MISC.Cam;
        public ModuleDockAssistCam cam
        {
            get { return _cam; }
            set
            {
                if (this._cam != null && value != this._cam) { this._cam.CamActivate = false; }
                this._cam = value;
            }
        }

        public ModuleDockAssistTarget target = Default.MISC.Target;
        
        public override bool engaged
        {
            set
            {
                if (!this.active) { return; }
                if (this.driveTarget)
                {
                    if (value) { AddDriveTarget(); }
                    else { RemoveDriveTarget(); }
                }
                if (value)
                {
                    if (camView && !cameraPaused) { cam.CamActivate = true; }
                }
                else
                {
                    panel.ResetHeight();
                    if (!NullCamera()) { cam.CamActivate = false; }
                }
                base.engaged = value;
            }
        }
        #endregion

        #region Autopilot vars
        protected bool _cameraPaused = false;
        public bool cameraPaused
        {
            get { return !this.engaged || !this.camView ? false : this._cameraPaused; }
            set
            {
                if (this.engaged && this.camView)
                {
                    if (value)
                    {
                        HydroFlightCameraManager.SaveCurrent();
                        HydroFlightCameraManager.ResetToActiveVessel();
                    }
                    else { HydroFlightCameraManager.RetrieveLast(); }
                }
                this._cameraPaused = value;
            }
        }

        protected bool targetOrientReady = true;
        
        protected LineRenderer line = null;
        public SubList<Part> jebsTargetVessel = null;
        
        public override bool active
        {
            set
            {
                if (value != base.active && this.engaged && !this.manual && this._driveTarget) //TargetHasJeb() may return false
                {
                    if (value) { AddDriveTarget(); }
                    else { RemoveDriveTarget(); }
                }
                base.active = value;
            }
        }
        #endregion

        #region Methods
        public bool NullCamera()
        {
            return this.cam == null || !this.cam.IsOnActiveVessel();
        }

        public bool NullTarget()
        {
            return this.target == null || !this.target.IsNear();
        }

        protected bool isJebTargetVessel(Part jeb)
        {
            return NullTarget() ? false : jeb.vessel == this.target.vessel;
        }

        public bool TargetHasJeb()
        {
            return this.jebsTargetVessel.Count != 0;
        }    

        protected void AddDriveTarget()
        {
            HydroFlightInputManager.AddOnFlyByWire(this.target.vessel, this.nameString_Target, this.DriveTargetAutopilot);
            this.drivingTargetVessel = this.target.vessel;
        }

        protected void RemoveDriveTarget()
        {
            HydroFlightInputManager.RemoveOnFlyByWire(this.target.vessel, this.nameString_Target, this.DriveTargetAutopilot);
            this.drivingTargetVessel = null;
        }

        protected void ClearDockAssistLine()
        {
            line.SetWidth(0, 0);
            line.SetPosition(0, Vector3.zero);
            line.SetPosition(1, Vector3.zero);
        }

        protected Vector3 RelV()
        {
            return this.cam.VectorTransform(this.target.vessel.obt_velocity - this.cam.vessel.obt_velocity);
        }
        #endregion

        #region Autopilot
        protected virtual void DriveAutoOrient(FlightCtrlState ctrlState)
        {
            DockAssistStateCalculator stateCal = new DockAssistStateCalculator();
            stateCal.Calculate(this.cam, this.target);
            stateCal.SetCtrlStateRotation(ctrlState);
        }

        protected virtual void DriveTargetAutopilot(FlightCtrlState ctrlState)
        {
            TurnOnRCS(this.target.vessel);
            TurnOffSAS(this.target.vessel);
            Vector3 dir = (this.target.Pos - this.cam.Pos).normalized;
            HoldDirStateCalculator stateCal = new HoldDirStateCalculator();
            stateCal.Calculate(dir, Vector3.zero, this.target.Dir, this.target.Right, this.target.vessel);
            stateCal.SetCtrlStateRotation(ctrlState);
            this.targetOrientReady = stateCal.Steer(Angle.TranslationReadyAngleSin);
            RCSTarget.OnUpdate(this.target.vessel);
            RCSTarget.MakeRotation(ctrlState, this.angularAcc);
        }

        protected virtual void DriveKillRelV(FlightCtrlState ctrlState)
        {
            Vector3 RelV_Cam = RelV();
            ctrlState.X = -RelV_Cam.x / Velocity.Vel0;
            ctrlState.Y = -RelV_Cam.y / Velocity.Vel0;
            ctrlState.Z = -RelV_Cam.z / Velocity.Vel0;
            Vector3 translationRate = new Vector3(
                RCSActive.GetThrustRateFromAcc6(ctrlState.X >= 0 ? 0 : 1, acc),
                RCSActive.GetThrustRateFromAcc6(ctrlState.Y >= 0 ? 2 : 3, acc),
                RCSActive.GetThrustRateFromAcc6(ctrlState.Z >= 0 ? 4 : 5, acc));
            ctrlState.X /= translationRate.x;
            ctrlState.Y /= translationRate.y;
            ctrlState.Z /= translationRate.z;
            if (HMaths.Abs(ctrlState.X) > 1) { ctrlState.X = ctrlState.X > 0 ? 1 : -1; }
            if (HMaths.Abs(ctrlState.Y) > 1) { ctrlState.Y = ctrlState.Y > 0 ? 1 : -1; }
            if (HMaths.Abs(ctrlState.Z) > 1) { ctrlState.Z = ctrlState.Z > 0 ? 1 : -1; }
        }

        protected virtual void DriveFinalStage(FlightCtrlState ctrlState, Vector3 RelP_Target, Vector3 RelV_Cam)
        {
            if (RelV_Cam.z > Velocity.StopSpeed || RelV_Cam.z < -finalStageSpeed * Velocity.FinalStageSpeedMaxMultiplier)
            {
                DriveKillRelV(ctrlState);
            }
            else
            {
                ctrlState.X = -RelP_Target.x / Position.FinalStageErr;
                ctrlState.Y = -RelP_Target.y / Position.FinalStageErr;
                if (RelV_Cam.x * RelP_Target.x < 0) { ctrlState.X /= 2; }
                if (RelV_Cam.y * RelP_Target.y < 0) { ctrlState.Y /= 2; }
                if (RelV_Cam.z < -finalStageSpeed) { ctrlState.Z = 0; }
                else { ctrlState.Z = -1; }
            }
        }

        protected virtual void DriveAutoDocking(FlightCtrlState ctrlState)
        {
            DockAssistStateCalculator stateCal = new DockAssistStateCalculator();
            stateCal.Calculate(cam, target);
            DriveAutoOrient(ctrlState);
            //RCSActive.MakeRotation(ctrlState, AngularAcc);
            if (!stateCal.Steer(Angle.TranslationReadyAngleSin)) { DriveKillRelV(ctrlState); }
            else //HoldErr
            {
                Vector3 RelP_Target = new Vector3(stateCal.X, stateCal.Y, stateCal.Z);
                Vector2 RelP_Target_XY = new Vector2(stateCal.X, stateCal.Y);
                Vector3 RelV_Cam = RelV();
                Vector2 RelV_Cam_XY = new Vector2(RelV_Cam.x, RelV_Cam.y);
                if (RelP_Target_XY.magnitude < Position.FinalStageErr && stateCal.Z < Position.FinalStagePos.z + Position.FinalStageErr && stateCal.Z > 0)
                {
                    DriveFinalStage(ctrlState, RelP_Target, RelV_Cam);
                }
                else if (RelV_Cam.magnitude > Velocity.MaxSpeed) { DriveKillRelV(ctrlState); }
                else //< MaxSpeed
                {
                    if (stateCal.Z < Position.MinZ)
                    {
                        if (RelP_Target_XY.magnitude < Position.MinXY)
                        {
                            if (HMaths.Abs(RelV_Cam.z) > Velocity.StopSpeed) //Still moving in Z
                            {
                                DriveKillRelV(ctrlState);
                            }
                            else
                            {
                                if (HMaths.DotProduct(RelP_Target_XY.normalized, RelV_Cam_XY.normalized) < Angle.MaxTranslationErrAngleCos && RelV_Cam_XY.magnitude > Velocity.StopSpeed)//Moving inwards
                                {
                                    DriveKillRelV(ctrlState);
                                }
                                else if (RelV_Cam_XY.magnitude > Velocity.SafeSpeed)
                                {
                                    ctrlState.X = 0;
                                    ctrlState.Y = 0;
                                    ctrlState.Z = 0;
                                }
                                else
                                {
                                    ctrlState.X = RelP_Target_XY.normalized.x;
                                    ctrlState.Y = RelP_Target_XY.normalized.y;
                                    ctrlState.Z = 0;
                                }
                            }
                        }
                        else //>= MinXY
                        {
                            if (RelV_Cam_XY.magnitude > Velocity.StopSpeed) //Moving in XY
                            {
                                DriveKillRelV(ctrlState);
                            }
                            else
                            {
                                ctrlState.X = 0;
                                ctrlState.Y = 0;
                                ctrlState.Z = RelV_Cam.z > Velocity.SafeSpeed ? 0 : 1;
                            }
                        }
                    }
                    else // >= MinZ
                    {
                        Vector3 diff = (Position.FinalStagePos - RelP_Target).normalized;
                        if (HMaths.DotProduct(RelV_Cam.normalized, diff) < Angle.MaxTranslationErrAngleCos && RelV_Cam.magnitude > Velocity.StopSpeed)
                        {
                            DriveKillRelV(ctrlState);
                        }
                        else if (RelV_Cam.magnitude > Velocity.SafeSpeed)
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
            CamToVessel_Trans(ctrlState, this.cam);
        }

        protected override void DriveAutopilot(FlightCtrlState ctrlState)
        {
            base.DriveAutopilot(ctrlState);
            if (this.manual)
            {
                if (this.autoOrient)
                {
                    HydroActionGroupManager.ActiveVessel.SAS = false;
                    DriveAutoOrient(ctrlState);
                }
                if (this.camView && !this.autoOrient)
                {
                    HydroActionGroupManager.ActiveVessel.SAS = false;
                    CamToVessel_Rot(ctrlState, this.cam);
                }
                bool KillingRelV = this.killRelV && ctrlState.X == 0 && ctrlState.Y == 0 && ctrlState.Z == 0;
                if (KillingRelV)
                {
                    DriveKillRelV(ctrlState);
                    CamToVessel_Trans(ctrlState, this.cam);
                }
                else if (camView) { CamToVessel_Trans(ctrlState, this.cam); }
            }
            else //!Manual
            {
                HydroActionGroupManager.ActiveVessel.SAS = false;
                if (driveTarget)
                {
                    if (targetOrientReady)
                    {
                        Vector3 r = target.Pos - cam.Pos;
                        HoldDirStateCalculator stateCal = new HoldDirStateCalculator();
                        stateCal.Calculate(r.normalized, this.target.Right, this.cam.Dir, this.cam.Right, ActiveVessel);
                        stateCal.SetCtrlStateRotation(ctrlState);
                        bool OrientReady = stateCal.Steer(Angle.TranslationReadyAngleSin);
                        if (OrientReady && ActiveVessel.GetComponent<Rigidbody>().angularVelocity.magnitude < AngularV.MaxAngularV)
                        {
                            DriveFinalStage(ctrlState, VectorTransform(r, this.target.Right, this.target.Down, this.target.Dir), RelV());
                            CamToVessel_Trans(ctrlState, this.cam);
                        }
                        else
                        {
                            DriveKillRelV(ctrlState);
                            CamToVessel_Trans(ctrlState, this.cam);
                        }
                    }
                    else
                    {
                        DriveKillRelV(ctrlState);
                        CamToVessel_Trans(ctrlState, this.cam);
                        ctrlState.yaw = 0;
                        ctrlState.roll = 0;
                        ctrlState.pitch = 0;
                    }
                }
                else
                    DriveAutoDocking(ctrlState);
            }
            RCSActive.MakeRotation(ctrlState, this.angularAcc);
            RCSActive.MakeTranslation(ctrlState, this.acc);
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

        #region Functions
        public override void OnFlightStart()
        {
            base.OnFlightStart();

            //Drawing line learned from Kethane (there was a pump line before 0.18) |> stupid_chris: damn I feel old.
            GameObject obj = new GameObject("Docking Assist Line");
            line = obj.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.material = new Material(Shader.Find("Particles/Additive"));
            line.SetColors(Color.green, Color.blue);
            line.SetVertexCount(2);
            ClearDockAssistLine();
            this.drivingTargetVessel = null;
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            if (!this.showLine) { ClearDockAssistLine(); }
            else
            {
                line.SetWidth(0.01f, 0.01f);
                line.SetPosition(0, this.cam.CrossPos);
                line.SetPosition(1, this.target.Pos);
            }

            if (this.engaged)
            {
                if (this.driveTarget)
                {
                    if (this.drivingTargetVessel != this.target.vessel) //Vessel change detected
                    {
                        if (this.drivingTargetVessel != null)
                        {
                            HydroFlightInputManager.RemoveOnFlyByWire(drivingTargetVessel, nameString_Target, DriveTargetAutopilot);
                        }
                        AddDriveTarget();
                    }
                }
                else if (drivingTargetVessel != null) // ActiveVessel docked to TargetVessel
                {
                    RemoveDriveTarget();
                }
            }

            if (!NullTarget()) { this.jebsTargetVessel.OnUpdate(); }
        }

        protected override void LoadDefault()
        {
            base.LoadDefault();
            this.autoOrient = Default.BOOL.AutoOrient;
            this.camView = Default.BOOL.CamView;
            this._driveTarget = Default.BOOL.DriveTarget;
            this.killRelV = Default.BOOL.KillRelV;
            this.manual = Default.BOOL.Manual;
            this.showLine = Default.BOOL.ShowLine;
            this.finalStageSpeed = Default.FLOAT.FinalStageSpeed;
            this.angularAcc = Default.FLOAT.AngularAcc;
            this.acc = Default.FLOAT.Acc;
            this.cam = Default.MISC.Cam;
            this.target = Default.MISC.Target;
        }
        #endregion
    }
}
using HydroTech.Autopilots.Calculators;
using HydroTech.Data;
using HydroTech.Managers;
using HydroTech.Panels;
using HydroTech.Utils;
using UnityEngine;

namespace HydroTech.Autopilots
{
    public class APDockAssist : Autopilot
    {
        #region Constants
        private static readonly Vector3 finalStagePos = new Vector3(0, 0, 15);
        private const float finalStageErr = 0.05f;
        private const float vel0 = 1;
        private const float safeSpeed = 0.5f;
        private const float stopSpeed = 0.05f;
        #endregion

        #region Static properties
        private static string NameStringTarget => "DockAP.Target";
        #endregion

        #region Fields
        private readonly RCSCalculator rcsTarget = new RCSCalculator();
        private Vessel drivingTargetVessel;
        public ModuleDockAssistTarget target;
        private bool targetOrientReady = true;
        private LineRenderer line;
        public SubList<HydroJebCore> jebsTargetVessel;
        public float angularAcc = 0.5f;
        public float acc = 0.5f;
        public float finalStageSpeed = 0.4f;
        #endregion

        #region Properties
        private bool showLine = true;
        public bool ShowLine
        {
            get { return !this.NullCamera && !this.NullTarget && this.showLine; }
            set { this.showLine = value; }
        }

        private bool autoOrient;
        public bool AutoOrient
        {
            get { return !this.NullCamera && !this.NullTarget && this.autoOrient; }
            set { this.autoOrient = value; }
        }

        private bool killRelV;
        public bool KillRelV
        {
            get { return !this.NullCamera && !this.NullTarget && this.killRelV; }
            set { this.killRelV = value; }
        }

        private bool camView;
        public bool CamView
        {
            get { return !this.NullCamera && this.camView; }
            set
            {
                if (!this.NullCamera && this.Engaged && !this.CameraPaused) { this.Cam.CamActive = value; }
                this.camView = value;
            }
        }

        private bool manual = true;
        public bool Manual
        {
            get { return this.NullCamera || this.NullTarget || this.manual; }
            set
            {
                if (this.manual != value && this.Engaged && this.DriveTarget)
                {
                    if (value) { RemoveDriveTarget(); }
                    else { AddDriveTarget(); }
                }
                this.manual = value;
            }
        }

        private bool cameraPaused;
        public bool CameraPaused
        {
            get { return this.Engaged && this.CamView && this.cameraPaused; }
            set
            {
                if (this.Engaged && this.CamView)
                {
                    if (value)
                    {
                        HydroFlightManager.Instance.CameraManager.SaveCurrent();
                        HydroFlightManager.Instance.CameraManager.ResetToActiveVessel();
                    }
                    else { HydroFlightManager.Instance.CameraManager.RetrieveLast(); }
                }
                this.cameraPaused = value;
            }
        }

        public bool driveTarget;
        public bool DriveTarget
        {
            get { return !this.Manual && this.TargetHasJeb && this.driveTarget; }
            set
            {
                if (!this.Active || this.Manual || !this.TargetHasJeb) { return; }
                if (value && !this.driveTarget) { this.drivingTargetVessel = this.target.vessel; }
                if (this.Engaged)
                {
                    if (value)
                    {
                        if (!this.driveTarget) { AddDriveTarget(); }
                    }
                    else if (this.driveTarget) { RemoveDriveTarget(); }
                }
                this.driveTarget = value;
            }
        }

        private ModuleDockAssistCam cam;
        public ModuleDockAssistCam Cam
        {
            get { return this.cam; }
            set
            {
                if (this.cam != null && value != this.cam) { this.cam.CamActive = false; }
                this.cam = value;
            }
        }

        public bool NullCamera => this.Cam == null || !this.Cam.IsOnActiveVessel;

        public bool NullTarget => this.target == null || !this.target.IsNear;

        public bool TargetHasJeb => this.jebsTargetVessel.Count != 0;

        public Vector3 RelV => this.Cam.VectorTransform(this.target.vessel.obt_velocity - this.Cam.vessel.obt_velocity);

        protected override string NameString => "DockAP.Active";

        public override bool Engaged
        {
            set
            {
                if (!this.Active) { return; }
                if (this.DriveTarget)
                {
                    if (value) { AddDriveTarget(); }
                    else { RemoveDriveTarget(); }
                }
                if (value)
                {
                    if (this.CamView && !this.CameraPaused) { this.Cam.CamActive = true; }
                }
                else
                {
                    FlightMainPanel.Instance.DockAssist.ResetHeight();
                    if (!this.NullCamera) { this.Cam.CamActive = false; }
                }
                base.Engaged = value;
            }
        }

        public override bool Active
        {
            set
            {
                if (value != base.Active && this.Engaged && !this.Manual && this.driveTarget) //TargetHasJeb() may return false
                {
                    if (value) { AddDriveTarget(); }
                    else { RemoveDriveTarget(); }
                }
                base.Active = value;
            }
        }
        #endregion

        #region Constructor
        public APDockAssist()
        {
            this.jebsTargetVessel = new SubList<HydroJebCore>(HydroFlightManager.Instance.Targets, IsJebTargetVessel);
        }
        #endregion

        #region Methods
        private void DriveAutoOrient(FlightCtrlState ctrlState)
        {
            DockingAssistCalculator stateCal = new DockingAssistCalculator();
            stateCal.Calculate(this.Cam, this.target);
            stateCal.SetCtrlStateRotation(ctrlState);
        }

        private void DriveTargetAutopilot(FlightCtrlState ctrlState)
        {
            this.target.vessel.SetState(KSPActionGroup.RCS, true);
            this.target.vessel.SetState(KSPActionGroup.SAS, false);
            Vector3 dir = (this.target.Pos - this.Cam.Pos).normalized;
            HoldDirectionCalculator stateCal = new HoldDirectionCalculator();
            stateCal.Calculate(dir, Vector3.zero, this.target.Dir, this.target.Right, this.target.vessel);
            stateCal.SetCtrlStateRotation(ctrlState);
            this.targetOrientReady = stateCal.Steer(0.05f); //2.87°
            this.rcsTarget.OnUpdate(this.target.vessel);
            this.rcsTarget.MakeRotation(ctrlState, this.angularAcc);
        }

        private void DriveKillRelV(FlightCtrlState ctrlState)
        {
            Vector3 relVCam = this.RelV;
            ctrlState.X = -relVCam.x / vel0;
            ctrlState.Y = -relVCam.y / vel0;
            ctrlState.Z = -relVCam.z / vel0;
            Vector3 translationRate = new Vector3(ActiveRCS.GetThrustRateFromAcc6(ctrlState.X >= 0 ? 0 : 1, this.acc), ActiveRCS.GetThrustRateFromAcc6(ctrlState.Y >= 0 ? 2 : 3, this.acc), ActiveRCS.GetThrustRateFromAcc6(ctrlState.Z >= 0 ? 4 : 5, this.acc));
            ctrlState.X /= translationRate.x;
            ctrlState.Y /= translationRate.y;
            ctrlState.Z /= translationRate.z;
            if (Mathf.Abs(ctrlState.X) > 1) { ctrlState.X = ctrlState.X > 0 ? 1 : -1; }
            if (Mathf.Abs(ctrlState.Y) > 1) { ctrlState.Y = ctrlState.Y > 0 ? 1 : -1; }
            if (Mathf.Abs(ctrlState.Z) > 1) { ctrlState.Z = ctrlState.Z > 0 ? 1 : -1; }
        }

        private void DriveFinalStage(FlightCtrlState ctrlState, Vector3 relPTarget, Vector3 relVCam)
        {
            if (relVCam.z > stopSpeed || relVCam.z < -this.finalStageSpeed * 1.1f) { DriveKillRelV(ctrlState); }
            else
            {
                ctrlState.X = -relPTarget.x / finalStageErr;
                ctrlState.Y = -relPTarget.y / finalStageErr;
                if (relVCam.x * relPTarget.x < 0) { ctrlState.X /= 2; }
                if (relVCam.y * relPTarget.y < 0) { ctrlState.Y /= 2; }
                if (relVCam.z < -this.finalStageSpeed) { ctrlState.Z = 0; }
                else
                {
                    ctrlState.Z = -1;
                }
            }
        }

        private void DriveAutoDocking(FlightCtrlState ctrlState)
        {
            DockingAssistCalculator stateCal = new DockingAssistCalculator();
            stateCal.Calculate(this.Cam, this.target);
            DriveAutoOrient(ctrlState);
            if (!stateCal.Steer(0.05f /*2.87°*/)) { DriveKillRelV(ctrlState); }
            else //HoldErr
            {
                Vector3 relPTarget = new Vector3(stateCal.x, stateCal.y, stateCal.z);
                Vector2 relPTargetXy = new Vector2(stateCal.x, stateCal.y);
                Vector3 relVCam = this.RelV;
                Vector2 relVCamXy = new Vector2(relVCam.x, relVCam.y);
                if (relPTargetXy.magnitude < finalStageErr && stateCal.z < finalStagePos.z + finalStageErr && stateCal.z > 0) { DriveFinalStage(ctrlState, relPTarget, relVCam); }
                else if (relVCam.magnitude > 0.7f) { DriveKillRelV(ctrlState); }
                else //< MaxSpeed
                {
                    if (stateCal.z < 10)
                    {
                        if (relPTargetXy.magnitude < 20)
                        {
                            if (Mathf.Abs(relVCam.z) > stopSpeed) //Still moving in Z
                            {
                                DriveKillRelV(ctrlState);
                            }
                            else
                            {
                                if (Vector3.Dot(relPTargetXy.normalized, relVCamXy.normalized) < 0.95f /*18.19°*/ && relVCamXy.magnitude > stopSpeed) //Moving inwards
                                {
                                    DriveKillRelV(ctrlState);
                                }
                                else if (relVCamXy.magnitude > safeSpeed)
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
                            if (relVCamXy.magnitude > stopSpeed) //Moving in XY
                            {
                                DriveKillRelV(ctrlState);
                            }
                            else
                            {
                                ctrlState.X = 0;
                                ctrlState.Y = 0;
                                ctrlState.Z = relVCam.z > safeSpeed ? 0 : 1;
                            }
                        }
                    }
                    else // >= MinZ
                    {
                        Vector3 diff = (finalStagePos - relPTarget).normalized;
                        if (Vector3.Dot(relVCam.normalized, diff) < 0.95f /*18.19°*/ && relVCam.magnitude > stopSpeed) { DriveKillRelV(ctrlState); }
                        else if (relVCam.magnitude > safeSpeed)
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
            HTUtils.CamToVesselTrans(ctrlState, this.Cam);
        }

        private bool IsJebTargetVessel(HydroJebCore jeb)
        {
            return !this.NullTarget && jeb.vessel == this.target.vessel;
        }

        private void AddDriveTarget()
        {
            HydroFlightManager.Instance.InputManager.AddOnFlyByWire(this.target.vessel, NameStringTarget, DriveTargetAutopilot);
            this.drivingTargetVessel = this.target.vessel;
        }

        private void RemoveDriveTarget()
        {
            HydroFlightManager.Instance.InputManager.RemoveOnFlyByWire(this.target.vessel, NameStringTarget, DriveTargetAutopilot);
            this.drivingTargetVessel = null;
        }

        private void ClearDockAssistLine()
        {
            this.line.SetWidth(0, 0);
            this.line.SetPosition(0, Vector3.zero);
            this.line.SetPosition(1, Vector3.zero);
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
                this.line.SetPosition(0, this.Cam.Pos);
                this.line.SetPosition(1, this.target.Pos);
            }

            if (this.Engaged)
            {
                if (this.DriveTarget)
                {
                    if (this.drivingTargetVessel != this.target.vessel) //Vessel change detected
                    {
                        if (this.drivingTargetVessel != null) { HydroFlightManager.Instance.InputManager.RemoveOnFlyByWire(this.drivingTargetVessel, NameStringTarget, DriveTargetAutopilot); }
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

        protected override void DriveAutopilot(FlightCtrlState ctrlState)
        {
            base.DriveAutopilot(ctrlState);
            if (this.Manual)
            {
                if (this.AutoOrient)
                {
                    FlightGlobals.ActiveVessel.SetState(KSPActionGroup.SAS, false);
                    DriveAutoOrient(ctrlState);
                }
                if (this.CamView && !this.AutoOrient)
                {
                    FlightGlobals.ActiveVessel.SetState(KSPActionGroup.SAS, false);
                    HTUtils.CamToVesselRot(ctrlState, this.Cam);
                }
                bool killingRelV = this.KillRelV && ctrlState.X == 0 && ctrlState.Y == 0 && ctrlState.Z == 0;
                if (killingRelV)
                {
                    DriveKillRelV(ctrlState);
                    HTUtils.CamToVesselTrans(ctrlState, this.Cam);
                }
                else if (this.CamView) { HTUtils.CamToVesselTrans(ctrlState, this.Cam); }
            }
            else //!Manual
            {
                FlightGlobals.ActiveVessel.SetState(KSPActionGroup.SAS, false);
                if (this.DriveTarget)
                {
                    if (this.targetOrientReady)
                    {
                        Vector3 r = this.target.Pos - this.Cam.Pos;
                        HoldDirectionCalculator stateCal = new HoldDirectionCalculator();
                        stateCal.Calculate(r.normalized, this.target.Right, this.Cam.Dir, this.Cam.Right, ActiveVessel);
                        stateCal.SetCtrlStateRotation(ctrlState);
                        bool orientReady = stateCal.Steer(0.05f); //2.87°
                        if (orientReady && ActiveVessel.GetComponent<Rigidbody>().angularVelocity.magnitude < 0.01f)
                        {
                            DriveFinalStage(ctrlState, SwitchTransformCalculator.VectorTransform(r, this.target.Right, this.target.Down, this.target.Dir), this.RelV);
                            HTUtils.CamToVesselTrans(ctrlState, this.Cam);
                        }
                        else
                        {
                            DriveKillRelV(ctrlState);
                            HTUtils.CamToVesselTrans(ctrlState, this.Cam);
                        }
                    }
                    else
                    {
                        DriveKillRelV(ctrlState);
                        HTUtils.CamToVesselTrans(ctrlState, this.Cam);
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
            ActiveRCS.MakeRotation(ctrlState, this.angularAcc);
            ActiveRCS.MakeTranslation(ctrlState, this.acc);
        }
        #endregion
    }
}
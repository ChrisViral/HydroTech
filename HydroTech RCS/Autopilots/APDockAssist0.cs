#if DEBUG
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HydroTech_RCS.Autopilots
{
    using UnityEngine;
    using HydroTech_FC;
    using ASAS;
    using Modules;
    using Constants.Core;
    using Constants.Autopilots.Docking;
    using Panels;

    public partial class APDockAssist : RCSAutopilot
    {
        static public APDockAssist theAutopilot { get { return (APDockAssist)HydroJebCore.autopilots[AutopilotIDs.Dock]; } }
        static protected Panel panel { get { return PanelDockAssist.thePanel; } }

        public APDockAssist()
        {
            fileName = new FileName("dock", "cfg", HydroJebCore.AutopilotSaveFolder);
            jebsTargetVessel = new SubList<Part>(HydroJebCore.jebs.listInactiveVessel, isJebTargetVessel);
        }

        protected override string nameString { get { return Str.nameString; } }

        #region public variables for user input

        #region bool

        [HydroSLNodeInfo(name = "SETTINGS")]
        [HydroSLField(saveName = "ShowLine")]
        public bool _ShowLine = Default.BOOL.ShowLine;
        public bool ShowLine
        {
            get
            {
                if (NullCamera() || NullTarget())
                    return false;
                else
                    return _ShowLine;
            }
            set { _ShowLine = value; }
        }

        [HydroSLNodeInfo(name = "SETTINGS")]
        [HydroSLField(saveName = "AutoOrient")]
        public bool _AutoOrient = Default.BOOL.AutoOrient;
        public bool AutoOrient
        {
            get
            {
                if (NullCamera() || NullTarget())
                    return false;
                else
                    return _AutoOrient;
            }
            set { _AutoOrient = value; }
        }

        [HydroSLNodeInfo(name = "SETTINGS")]
        [HydroSLField(saveName = "KillRelV")]
        public bool _KillRelV = Default.BOOL.KillRelV;
        public bool KillRelV
        {
            get
            {
                if (NullCamera() || NullTarget())
                    return false;
                else
                    return _KillRelV;
            }
            set { _KillRelV = value; }
        }

        [HydroSLNodeInfo(name = "SETTINGS")]
        [HydroSLField(saveName = "CamView")]
        public bool _CamView = Default.BOOL.CamView;
        public bool CamView
        {
            get
            {
                if (NullCamera())
                    return false;
                else
                    return _CamView;
            }
            set
            {
                if (!NullCamera() && Engaged && !CameraPaused)
                    Cam.CamActivate = value;
                _CamView = value;
            }
        }

        [HydroSLNodeInfo(name = "SETTINGS")]
        [HydroSLField(saveName = "Manual")]
        public bool _Manual = Default.BOOL.Manual;
        public bool Manual
        {
            get
            {
                if (NullCamera() || NullTarget())
                    return true;
                else
                    return _Manual;
            }
            set
            {
                if (_Manual != value)
                {
                    if (Engaged && DriveTarget)
                    {
                        if (value)
                            RemoveDriveTarget();
                        else
                            AddDriveTarget();
                    }
                }
                _Manual = value;
            }
        }

        [HydroSLNodeInfo(name = "SETTINGS")]
        [HydroSLField(saveName = "DriveTarget")]
        public bool _DriveTarget = Default.BOOL.DriveTarget;
        public bool DriveTarget
        {
            get
            {
                if (Manual || !TargetHasJeb())
                    return false;
                else
                    return _DriveTarget;
            }
            set
            {
                if (!Active || Manual || !TargetHasJeb())
                    return;
                if (value && !_DriveTarget)
                    drivingTargetVessel = Target.vessel;
                if (Engaged)
                {
                    if (value && !_DriveTarget)
                        AddDriveTarget();
                    else if (!value && _DriveTarget)
                        RemoveDriveTarget();
                }
                _DriveTarget = value;
            }
        }

        #endregion

        #region float

        [HydroSLNodeInfo(name = "SETTINGS")]
        [HydroSLField(saveName = "AngularAcc")]
        public float AngularAcc = Default.FLOAT.AngularAcc;

        [HydroSLNodeInfo(name = "SETTINGS")]
        [HydroSLField(saveName = "Acc")]
        public float Acc = Default.FLOAT.Acc;

        [HydroSLNodeInfo(name = "SETTINGS")]
        [HydroSLField(saveName = "FinalSpeed")]
        public float FinalStageSpeed = Default.FLOAT.FinalStageSpeed;

        #endregion

        #region misc

        protected ModuleDockAssistCam _Cam = Default.MISC.Cam;
        public ModuleDockAssistCam Cam
        {
            get { return _Cam; }
            set
            {
                if (value != _Cam && _Cam != null)
                    _Cam.CamActivate = false;
                _Cam = value;
            }
        }
        public ModuleDockAssistTarget Target = Default.MISC.Target;

        #endregion

        #region override

        public override bool Engaged
        {
            set
            {
                if (!Active)
                    return;
                if (DriveTarget)
                {
                    if (value)
                        AddDriveTarget();
                    else
                        RemoveDriveTarget();
                }
                if (value)
                {
                    if (CamView && !CameraPaused)
                        Cam.CamActivate = true;
                }
                else
                {
                    panel.ResetHeight();
                    if (!NullCamera())
                        Cam.CamActivate = false;
                }
                base.Engaged = value;
            }
        }

        #endregion

        #endregion

        #region variables for autopilot

        #region bool

        protected bool _CameraPaused = false;
        public bool CameraPaused
        {
            get
            {
                if (!Engaged || !CamView)
                    return false;
                else
                    return _CameraPaused;
            }
            set
            {
                if (Engaged && CamView)
                {
                    if (value)
                    {
                        HydroFlightCameraManager.SaveCurrent();
                        HydroFlightCameraManager.ResetToActiveVessel();
                    }
                    else
                        HydroFlightCameraManager.RetrieveLast();
                }
                _CameraPaused = value;
            }
        }
        protected bool TargetOrientReady = true;

        #endregion

        #region misc

        protected LineRenderer line = null;
        public SubList<Part> jebsTargetVessel = null;

        #endregion

        #region override

        public override bool Active
        {
            set
            {
                if (value != base.Active && Engaged && !Manual && _DriveTarget) // TargetHasJeb() may return false
                {
                    if (value)
                        AddDriveTarget();
                    else
                        RemoveDriveTarget();
                }
                base.Active = value;
            }
        }

        #endregion

        #endregion

        public bool NullCamera() { return Cam == null || !Cam.IsOnActiveVessel(); }
        public bool NullTarget() { return Target == null || !Target.IsNear(); }

        protected bool isJebTargetVessel(Part jeb)
        {
            if (NullTarget())
                return false;
            return jeb.vessel == Target.vessel;
        }
        public bool TargetHasJeb() { return jebsTargetVessel.Count != 0; }

        protected CalculatorRCSThrust RCSTarget = new CalculatorRCSThrust();

        virtual protected String nameString_Target { get { return Str.nameString_Target; } }
        protected Vessel drivingTargetVessel = null;
        protected void AddDriveTarget()
        {
            HydroFlightInputManager.AddOnFlyByWire(Target.vessel, nameString_Target, DriveTargetAutopilot);
            drivingTargetVessel = Target.vessel;
        }
        protected void RemoveDriveTarget()
        {
            HydroFlightInputManager.RemoveOnFlyByWire(Target.vessel, nameString_Target, DriveTargetAutopilot);
            drivingTargetVessel = null;
        }

        static public void CamToVessel_Rot(FlightCtrlState ctrlState, ModuleDockAssistCam mcam)
        {
            SwitchTransformCalculator sCal = new SwitchTransformCalculator();
            sCal.GetRotation(ctrlState);
            sCal.ChangeTransformRotation(mcam.Right, mcam.Down, mcam.Dir, mcam.VesselTransform);
            sCal.SetRotation(ctrlState);
        }
        static public void CamToVessel_Trans(FlightCtrlState ctrlState, ModuleDockAssistCam mcam)
        {
            SwitchTransformCalculator sCal = new SwitchTransformCalculator();
            sCal.GetTranslation(ctrlState);
            sCal.ChangeTransformTranslation(mcam.Right, mcam.Down, mcam.Dir, mcam.VesselTransform);
            sCal.SetTranslation(ctrlState);
        }

        protected void ClearDockAssistLine()
        {
            line.SetWidth(0.0F, 0.0F);
            line.SetPosition(0, Vector3.zero);
            line.SetPosition(1, Vector3.zero);
        }

        public override void onFlightStart()
        {
            base.onFlightStart();

            // Drawing line learned from Kethane (there was a pump line before 0.18)
            GameObject obj = new GameObject("Docking Assist Line");
            line = obj.AddComponent<LineRenderer>();

            line.useWorldSpace = false;
            line.material = new Material(Shader.Find("Particles/Additive"));
            line.SetColors(Color.green, Color.blue);
            line.SetVertexCount(2);

            ClearDockAssistLine();

            drivingTargetVessel = null;
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            if (!ShowLine)
                ClearDockAssistLine();
            else
            {
                line.SetWidth(0.01F, 0.01F);
                line.SetPosition(0, Cam.CrossPos);
                line.SetPosition(1, Target.Pos);
            }
            if (Engaged)
            {
                if (DriveTarget)
                {
                    if (drivingTargetVessel != Target.vessel) // Vessel change detected
                    {
                        if (drivingTargetVessel != null)
                            HydroFlightInputManager.RemoveOnFlyByWire(drivingTargetVessel, nameString_Target, DriveTargetAutopilot);
                        AddDriveTarget();
                    }
                }
                else if (drivingTargetVessel != null) // ActiveVessel docked to TargetVessel
                    RemoveDriveTarget();
            }

            if (!NullTarget())
                jebsTargetVessel.OnUpdate();
        }

        protected override void LoadDefault()
        {
            base.LoadDefault();
            AutoOrient = Default.BOOL.AutoOrient;
            CamView = Default.BOOL.CamView;
            _DriveTarget = Default.BOOL.DriveTarget;
            KillRelV = Default.BOOL.KillRelV;
            Manual = Default.BOOL.Manual;
            ShowLine = Default.BOOL.ShowLine;
            FinalStageSpeed = Default.FLOAT.FinalStageSpeed;
            AngularAcc = Default.FLOAT.AngularAcc;
            Acc = Default.FLOAT.Acc;
            Cam = Default.MISC.Cam;
            Target = Default.MISC.Target;
        }
    }
}
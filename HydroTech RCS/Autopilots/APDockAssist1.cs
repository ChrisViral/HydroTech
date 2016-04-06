using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HydroTech_RCS.Autopilots
{
    using UnityEngine;
    using HydroTech_FC;
    using ASAS;
    using Constants.Autopilots.Docking;

    public partial class APDockAssist
    {
        virtual protected void DriveAutoOrient(FlightCtrlState ctrlState)
        {
            DockAssistStateCalculator stateCal = new DockAssistStateCalculator();
            stateCal.Calculate(Cam, Target);
            stateCal.SetCtrlStateRotation(ctrlState);
        }

        virtual protected void DriveTargetAutopilot(FlightCtrlState ctrlState)
        {
            TurnOnRCS(Target.vessel);
            TurnOffSAS(Target.vessel);
            Vector3 dir = (Target.Pos - Cam.Pos).normalized;
            HoldDirStateCalculator stateCal = new HoldDirStateCalculator();
            stateCal.Calculate(
                dir,
                Vector3.zero,
                Target.Dir,
                Target.Right,
                Target.vessel
                );
            stateCal.SetCtrlStateRotation(ctrlState);
            TargetOrientReady = stateCal.Steer(Angle.TranslationReadyAngleSin);
            RCSTarget.OnUpdate(Target.vessel);
            RCSTarget.MakeRotation(ctrlState, AngularAcc);
        }

        protected Vector3 RelV()
        {
            Vector3 RelV_Obt = Target.vessel.obt_velocity - Cam.vessel.obt_velocity;
            return Cam.VectorTransform(RelV_Obt);
        }
        virtual protected void DriveKillRelV(FlightCtrlState ctrlState)
        {
            Vector3 RelV_Cam = RelV();
            ctrlState.X = -RelV_Cam.x / Velocity.Vel0;
            ctrlState.Y = -RelV_Cam.y / Velocity.Vel0;
            ctrlState.Z = -RelV_Cam.z / Velocity.Vel0;
            Vector3 translationRate = new Vector3(
                RCSActive.GetThrustRateFromAcc6(ctrlState.X >= 0 ? 0 : 1, Acc),
                RCSActive.GetThrustRateFromAcc6(ctrlState.Y >= 0 ? 2 : 3, Acc),
                RCSActive.GetThrustRateFromAcc6(ctrlState.Z >= 0 ? 4 : 5, Acc)
                );
            ctrlState.X /= translationRate.x;
            ctrlState.Y /= translationRate.y;
            ctrlState.Z /= translationRate.z;
            if (HMaths.Abs(ctrlState.X) > 1)
                ctrlState.X = ctrlState.X > 0 ? 1 : -1;
            if (HMaths.Abs(ctrlState.Y) > 1)
                ctrlState.Y = ctrlState.Y > 0 ? 1 : -1;
            if (HMaths.Abs(ctrlState.Z) > 1)
                ctrlState.Z = ctrlState.Z > 0 ? 1 : -1;
        }

        virtual protected void DriveFinalStage(FlightCtrlState ctrlState, Vector3 RelP_Target, Vector3 RelV_Cam)
        {
            if (RelV_Cam.z > Velocity.StopSpeed
                || RelV_Cam.z < -FinalStageSpeed * Velocity.FinalStageSpeedMaxMultiplier)
                DriveKillRelV(ctrlState);
            else
            {
                ctrlState.X = -RelP_Target.x / Position.FinalStageErr;
                ctrlState.Y = -RelP_Target.y / Position.FinalStageErr;
                if (RelV_Cam.x * RelP_Target.x < 0)
                    ctrlState.X /= 2;
                if (RelV_Cam.y * RelP_Target.y < 0)
                    ctrlState.Y /= 2;
                if (RelV_Cam.z < -FinalStageSpeed)
                    ctrlState.Z = 0;
                else
                    ctrlState.Z = -1.0F;
            }
        }

        virtual protected void DriveAutoDocking(FlightCtrlState ctrlState)
        {
            DockAssistStateCalculator stateCal = new DockAssistStateCalculator();
            stateCal.Calculate(Cam, Target);

            DriveAutoOrient(ctrlState);
            //RCSActive.MakeRotation(ctrlState, AngularAcc);

            if (!stateCal.Steer(Angle.TranslationReadyAngleSin))
                DriveKillRelV(ctrlState);
            else // HoldErr
            {
                Vector3 RelP_Target = new Vector3(stateCal.X, stateCal.Y, stateCal.Z);
                Vector2 RelP_Target_XY = new Vector2(stateCal.X, stateCal.Y);
                Vector3 RelV_Cam = RelV();
                Vector2 RelV_Cam_XY = new Vector2(RelV_Cam.x, RelV_Cam.y);
                if (RelP_Target_XY.magnitude < Position.FinalStageErr
                    && stateCal.Z < Position.FinalStagePos.z + Position.FinalStageErr && stateCal.Z > 0)
                    DriveFinalStage(ctrlState, RelP_Target, RelV_Cam);
                else if (RelV_Cam.magnitude > Velocity.MaxSpeed)
                    DriveKillRelV(ctrlState);
                else // < MaxSpeed
                {
                    if (stateCal.Z < Position.MinZ)
                    {
                        if (RelP_Target_XY.magnitude < Position.MinXY)
                        {
                            if (HMaths.Abs(RelV_Cam.z) > Velocity.StopSpeed) // Still moving in Z
                                DriveKillRelV(ctrlState);
                            else
                            {
                                if (HMaths.DotProduct(RelP_Target_XY.normalized, RelV_Cam_XY.normalized)
                                        < Angle.MaxTranslationErrAngleCos
                                    && RelV_Cam_XY.magnitude > Velocity.StopSpeed) // Moving inwards
                                    DriveKillRelV(ctrlState);
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
                        else // >= MinXY
                        {
                            if (RelV_Cam_XY.magnitude > Velocity.StopSpeed) // Moving in XY
                                DriveKillRelV(ctrlState);
                            else
                            {
                                ctrlState.X = 0;
                                ctrlState.Y = 0;
                                if (RelV_Cam.z > Velocity.SafeSpeed)
                                    ctrlState.Z = 0;
                                else
                                    ctrlState.Z = 1;
                            }
                        }
                    }
                    else // >= MinZ
                    {
                        Vector3 diff = (Position.FinalStagePos - RelP_Target).normalized;
                        if (HMaths.DotProduct(RelV_Cam.normalized, diff) < Angle.MaxTranslationErrAngleCos
                            && RelV_Cam.magnitude > Velocity.StopSpeed)
                            DriveKillRelV(ctrlState);
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
            CamToVessel_Trans(ctrlState, Cam);
        }

        protected override void DriveAutopilot(FlightCtrlState ctrlState)
        {
            base.DriveAutopilot(ctrlState);
            if (Manual)
            {
                if (AutoOrient)
                {
                    HydroActionGroupManager.ActiveVessel.SAS = false;
                    DriveAutoOrient(ctrlState);
                }
                if (CamView && !AutoOrient)
                {
                    HydroActionGroupManager.ActiveVessel.SAS = false;
                    CamToVessel_Rot(ctrlState, Cam);
                }
                bool KillingRelV = KillRelV && ctrlState.X == 0 && ctrlState.Y == 0 && ctrlState.Z == 0;
                if (KillingRelV)
                    DriveKillRelV(ctrlState);
                if (KillingRelV || CamView)
                    CamToVessel_Trans(ctrlState, Cam);
            }
            else // !Manual
            {
                HydroActionGroupManager.ActiveVessel.SAS = false;
                if (DriveTarget)
                {
                    if (TargetOrientReady)
                    {
                        Vector3 r = Target.Pos - Cam.Pos;
                        HoldDirStateCalculator stateCal = new HoldDirStateCalculator();
                        stateCal.Calculate(
                            r.normalized,
                            Target.Right,
                            Cam.Dir,
                            Cam.Right,
                            ActiveVessel
                            );
                        stateCal.SetCtrlStateRotation(ctrlState);
                        bool OrientReady = stateCal.Steer(Angle.TranslationReadyAngleSin);
                        if (OrientReady && ActiveVessel.rigidbody.angularVelocity.magnitude < AngularV.MaxAngularV)
                        {
                            DriveFinalStage(
                                ctrlState,
                                VectorTransform(r, Target.Right, Target.Down, Target.Dir),
                                RelV()
                                );
                            CamToVessel_Trans(ctrlState, Cam);
                        }
                        else
                        {
                            DriveKillRelV(ctrlState);
                            CamToVessel_Trans(ctrlState, Cam);
                        }
                    }
                    else
                    {
                        DriveKillRelV(ctrlState);
                        CamToVessel_Trans(ctrlState, Cam);
                        ctrlState.yaw = 0;
                        ctrlState.roll = 0;
                        ctrlState.pitch = 0;
                    }
                }
                else
                    DriveAutoDocking(ctrlState);
            }
            RCSActive.MakeRotation(ctrlState, AngularAcc);
            RCSActive.MakeTranslation(ctrlState, Acc);
        }
    }
}
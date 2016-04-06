using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HydroTech_RCS.Autopilots
{
    using UnityEngine;
    using HydroTech_FC;
    using ASAS;
    using Constants.Core;
    using Constants.Autopilots.Landing;

    public partial class APLanding
    {
        protected bool DriveHoldDir(FlightCtrlState ctrlState, Vector3 dir)
        {
            // Point up
            LandingStateCalculator stateCal = new LandingStateCalculator();
            stateCal.Calculate(
                VABPod,
                dir,
                Vector3d.zero,
                ActiveVessel
                );
            stateCal.SetCtrlStateRotation(ctrlState);

            if (stateCal.Steer(Angle.TranslationReadyAngleSin))
                RCSActive.MakeRotation(ctrlState, AngularAcc.MaxAngularAcc_Hold);
            else
                RCSActive.MakeRotation(ctrlState, AngularAcc.MaxAngularAcc_Steer);

            // Kill rotation
            Vector3 angularVelocity = VectorTransform(
                ActiveVessel.rigidbody.angularVelocity,
                ActiveVessel.ReferenceTransform
                );
            SetRotationRoll(ctrlState, Rotation.KillRotThrustRate * angularVelocity.z);

            if (!stateCal.Steer(Angle.TranslationReadyAngleSin))
                return false;
            else
                return true;
        }

        protected void DeployLandingGears()
        {
            /* ActiveVessel.rootPart.SendEvent(EventStrings.LowerLandingLeg);
            ActiveVessel.rootPart.SendEvent(EventStrings.LowerLandingGear); */
            HydroActionGroupManager.ActiveVessel.Gear = true;
        }

        protected void CheckAltitudeAndDeployLandingGears()
        {
            if (HoriSpeed < Velocity.SafeHorizontalSpeed
                && AltTrue <= Position.DeployGearHeight)
                DeployLandingGears();
        }

        protected override void DriveAutopilot(FlightCtrlState ctrlState)
        {
            base.DriveAutopilot(ctrlState);

            HydroActionGroupManager.ActiveVessel.SAS = false;

            if (touchdown || status != Status.HOVER)
                RemoveUserInput(ctrlState);

            if (Engines && burnRetro)
            {
                Vector3 decVector = new Vector3();
                switch (status)
                {
                    case Status.IDLE: decVector = SurfVel.normalized; break;
                    case Status.LANDED: decVector = -SurfUpNormal; break;
                    case Status.HOVER: decVector = -SurfUpNormal; break;
                    case Status.DESCEND: decVector = -SurfUpNormal; break;
                    case Status.AVOID: decVector = -SurfUpNormal; break;
                    case Status.HORIZONTAL: decVector = new Vector3(SurfVel.x, SurfVel.y, 0).normalized; break;
                    case Status.VERTICAL: decVector = -SurfUpNormal; break;
                    case Status.DECELERATE: decVector = SurfVel.normalized; break;
                    default: decVector = Vector3.zero; break;
                }

                if (!DriveHoldDir(ctrlState, -decVector))
                    return;

                CheckAltitudeAndDeployLandingGears();

                DriveHorizontalDec(ctrlState);
                switch (status)
                {
                    case Status.IDLE: SetTranslationZ(ctrlState, 0); return;
                    case Status.LANDED: SetTranslationZ(ctrlState, 0); return;
                    case Status.HOVER: DriveHoverManeuver(ctrlState); return;
                    case Status.DESCEND: DriveFinalDescent(ctrlState); return;
                    case Status.AVOID: DriveAvoidContact(ctrlState); return;
                    case Status.HORIZONTAL: SetTranslationZ(ctrlState, -1); return;
                    case Status.VERTICAL: SetTranslationZ(ctrlState, -1); return;
                    case Status.DECELERATE:
                        SetTranslationZ(ctrlState, cd.ThrRate / SurfVel.z * SurfVel.magnitude);
                        return;
                    default: return;
                }
            }
            else
            {
                if (!Engines)
                    FlightInputHandler.state.mainThrottle = 0;

                if (!DriveHoldDir(ctrlState, SurfUpNormal))
                    return;

                CheckAltitudeAndDeployLandingGears();

                switch (status)
                {
                    case Status.IDLE: SetTranslationZ(ctrlState, 0); return;
                    case Status.LANDED: SetTranslationZ(ctrlState, 0); return;
                    case Status.HOVER: DriveHoverManeuver(ctrlState); return;
                    case Status.DESCEND: DriveFinalDescent(ctrlState); return;
                    case Status.AVOID: DriveAvoidContact(ctrlState); return;
                    case Status.HORIZONTAL:
                        DriveHorizontalBrake(ctrlState);
                        SetTranslationZ(ctrlState, 0);
                        return;
                    case Status.VERTICAL: SetTranslationZ(ctrlState, -1.0F); return;
                    case Status.DECELERATE:
                        DriveHorizontalDec(ctrlState);
                        SetTranslationZ(ctrlState, -cd.ThrRate);
                        return;
                    default: return;
                }
            }
        }

        protected void DriveHorizontalDec(FlightCtrlState ctrlState)
        {
            ctrlState.X = HMaths.Cut(
                HydroJebCore.activeVesselRCS.GetThrustRateFromAcc3(0, SurfXSpeed * Acceleration.MaxDeceleration),
                -1.0F, 1.0F
                );
            SetTranslationY(ctrlState, HMaths.Cut(
                HydroJebCore.activeVesselRCS.GetThrustRateFromAcc3(VABPod ? 1 : 2, SurfYSpeed * Acceleration.MaxDeceleration),
                -1.0F, 1.0F
                ));
        }

        protected void DriveHorizontalBrake(FlightCtrlState ctrlState)
        {
            ctrlState.X = HMaths.Sign(SurfXSpeed);
            SetTranslationY(ctrlState, HMaths.Sign(SurfYSpeed));
        }

        protected void DriveFinalDescent(FlightCtrlState ctrlState)
        {
            DriveHorizontalDec(ctrlState);
            SetTranslationZ(
                ctrlState,
                HMaths.Cut(-HoverThrustRate + (safeTouchDownSpeed + VertSpeed) / TWR, -1.0F, 0.0F)
                );
        }

        protected void DriveAvoidContact(FlightCtrlState ctrlState)
        {
            if (VertSpeed < 0.0F)
                SetTranslationZ(ctrlState, -1.0F);
            else
            {
                SetTranslationZ(ctrlState, -HoverThrustRate);
                DriveHorizontalDec(ctrlState);
            }
        }

        protected void DriveHoverManeuver(FlightCtrlState ctrlState)
        {
            float modifier = HMaths.Max(AltDiff, -10) + VertSpeed;
            SetTranslationZ(
                ctrlState,
                HMaths.Cut(-HoverThrustRate + modifier / TWR, -1.0F, 0.0F)
                );
            if (HoriSpeed > SafeHorizontalSpeed)
                DriveHorizontalDec(ctrlState);
        }

        protected void SetTranslationY(FlightCtrlState ctrlState, float Y)
        {
            if (VABPod)
                ctrlState.Y = Y;
            else
                ctrlState.Z = Y;
        }
        protected void SetTranslationZ(FlightCtrlState ctrlState, float Z)
        {
            if (VABPod)
                ctrlState.Z = Z;
            else
                ctrlState.Y = -Z;
            if (_Engines)
            {
                ctrlState.mainThrottle = -Z * maxThrottle;
                FlightInputHandler.state.mainThrottle = ctrlState.mainThrottle;
            }
        }
        protected void SetRotationRoll(FlightCtrlState ctrlState, float roll)
        {
            if (VABPod)
                ctrlState.roll = roll;
            else
                ctrlState.yaw = roll;
        }
    }
}
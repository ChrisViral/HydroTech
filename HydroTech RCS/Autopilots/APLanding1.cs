using HydroTech_FC;
using HydroTech_RCS.Autopilots.ASAS;
using HydroTech_RCS.Constants.Autopilots.Landing;
using UnityEngine;

namespace HydroTech_RCS.Autopilots
{
    public partial class APLanding
    {
        protected bool DriveHoldDir(FlightCtrlState ctrlState, Vector3 dir)
        {
            // Point up
            LandingStateCalculator stateCal = new LandingStateCalculator();
            stateCal.Calculate(this.vabPod, dir, Vector3d.zero, ActiveVessel);
            stateCal.SetCtrlStateRotation(ctrlState);

            if (stateCal.Steer(Angle.translationReadyAngleSin)) { RcsActive.MakeRotation(ctrlState, AngularAcc.maxAngularAccHold); }
            else
            { RcsActive.MakeRotation(ctrlState, AngularAcc.maxAngularAccSteer); }

            // Kill rotation
            Vector3 angularVelocity = VectorTransform(ActiveVessel.rigidbody.angularVelocity, ActiveVessel.ReferenceTransform);
            SetRotationRoll(ctrlState, Rotation.killRotThrustRate * angularVelocity.z);

            if (!stateCal.Steer(Angle.translationReadyAngleSin)) { return false; }
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
            if ((this.HoriSpeed < Velocity.safeHorizontalSpeed) && (this.AltTrue <= Position.deployGearHeight)) { DeployLandingGears(); }
        }

        protected override void DriveAutopilot(FlightCtrlState ctrlState)
        {
            base.DriveAutopilot(ctrlState);

            HydroActionGroupManager.ActiveVessel.SAS = false;

            if (this.touchdown || (this.status != Status.HOVER)) { RemoveUserInput(ctrlState); }

            if (this.Engines && this.burnRetro)
            {
                Vector3 decVector = new Vector3();
                switch (this.status)
                {
                    case Status.IDLE:
                        decVector = this.SurfVel.normalized;
                        break;
                    case Status.LANDED:
                        decVector = -this.surfUpNormal;
                        break;
                    case Status.HOVER:
                        decVector = -this.surfUpNormal;
                        break;
                    case Status.DESCEND:
                        decVector = -this.surfUpNormal;
                        break;
                    case Status.AVOID:
                        decVector = -this.surfUpNormal;
                        break;
                    case Status.HORIZONTAL:
                        decVector = new Vector3(this.SurfVel.x, this.SurfVel.y, 0).normalized;
                        break;
                    case Status.VERTICAL:
                        decVector = -this.surfUpNormal;
                        break;
                    case Status.DECELERATE:
                        decVector = this.SurfVel.normalized;
                        break;
                    default:
                        decVector = Vector3.zero;
                        break;
                }

                if (!DriveHoldDir(ctrlState, -decVector)) { return; }

                CheckAltitudeAndDeployLandingGears();

                DriveHorizontalDec(ctrlState);
                switch (this.status)
                {
                    case Status.IDLE:
                        SetTranslationZ(ctrlState, 0);
                        return;
                    case Status.LANDED:
                        SetTranslationZ(ctrlState, 0);
                        return;
                    case Status.HOVER:
                        DriveHoverManeuver(ctrlState);
                        return;
                    case Status.DESCEND:
                        DriveFinalDescent(ctrlState);
                        return;
                    case Status.AVOID:
                        DriveAvoidContact(ctrlState);
                        return;
                    case Status.HORIZONTAL:
                        SetTranslationZ(ctrlState, -1);
                        return;
                    case Status.VERTICAL:
                        SetTranslationZ(ctrlState, -1);
                        return;
                    case Status.DECELERATE:
                        SetTranslationZ(ctrlState, (this.cd.ThrRate / this.SurfVel.z) * this.SurfVel.magnitude);
                        return;
                    default:
                        return;
                }
            }
            if (!this.Engines) { FlightInputHandler.state.mainThrottle = 0; }

            if (!DriveHoldDir(ctrlState, this.surfUpNormal)) { return; }

            CheckAltitudeAndDeployLandingGears();

            switch (this.status)
            {
                case Status.IDLE:
                    SetTranslationZ(ctrlState, 0);
                    return;
                case Status.LANDED:
                    SetTranslationZ(ctrlState, 0);
                    return;
                case Status.HOVER:
                    DriveHoverManeuver(ctrlState);
                    return;
                case Status.DESCEND:
                    DriveFinalDescent(ctrlState);
                    return;
                case Status.AVOID:
                    DriveAvoidContact(ctrlState);
                    return;
                case Status.HORIZONTAL:
                    DriveHorizontalBrake(ctrlState);
                    SetTranslationZ(ctrlState, 0);
                    return;
                case Status.VERTICAL:
                    SetTranslationZ(ctrlState, -1.0F);
                    return;
                case Status.DECELERATE:
                    DriveHorizontalDec(ctrlState);
                    SetTranslationZ(ctrlState, -this.cd.ThrRate);
                    return;
                default:
                    return;
            }
        }

        protected void DriveHorizontalDec(FlightCtrlState ctrlState)
        {
            ctrlState.X = HMaths.Cut(HydroJebCore.activeVesselRcs.GetThrustRateFromAcc3(0, this.SurfXSpeed * Acceleration.maxDeceleration), -1.0F, 1.0F);
            SetTranslationY(ctrlState, HMaths.Cut(HydroJebCore.activeVesselRcs.GetThrustRateFromAcc3(this.vabPod ? 1 : 2, this.SurfYSpeed * Acceleration.maxDeceleration), -1.0F, 1.0F));
        }

        protected void DriveHorizontalBrake(FlightCtrlState ctrlState)
        {
            ctrlState.X = HMaths.Sign(this.SurfXSpeed);
            SetTranslationY(ctrlState, HMaths.Sign(this.SurfYSpeed));
        }

        protected void DriveFinalDescent(FlightCtrlState ctrlState)
        {
            DriveHorizontalDec(ctrlState);
            SetTranslationZ(ctrlState, HMaths.Cut(-this.hoverThrustRate + ((this.safeTouchDownSpeed + this.VertSpeed) / this.Twr), -1.0F, 0.0F));
        }

        protected void DriveAvoidContact(FlightCtrlState ctrlState)
        {
            if (this.VertSpeed < 0.0F) { SetTranslationZ(ctrlState, -1.0F); }
            else
            {
                SetTranslationZ(ctrlState, -this.hoverThrustRate);
                DriveHorizontalDec(ctrlState);
            }
        }

        protected void DriveHoverManeuver(FlightCtrlState ctrlState)
        {
            float modifier = HMaths.Max(this.AltDiff, -10) + this.VertSpeed;
            SetTranslationZ(ctrlState, HMaths.Cut(-this.hoverThrustRate + (modifier / this.Twr), -1.0F, 0.0F));
            if (this.HoriSpeed > this.SafeHorizontalSpeed) { DriveHorizontalDec(ctrlState); }
        }

        protected void SetTranslationY(FlightCtrlState ctrlState, float y)
        {
            if (this.vabPod) { ctrlState.Y = y; }
            else
            { ctrlState.Z = y; }
        }

        protected void SetTranslationZ(FlightCtrlState ctrlState, float z)
        {
            if (this.vabPod) { ctrlState.Z = z; }
            else
            { ctrlState.Y = -z; }
            if (this.engines)
            {
                ctrlState.mainThrottle = -z * this.maxThrottle;
                FlightInputHandler.state.mainThrottle = ctrlState.mainThrottle;
            }
        }

        protected void SetRotationRoll(FlightCtrlState ctrlState, float roll)
        {
            if (this.vabPod) { ctrlState.roll = roll; }
            else
            { ctrlState.yaw = roll; }
        }
    }
}
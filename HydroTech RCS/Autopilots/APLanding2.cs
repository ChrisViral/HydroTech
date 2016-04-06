using System;
using HydroTech_FC;
using HydroTech_RCS.Autopilots.Modules;
using HydroTech_RCS.Constants.Autopilots.Landing;
using UnityEngine;

namespace HydroTech_RCS.Autopilots
{
    public partial class APLanding
    {
        protected string GetStatusString(Status st)
        {
            switch (st)
            {
                case Status.DISENGAGED:
                    return Str.Status.disengaged;
                case Status.IDLE:
                    return Str.Status.idle;
                case Status.DECELERATE:
                    return Str.Status.decelerate;
                case Status.DESCEND:
                    return Str.Status.descend;
                case Status.VERTICAL:
                    return Str.Status.vertical;
                case Status.HORIZONTAL:
                    return Str.Status.horizontal;
                case Status.WARP:
                    return Str.Status.warp;
                case Status.AVOID:
                    return Str.Status.avoid;
                case Status.LANDED:
                    return Str.Status.landed;
                case Status.HOVER:
                    return Str.Status.FLOAT;
                default:
                    return "NULL";
            }
        }

        protected string GetWarningString(Indicator i)
        {
            switch (i)
            {
                case Indicator.LANDED:
                    return Str.Warning.landed;
                case Indicator.WARP:
                    return Str.Warning.warp;
                case Indicator.SAFE:
                    return Str.Warning.safe;
                case Indicator.OK:
                    return Str.Warning.ok;
                case Indicator.DANGER:
                    return Str.Warning.danger;
                case Indicator.LOWTWR:
                    return Str.Warning.lowtwr;
                case Indicator.OUTSYNC:
                    return Str.Warning.outsync;
                case Indicator.FINAL:
                    return Str.Warning.final;
                case Indicator.HOVER:
                    return Str.Warning.FLOAT;
                default:
                    return "NULL";
            }
        }

        protected Color GetWarningStringColor(Indicator i)
        {
            switch (i)
            {
                case Indicator.LANDED:
                    return Color.blue;
                case Indicator.WARP:
                    return Color.cyan;
                case Indicator.SAFE:
                    return Color.green;
                case Indicator.OK:
                    return Color.yellow;
                case Indicator.DANGER:
                    return Color.red;
                case Indicator.LOWTWR:
                    return Color.red;
                case Indicator.OUTSYNC:
                    return Color.red;
                case Indicator.FINAL:
                    return Color.yellow;
                case Indicator.HOVER:
                    return Color.yellow;
                default:
                    return Color.white;
            }
        }

        protected Indicator GetIndicator(CalculatorDescent.Indicator i)
        {
            switch (i)
            {
                case CalculatorDescent.Indicator.WARP:
                    return Indicator.WARP;
                case CalculatorDescent.Indicator.SAFE:
                    return Indicator.SAFE;
                case CalculatorDescent.Indicator.OK:
                    return Indicator.OK;
                case CalculatorDescent.Indicator.DANGER:
                    return Indicator.DANGER;
                default:
                    return Indicator.LANDED;
            }
        }

        protected float GetBodySyncAltitude(CelestialBody body)
        {
            return (float)HMaths.CubeRoot((body.gravParameter * body.rotationPeriod * body.rotationPeriod) / (4 * HMaths.PI * HMaths.PI));
        }

        protected float HoriBrakeSpeed(float alt)
        {
            return 0.05F * alt;
        }

        public float AllowedHoriSpeed(float alt)
        {
            return 0.1F * alt;
        }

        protected float VesselHeight()
        {
            float height = 0.0F;
            foreach (Part p in ActiveVessel.parts)
            {
                Vector3 r = p.Rigidbody.worldCenterOfMass - ActiveVessel.CoM;
                float h = -HMaths.DotProduct(r, this.Up);
                height = HMaths.Max(height, h);
            }
            return height;
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            // Calculate vessel states
            Vector3 coM = ActiveVessel.CoM;
            this.surfUpNormal = (coM - this.MainBody.position).normalized;
            this.g = (float)FlightGlobals.getGeeForceAtPosition(coM).magnitude;
            this.GAsl = (float)FlightGlobals.getGeeForceAtPosition(this.MainBody.position + ((Vector3d)this.surfUpNormal * this.MainBody.Radius)).magnitude;
            this.tad.OnUpdate(ActiveVessel, VesselHeight(), this.SlopeDetection);

            // Get vessel TWR
            this.TwrRcs = this.vabPod ? HydroJebCore.activeVesselRcs.maxAcc.zn : HydroJebCore.activeVesselRcs.maxAcc.yp;
            CalculatorEngineThrust cet = new CalculatorEngineThrust();
            cet.OnUpdate(ActiveVessel, this.vabPod ? Vector3.down : Vector3.forward);
            this.TwrEng = cet.MaxAcc * this.maxThrottle;
            this.hoverThrustRate = this.g / this.Twr;

            // Get vessel behaviour
            if (this.Landed && this.touchdown) { this.indicator = Indicator.LANDED; }
            else if ((this.hoverThrustRate > 1.0F) || (this.GAsl > this.Twr))
            {
                this.indicator = Indicator.LOWTWR;
                this.engaged = false;
            }
            else if (this.AltAsl > this.MainBodySyncAlt)
            {
                this.indicator = Indicator.OUTSYNC;
                this.engaged = false;
            }
            else if (this.AltDiff < Position.finalDescentHeight) { this.indicator = this.touchdown ? Indicator.FINAL : Indicator.HOVER; }
            else // ready for landing
            {
                this.cd.OnUpdate(Position.finalDescentHeight, this.safeTouchDownSpeed, this.GAsl, this.Twr, -this.VertSpeed, this.AltDiff);
                this.indicator = GetIndicator(this.cd.indicator);
            }

            if (!this.engaged) { this.status = Status.DISENGAGED; }
            else if ((TimeWarp.WarpMode == TimeWarp.Modes.HIGH) && (TimeWarp.CurrentRateIndex != 0)) { this.status = Status.WARP; }
            else
            {
                if (this.indicator == Indicator.LANDED) { this.status = Status.LANDED; }
                else if (this.indicator == Indicator.HOVER) { this.status = Status.HOVER; }
                else if (this.indicator == Indicator.FINAL)
                {
                    if (this.HoriSpeed < this.SafeHorizontalSpeed) { this.status = Status.DESCEND; }
                    else
                    { this.status = Status.AVOID; }
                }
                else if (this.cd.behaviour == CalculatorDescent.Behaviour.IDLE)
                {
                    if (this.HoriSpeed > HoriBrakeSpeed(this.AltTrue)) { this.status = Status.HORIZONTAL; }
                    else if (this.HoriSpeed < this.SafeHorizontalSpeed) { this.status = Status.IDLE; }
                    else
                    { this.status = Status.DECELERATE; }
                }
                else if (this.cd.behaviour == CalculatorDescent.Behaviour.BRAKE) { this.status = Status.VERTICAL; }
                else // CalculatorDescent.BEHAVIOUR.NORMAL
                {
                    if (this.HoriSpeed > HoriBrakeSpeed(this.AltTrue)) { this.status = Status.HORIZONTAL; }
                    else
                    { this.status = Status.DECELERATE; }
                }
            }
        }

        #region variables for autopilot

        #region bool
        #endregion

        #region float
        protected float hoverThrustRate;
        protected float vertThrustRate;
        protected float twrRcs;

        public float TwrRcs
        {
            get { return this.twrRcs; }
            protected set { this.twrRcs = value; }
        }

        protected float twrEng;

        public float TwrEng
        {
            get { return this.twrEng; }
            protected set { this.twrEng = value; }
        }

        public float Twr
        {
            get { return this.engines ? this.TwrRcs + this.TwrEng : this.TwrRcs; }
        }

        protected float g;
        protected float _gAsl;

        public float GAsl
        {
            get { return this._gAsl; }
            protected set { this._gAsl = value; }
        }
        #endregion

        #region misc
        protected Vector3 surfUpNormal;
        protected Status status = Status.DISENGAGED;
        protected Indicator indicator = Indicator.SAFE;
        protected TrueAltitudeDetector tad = new TrueAltitudeDetector();
        protected CalculatorDescent cd = new CalculatorDescent();
        #endregion

        #endregion

        #region get

        #region bool
        public bool Landed
        {
            get { return ActiveVessel.LandedOrSplashed; }
        }

        public bool SlopeDetection
        {
            get { return this.AltAsl <= Position.startSlopeDetectionHeight; }
        }
        #endregion

        #region float
        public float AltAsl
        {
            get { return (float)this.MainBody.GetAltitude(ActiveVessel.CoM); }
        }

        public float AltTrue
        {
            get { return this.tad.DistCenter; }
        }

        public float TerrainHeight
        {
            get { return this.AltAsl - this.AltTrue; }
        }

        public float AltKeepTrue
        {
            get { return this.useTrueAlt ? this.altKeep : HMaths.Max(this.altKeep - this.TerrainHeight, Position.finalDescentHeight); }
        }

        public float AltKeepAsl
        {
            get { return this.useTrueAlt ? this.altKeep + this.TerrainHeight : this.altKeep; }
        }

        public float AltDiff
        {
            get { return this.touchdown ? this.AltTrue : this.AltTrue - this.AltKeepTrue; }
        }

        public bool Terrain
        {
            get { return this.tad.terrain; }
        }

        public float DetectRadius
        {
            get { return this.tad.Radius; }
        }

        public float Slope(DetectorGroundContact.Direction dir)
        {
            return this.tad.Slope(dir);
        }

        public float VertSpeed
        {
            get { return (float)ActiveVessel.verticalSpeed; }
        }

        public float HoriSpeed
        {
            get { return (float)ActiveVessel.horizontalSrfSpeed; }
        }

        protected float SurfXSpeed
        {
            get { return this.SurfVelVessel.x; }
        }

        protected float SurfYSpeed
        {
            get { return this.vabPod ? this.SurfVelVessel.y : this.SurfVelVessel.z; }
        }

        public float MainBodySyncAlt
        {
            get { return GetBodySyncAltitude(this.MainBody); }
        }

        public float AllowedHori
        {
            get { return AllowedHoriSpeed(this.AltKeepTrue); }
        }

        public float SafeHorizontalSpeed
        {
            get { return this.touchdown ? Velocity.safeHorizontalSpeed : AllowedHoriSpeed(this.AltTrue); }
        }
        #endregion

        #region misc
        protected Vector3 Up
        {
            get { return this.vabPod ? ActiveVessel.ReferenceTransform.up : -ActiveVessel.ReferenceTransform.forward; }
        }

        protected Vector3 SurfVel
        {
            get { return ActiveVessel.srf_velocity; }
        }

        protected Vector3 SurfVelVessel
        {
            get { return VectorTransform(this.SurfVel, ActiveVessel.ReferenceTransform); }
        }

        protected CelestialBody MainBody
        {
            get { return ActiveVessel.mainBody; }
        }

        public string MainBodyName
        {
            get { return this.MainBody.name; }
        }

        public string StatusString
        {
            get { return GetStatusString(this.status); }
        }

        public string WarningString
        {
            get { return GetWarningString(this.indicator); }
        }

        public Color WarningColor
        {
            get { return GetWarningStringColor(this.indicator); }
        }
        #endregion

        #endregion
    }
}
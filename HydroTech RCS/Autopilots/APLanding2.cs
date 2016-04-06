using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HydroTech_RCS.Autopilots
{
    using UnityEngine;
    using HydroTech_FC;
    using Modules;
    using Constants.Autopilots.Landing;

    public partial class APLanding
    {
        #region variables for autopilot

        #region bool
        #endregion

        #region float

        protected float HoverThrustRate;
        protected float VertThrustRate;
        protected float _TWR_RCS;
        public float TWR_RCS
        {
            get { return _TWR_RCS; }
            protected set { _TWR_RCS = value; }
        }
        protected float _TWR_Eng;
        public float TWR_Eng
        {
            get { return _TWR_Eng; }
            protected set { _TWR_Eng = value; }
        }
        public float TWR { get { return _Engines ? (TWR_RCS + TWR_Eng) : TWR_RCS; } }
        protected float g;
        protected float _gASL;
        public float gASL
        {
            get { return _gASL; }
            protected set { _gASL = value; }
        }

        #endregion

        #region misc

        protected Vector3 SurfUpNormal;
        protected Status status = Status.DISENGAGED;
        protected Indicator indicator = Indicator.SAFE;
        protected TrueAltitudeDetector tad = new TrueAltitudeDetector();
        protected CalculatorDescent cd = new CalculatorDescent();

        #endregion

        #endregion

        #region get

        #region bool

        public bool Landed { get { return ActiveVessel.LandedOrSplashed; } }
        public bool SlopeDetection { get { return AltASL <= Position.StartSlopeDetectionHeight; } }

        #endregion

        #region float

        public float AltASL { get { return (float)MainBody.GetAltitude(ActiveVessel.CoM); } }
        public float AltTrue { get { return tad.DistCenter; } }
        public float TerrainHeight { get { return AltASL - AltTrue; } }
        public float AltKeepTrue { get { return useTrueAlt ? altKeep : HMaths.Max(altKeep - TerrainHeight, Position.FinalDescentHeight); } }
        public float AltKeepASL { get { return useTrueAlt ? altKeep + TerrainHeight : altKeep; } }
        public float AltDiff { get { return touchdown ? AltTrue : AltTrue - AltKeepTrue; } }
        public bool Terrain { get { return tad.terrain; } }
        public float DetectRadius { get { return tad.Radius; } }
        public float Slope(DetectorGroundContact.DIRECTION dir) { return tad.Slope(dir); }
        public float VertSpeed { get { return (float)ActiveVessel.verticalSpeed; } }
        public float HoriSpeed { get { return (float)(ActiveVessel.horizontalSrfSpeed); } }
        protected float SurfXSpeed { get { return SurfVel_Vessel.x; } }
        protected float SurfYSpeed { get { return VABPod ? SurfVel_Vessel.y : SurfVel_Vessel.z; } }
        public float MainBodySyncAlt { get { return GetBodySyncAltitude(MainBody); } }
        public float AllowedHori { get { return AllowedHoriSpeed(AltKeepTrue); } }
        public float SafeHorizontalSpeed { get { return touchdown ? Velocity.SafeHorizontalSpeed : AllowedHoriSpeed(AltTrue); } }

        #endregion

        #region misc

        protected Vector3 Up { get { return VABPod ? ActiveVessel.ReferenceTransform.up : -ActiveVessel.ReferenceTransform.forward; } }
        protected Vector3 SurfVel { get { return ActiveVessel.srf_velocity; } }
        protected Vector3 SurfVel_Vessel { get { return VectorTransform(SurfVel, ActiveVessel.ReferenceTransform); } }
        protected CelestialBody MainBody { get { return ActiveVessel.mainBody; } }
        public String MainBodyName { get { return MainBody.name; } }
        public String StatusString { get { return GetStatusString(status); } }
        public String WarningString { get { return GetWarningString(indicator); } }
        public Color WarningColor { get { return GetWarningStringColor(indicator); } }

        #endregion

        #endregion

        protected String GetStatusString(Status st)
        {
            switch (st)
            {
                case Status.DISENGAGED: return Str.Status.DISENGAGED;
                case Status.IDLE: return Str.Status.IDLE;
                case Status.DECELERATE: return Str.Status.DECELERATE;
                case Status.DESCEND: return Str.Status.DESCEND;
                case Status.VERTICAL: return Str.Status.VERTICAL;
                case Status.HORIZONTAL: return Str.Status.HORIZONTAL;
                case Status.WARP: return Str.Status.WARP;
                case Status.AVOID: return Str.Status.AVOID;
                case Status.LANDED: return Str.Status.LANDED;
                case Status.HOVER: return Str.Status.FLOAT;
                default: return "NULL";
            }
        }
        protected String GetWarningString(Indicator i)
        {
            switch (i)
            {
                case Indicator.LANDED: return Str.Warning.LANDED;
                case Indicator.WARP: return Str.Warning.WARP;
                case Indicator.SAFE: return Str.Warning.SAFE;
                case Indicator.OK: return Str.Warning.OK;
                case Indicator.DANGER: return Str.Warning.DANGER;
                case Indicator.LOWTWR: return Str.Warning.LOWTWR;
                case Indicator.OUTSYNC: return Str.Warning.OUTSYNC;
                case Indicator.FINAL: return Str.Warning.FINAL;
                case Indicator.HOVER: return Str.Warning.FLOAT;
                default: return "NULL";
            }
        }
        protected Color GetWarningStringColor(Indicator i)
        {
            switch (i)
            {
                case Indicator.LANDED: return Color.blue;
                case Indicator.WARP: return Color.cyan;
                case Indicator.SAFE: return Color.green;
                case Indicator.OK: return Color.yellow;
                case Indicator.DANGER: return Color.red;
                case Indicator.LOWTWR: return Color.red;
                case Indicator.OUTSYNC: return Color.red;
                case Indicator.FINAL: return Color.yellow;
                case Indicator.HOVER: return Color.yellow;
                default: return Color.white;
            }
        }

        protected Indicator GetIndicator(CalculatorDescent.INDICATOR i)
        {
            switch (i)
            {
                case CalculatorDescent.INDICATOR.WARP: return Indicator.WARP;
                case CalculatorDescent.INDICATOR.SAFE: return Indicator.SAFE;
                case CalculatorDescent.INDICATOR.OK: return Indicator.OK;
                case CalculatorDescent.INDICATOR.DANGER: return Indicator.DANGER;
                default: return Indicator.LANDED;
            }
        }

        protected float GetBodySyncAltitude(CelestialBody body)
        {
            return (float)HMaths.CubeRoot(
                body.gravParameter * body.rotationPeriod * body.rotationPeriod
                / (4 * HMaths.PI * HMaths.PI)
                );
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
            float Height = 0.0F;
            foreach (Part p in ActiveVessel.parts)
            {
                Vector3 r = p.Rigidbody.worldCenterOfMass - ActiveVessel.CoM;
                float h = -HMaths.DotProduct(r, Up);
                Height = HMaths.Max(Height, h);
            }
            return Height;
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            // Calculate vessel states
            Vector3 CoM = ActiveVessel.CoM;
            SurfUpNormal = (CoM - MainBody.position).normalized;
            g = (float)FlightGlobals.getGeeForceAtPosition(CoM).magnitude;
            gASL = (float)FlightGlobals.getGeeForceAtPosition(
                MainBody.position + (Vector3d)SurfUpNormal * MainBody.Radius
                ).magnitude;
            tad.OnUpdate(ActiveVessel, VesselHeight(), SlopeDetection);

            // Get vessel TWR
            TWR_RCS = VABPod ? HydroJebCore.activeVesselRCS.MaxAcc.zn : HydroJebCore.activeVesselRCS.MaxAcc.yp;
            CalculatorEngineThrust cet = new CalculatorEngineThrust();
            cet.OnUpdate(ActiveVessel, VABPod ? Vector3.down : Vector3.forward);
            TWR_Eng = cet.MaxAcc * maxThrottle;
            HoverThrustRate = g / TWR;

            // Get vessel behaviour
            if (Landed && touchdown)
                indicator = Indicator.LANDED;
            else if (HoverThrustRate > 1.0F || gASL > TWR)
            {
                indicator = Indicator.LOWTWR;
                Engaged = false;
            }
            else if (AltASL > MainBodySyncAlt)
            {
                indicator = Indicator.OUTSYNC;
                Engaged = false;
            }
            else if (AltDiff < Position.FinalDescentHeight)
                indicator = touchdown ? Indicator.FINAL : Indicator.HOVER;
            else // ready for landing
            {
                cd.OnUpdate(Position.FinalDescentHeight, safeTouchDownSpeed, gASL, TWR, -VertSpeed, AltDiff);
                indicator = GetIndicator(cd.indicator);
            }

            if (!Engaged)
                status = Status.DISENGAGED;
            else if (TimeWarp.WarpMode == TimeWarp.Modes.HIGH && TimeWarp.CurrentRateIndex != 0)
                status = Status.WARP;
            else
            {
                if (indicator == Indicator.LANDED)
                    status = Status.LANDED;
                else if (indicator == Indicator.HOVER)
                    status = Status.HOVER;
                else if (indicator == Indicator.FINAL)
                {
                    if (HoriSpeed < SafeHorizontalSpeed)
                        status = Status.DESCEND;
                    else
                        status = Status.AVOID;
                }
                else if (cd.behaviour == CalculatorDescent.BEHAVIOUR.IDLE)
                {
                    if (HoriSpeed > HoriBrakeSpeed(AltTrue))
                        status = Status.HORIZONTAL;
                    else if (HoriSpeed < SafeHorizontalSpeed)
                        status = Status.IDLE;
                    else
                        status = Status.DECELERATE;
                }
                else if (cd.behaviour == CalculatorDescent.BEHAVIOUR.BRAKE)
                    status = Status.VERTICAL;
                else // CalculatorDescent.BEHAVIOUR.NORMAL
                {
                    if (HoriSpeed > HoriBrakeSpeed(AltTrue))
                        status = Status.HORIZONTAL;
                    else
                        status = Status.DECELERATE;
                }
            }
        }
    }
}
using HydroTech_FC;
using HydroTech_RCS.Autopilots.Calculators;
using HydroTech_RCS.Constants;
using HydroTech_RCS.Constants.Core;
using HydroTech_RCS.Panels;
using UnityEngine;

namespace HydroTech_RCS.Autopilots
{
    public class APLanding : RCSAutopilot
    {
        public enum Indicator
        {
            LANDED,
            SAFE,
            WARP,
            OK,
            DANGER,
            LOWTWR,
            OUTSYNC,
            FINAL,
            HOVER
        }

        public enum Status
        {
            DISENGAGED,
            IDLE,
            HORIZONTAL,
            VERTICAL,
            DECELERATE,
            DESCEND,
            AVOID,
            WARP,
            LANDED,
            HOVER
        }

        #region Properties
        public static APLanding TheAutopilot
        {
            get { return (APLanding)HydroJebCore.autopilots[AutopilotIDs.landing]; }
        }

        public static PanelLanding Panel
        {
            get { return PanelLanding.ThePanel; }
        }

        protected override string NameString
        {
            get { return AutopilotConsts.landingName; }
        }

        public bool Landed
        {
            get { return ActiveVessel.LandedOrSplashed; }
        }

        public bool SlopeDetection
        {
            get { return this.AltAsl <= AutopilotConsts.startSlopeDetectionHeight; }
        }

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
            get { return this.useTrueAlt ? this.altKeep : HMaths.Max(this.altKeep - this.TerrainHeight, AutopilotConsts.finalDescentHeight); }
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

        public float Slope(GroundContactCalculator.Direction dir)
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
            get { return this.touchdown ? AutopilotConsts.safeHorizontalSpeed : AllowedHoriSpeed(this.AltTrue); }
        }

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

        #region User input vars     
        [HydroSLNodeInfo(name = "SETTINGS"), HydroSLField(saveName = "UseEngines")]
        public bool engines = AutopilotConsts.engine;
        public bool Engines
        {
            get { return this.engines; }
            set
            {
                if (value != this.engines) { Panel.ResetHeight(); }
                this.engines = value;
            }
        }

        [HydroSLNodeInfo(name = "SETTINGS"), HydroSLField(saveName = "PointUp")]
        public bool vabPod = AutopilotConsts.vabPod;

        [HydroSLNodeInfo(name = "SETTIINGS"), HydroSLField(saveName = "BurnRetro", isTesting = true)]
        public bool burnRetro = AutopilotConsts.burnRetro;

        [HydroSLNodeInfo(name = "SETTINGS"), HydroSLField(saveName = "Touchdown")]
        public bool touchdown = AutopilotConsts.touchdown;

        [HydroSLNodeInfo(name = "SETTINGS"), HydroSLField(saveName = "TrueAlt")]
        public bool useTrueAlt = AutopilotConsts.useTrueAlt;

        [HydroSLNodeInfo(name = "SETTINGS"), HydroSLField(saveName = "TouchdownSpeed")]
        public float safeTouchDownSpeed = AutopilotConsts.safeTouchDownSpeed;

        [HydroSLNodeInfo(name = "SETTINGS"), HydroSLField(saveName = "MaxThrottle")]
        public float maxThrottle = AutopilotConsts.maxThrottle;

        [HydroSLNodeInfo(name = "SETTINGS"), HydroSLField(saveName = "Altitude")]
        public float altKeep = AutopilotConsts.altKeep;

        public float MaxThrottle
        {
            get { return this.maxThrottle * 100; }
            set { this.maxThrottle = value / 100; }
        }

        public override bool Engaged
        {
            set
            {
                if (value != this.engaged) { Panel.ResetHeight(); }
                base.Engaged = value;
            }
        }
        #endregion

        #region Autopilot vars 
        protected float hoverThrustRate;
        protected float vertThrustRate;
        protected float g;
        protected Vector3 surfUpNormal;
        protected Status status = Status.DISENGAGED;
        protected Indicator indicator = Indicator.SAFE;
        protected GroundContactCalculator tad = new GroundContactCalculator();
        protected DescentCalculator cd = new DescentCalculator();

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

        protected float gAsl;
        public float GAsl
        {
            get { return this.gAsl; }
            protected set { this.gAsl = value; }
        }
        #endregion

        #region Constructor
        public APLanding()
        {
            this.fileName = new FileName("landing", "cfg", HydroJebCore.autopilotSaveFolder);
        }
        #endregion

        #region Autopilot
        protected bool DriveHoldDir(FlightCtrlState ctrlState, Vector3 dir)
        {
            // Point up
            LandingCalculator stateCal = new LandingCalculator();
            stateCal.Calculate(this.vabPod, dir, Vector3d.zero, ActiveVessel);
            stateCal.SetCtrlStateRotation(ctrlState);
            RcsActive.MakeRotation(ctrlState, stateCal.Steer(AutopilotConsts.translationReadyAngleSin) ? AutopilotConsts.maxAngularAccHold : AutopilotConsts.maxAngularAccSteer);

            // Kill rotation
            Vector3 angularVelocity = VectorTransform(ActiveVessel.GetComponent<Rigidbody>().angularVelocity, ActiveVessel.ReferenceTransform);
            SetRotationRoll(ctrlState, AutopilotConsts.killRotThrustRate * angularVelocity.z);

            return stateCal.Steer(AutopilotConsts.translationReadyAngleSin);
        }

        protected void DeployLandingGears()
        {
            HydroActionGroupManager.ActiveVessel.Gear = true;
        }

        protected void CheckAltitudeAndDeployLandingGears()
        {
            if (this.HoriSpeed < AutopilotConsts.safeHorizontalSpeed && this.AltTrue <= AutopilotConsts.deployGearHeight) { DeployLandingGears(); }
        }

        protected override void DriveAutopilot(FlightCtrlState ctrlState)
        {
            base.DriveAutopilot(ctrlState);

            HydroActionGroupManager.ActiveVessel.SAS = false;

            if (this.touchdown || this.status != Status.HOVER) { RemoveUserInput(ctrlState); }

            if (this.Engines && this.burnRetro)
            {
                Vector3 decVector;
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
            ctrlState.X = HMaths.Cut(HydroJebCore.activeVesselRcs.GetThrustRateFromAcc3(0, this.SurfXSpeed * AutopilotConsts.maxDeceleration), -1, 1);
            SetTranslationY(ctrlState, HMaths.Cut(HydroJebCore.activeVesselRcs.GetThrustRateFromAcc3(this.vabPod ? 1 : 2, this.SurfYSpeed * AutopilotConsts.maxDeceleration), -1, 1));
        }

        protected void DriveHorizontalBrake(FlightCtrlState ctrlState)
        {
            ctrlState.X = HMaths.Sign(this.SurfXSpeed);
            SetTranslationY(ctrlState, HMaths.Sign(this.SurfYSpeed));
        }

        protected void DriveFinalDescent(FlightCtrlState ctrlState)
        {
            DriveHorizontalDec(ctrlState);
            SetTranslationZ(ctrlState, HMaths.Cut(-this.hoverThrustRate + ((this.safeTouchDownSpeed + this.VertSpeed) / this.Twr), -1, 0));
        }

        protected void DriveAvoidContact(FlightCtrlState ctrlState)
        {
            if (this.VertSpeed < 0) { SetTranslationZ(ctrlState, -1); }
            else
            {
                SetTranslationZ(ctrlState, -this.hoverThrustRate);
                DriveHorizontalDec(ctrlState);
            }
        }

        protected void DriveHoverManeuver(FlightCtrlState ctrlState)
        {
            float modifier = HMaths.Max(this.AltDiff, -10) + this.VertSpeed;
            SetTranslationZ(ctrlState, HMaths.Cut(-this.hoverThrustRate + (modifier / this.Twr), -1, 0));
            if (this.HoriSpeed > this.SafeHorizontalSpeed) { DriveHorizontalDec(ctrlState); }
        }

        protected void SetTranslationY(FlightCtrlState ctrlState, float y)
        {
            if (this.vabPod) { ctrlState.Y = y; }
            else
            {
                ctrlState.Z = y;
            }
        }

        protected void SetTranslationZ(FlightCtrlState ctrlState, float z)
        {
            if (this.vabPod) { ctrlState.Z = z; }
            else
            {
                ctrlState.Y = -z;
            }
            if (this.engines)
            {
                ctrlState.mainThrottle = -z * this.maxThrottle;
                FlightInputHandler.state.mainThrottle = ctrlState.mainThrottle;
            }
        }

        protected void SetRotationRoll(FlightCtrlState ctrlState, float roll)
        {
            if (this.vabPod) { ctrlState.roll = roll; }
            else { ctrlState.yaw = roll; }
        }
        #endregion

        #region Methods
        //TODO: those are essentially enum parsers, should implement my autoparsing class from RealChute
        protected string GetStatusString(Status st)
        {
            switch (st)
            {
                case Status.DISENGAGED:
                    return AutopilotConsts.disengaged;
                case Status.IDLE:
                    return AutopilotConsts.idle;
                case Status.DECELERATE:
                    return AutopilotConsts.decelerate;
                case Status.DESCEND:
                    return AutopilotConsts.descend;
                case Status.VERTICAL:
                    return AutopilotConsts.vertical;
                case Status.HORIZONTAL:
                    return AutopilotConsts.horizontal;
                case Status.WARP:
                    return AutopilotConsts.stsWarp;
                case Status.AVOID:
                    return AutopilotConsts.avoid;
                case Status.LANDED:
                    return AutopilotConsts.stsLanded;
                case Status.HOVER:
                    return AutopilotConsts.stsFloat;
                default:
                    return "NULL";
            }
        }
        protected string GetWarningString(Indicator i)
        {
            switch (i)
            {
                case Indicator.LANDED:
                    return AutopilotConsts.wrnLanded;
                case Indicator.WARP:
                    return AutopilotConsts.wrnWarp;
                case Indicator.SAFE:
                    return AutopilotConsts.safe;
                case Indicator.OK:
                    return AutopilotConsts.ok;
                case Indicator.DANGER:
                    return AutopilotConsts.danger;
                case Indicator.LOWTWR:
                    return AutopilotConsts.lowtwr;
                case Indicator.OUTSYNC:
                    return AutopilotConsts.outsync;
                case Indicator.FINAL:
                    return AutopilotConsts.final;
                case Indicator.HOVER:
                    return AutopilotConsts.wrnFloat;
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

        //Could set both to similar numbers and just cast?
        protected Indicator GetIndicator(DescentCalculator.DescentIndicator i)
        {
            switch (i)
            {
                case DescentCalculator.DescentIndicator.WARP:
                    return Indicator.WARP;
                case DescentCalculator.DescentIndicator.SAFE:
                    return Indicator.SAFE;
                case DescentCalculator.DescentIndicator.OK:
                    return Indicator.OK;
                case DescentCalculator.DescentIndicator.DANGER:
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
            return 0.05f * alt;
        }

        public float AllowedHoriSpeed(float alt)
        {
            return 0.1f * alt;
        }

        protected float VesselHeight()
        {
            float height = 0;
            foreach (Part p in ActiveVessel.parts)
            {
                Vector3 r = p.Rigidbody.worldCenterOfMass - ActiveVessel.CoM;
                float h = -HMaths.DotProduct(r, this.Up);
                height = HMaths.Max(height, h);
            }
            return height;
        }
        #endregion

        #region Functions
        public override void OnUpdate()
        {
            base.OnUpdate();

            //Calculate vessel states
            Vector3 coM = ActiveVessel.CoM;
            this.surfUpNormal = (coM - this.MainBody.position).normalized;
            this.g = (float)FlightGlobals.getGeeForceAtPosition(coM).magnitude;
            this.GAsl = (float)FlightGlobals.getGeeForceAtPosition(this.MainBody.position + ((Vector3d)this.surfUpNormal * this.MainBody.Radius)).magnitude;
            this.tad.OnUpdate(ActiveVessel, VesselHeight(), this.SlopeDetection);

            //Get vessel TWR
            this.TwrRcs = this.vabPod ? HydroJebCore.activeVesselRcs.maxAcc.zn : HydroJebCore.activeVesselRcs.maxAcc.yp;
            EngineCalculator cet = new EngineCalculator();
            cet.OnUpdate(ActiveVessel, this.vabPod ? Vector3.down : Vector3.forward);
            this.TwrEng = cet.MaxAcc * this.maxThrottle;
            this.hoverThrustRate = this.g / this.Twr;

            //Get vessel CurrentDescentBehaviour
            if (this.Landed && this.touchdown) { this.indicator = Indicator.LANDED; }
            else if (this.hoverThrustRate > 1.0F || this.GAsl > this.Twr)
            {
                this.indicator = Indicator.LOWTWR;
                this.engaged = false;
            }
            else if (this.AltAsl > this.MainBodySyncAlt)
            {
                this.indicator = Indicator.OUTSYNC;
                this.engaged = false;
            }
            else if (this.AltDiff < AutopilotConsts.finalDescentHeight) { this.indicator = this.touchdown ? Indicator.FINAL : Indicator.HOVER; }
            else //Ready for landing
            {
                this.cd.OnUpdate(AutopilotConsts.finalDescentHeight, this.safeTouchDownSpeed, this.GAsl, this.Twr, -this.VertSpeed, this.AltDiff);
                this.indicator = GetIndicator(this.cd.Indicator);
            }

            if (!this.engaged) { this.status = Status.DISENGAGED; }
            else if (TimeWarp.WarpMode == TimeWarp.Modes.HIGH && TimeWarp.CurrentRateIndex != 0) { this.status = Status.WARP; }
            else
            {
                if (this.indicator == Indicator.LANDED) { this.status = Status.LANDED; }
                else if (this.indicator == Indicator.HOVER) { this.status = Status.HOVER; }
                else if (this.indicator == Indicator.FINAL)
                {
                    this.status = this.HoriSpeed < this.SafeHorizontalSpeed ? Status.DESCEND : Status.AVOID;
                }
                else if (this.cd.Behaviour == DescentCalculator.DescentBehaviour.IDLE)
                {
                    if (this.HoriSpeed > HoriBrakeSpeed(this.AltTrue)) { this.status = Status.HORIZONTAL; }
                    else if (this.HoriSpeed < this.SafeHorizontalSpeed) { this.status = Status.IDLE; }
                    else { this.status = Status.DECELERATE; }
                }
                else if (this.cd.Behaviour == DescentCalculator.DescentBehaviour.BRAKE) { this.status = Status.VERTICAL; }
                else // CalculatorDescent.DescentBehaviour.NORMAL
                {
                    this.status = this.HoriSpeed > HoriBrakeSpeed(this.AltTrue) ? Status.HORIZONTAL : Status.DECELERATE;
                }
            }
        }

        protected override void LoadDefault()
        {
            base.LoadDefault();
            this.vabPod = AutopilotConsts.vabPod;
            this.engines = AutopilotConsts.engine;
            this.burnRetro = AutopilotConsts.burnRetro;
            this.touchdown = AutopilotConsts.touchdown;
            this.useTrueAlt = AutopilotConsts.useTrueAlt;
            this.safeTouchDownSpeed = AutopilotConsts.safeTouchDownSpeed;
            this.maxThrottle = AutopilotConsts.maxThrottle;
            this.altKeep = AutopilotConsts.altKeep;
        }
        #endregion
    }
}
using System;
using System.Collections.Generic;
using HydroTech.Autopilots.Calculators;
using HydroTech.Managers;
using HydroTech.Panels;
using HydroTech.Storage;
using HydroTech.Utils;
using UnityEngine;
using Direction = HydroTech.Autopilots.Calculators.GroundContactCalculator.Direction;

namespace HydroTech.Autopilots
{
    public class APLanding : Autopilot
    {
        public enum Indicator
        {
            WARP,
            SAFE,
            OK,
            DANGER,
            LANDED,
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

        #region Static fields
        private static readonly Dictionary<Indicator, Color> colorDict = new Dictionary<Indicator, Color>(9)
        #region Values
        {
            { Indicator.WARP,    Color.cyan   },
            { Indicator.SAFE,    Color.green  },
            { Indicator.OK,      Color.yellow },
            { Indicator.DANGER,  Color.red    },
            { Indicator.LANDED,  Color.blue   },
            { Indicator.LOWTWR,  Color.red    },
            { Indicator.OUTSYNC, Color.red    },
            { Indicator.FINAL,   Color.yellow },
            { Indicator.HOVER,   Color.yellow }
        };
        #endregion

        private static readonly Dictionary<Indicator, string> indicatorDict = new Dictionary<Indicator, string>(9)
        #region Values
        {
            { Indicator.WARP,    "Safe to warp"    },
            { Indicator.SAFE,    "Safe to land"    },
            { Indicator.OK,      "Ready to land"   },
            { Indicator.DANGER,  "Unsafe to land"  },
            { Indicator.LANDED,  "Landed"          },
            { Indicator.LOWTWR,  "TWR too low"     },
            { Indicator.OUTSYNC, "Past synch. alt" },
            { Indicator.FINAL,   "Close to ground" },
            { Indicator.HOVER,   "Close to ground" }
        };
        #endregion

        private static readonly Dictionary<Status, string> statusDict = new Dictionary<Status, string>(10)
        #region Values
        {
            { Status.DISENGAGED, "Disengaged"         },
            { Status.IDLE,       "Idle"               },
            { Status.HORIZONTAL, "Hor. decceleration" },
            { Status.VERTICAL,   "Ver. decceleration" },
            { Status.DECELERATE, "Decelerating"       },
            { Status.DESCEND,    "Descending"         },
            { Status.AVOID,      "Avoiding contact"   },
            { Status.WARP,       "Warping"            },
            { Status.LANDED,     "Holding"            },
            { Status.HOVER,      "Hovering"           }
        };
        #endregion
        #endregion

        #region Properties
        protected override string NameString
        {
            get { return "LandingAP"; }
        }

        public bool Landed
        {
            get { return ActiveVessel.LandedOrSplashed; }
        }

        public bool SlopeDetection
        {
            get { return this.AltAsl <= 2E5f; }
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
            get { return this.useTrueAlt ? this.altKeep : Mathf.Max(this.altKeep - this.TerrainHeight, HTUtils.finalDescentHeight); }
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

        public float Slope(Direction dir)
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
            get { return this.touchdown ? HTUtils.safeHorizontalSpeed : AllowedHoriSpeed(this.AltTrue); }
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
            get { return statusDict[this.status]; }
        }

        public string WarningString
        {
            get { return indicatorDict[this.indicator]; }
        }

        public Color WarningColor
        {
            get { return colorDict[this.indicator]; }
        }
        #endregion

        #region User input vars     
        [HydroSLNodeInfo(name = "SETTINGS"), HydroSLField(saveName = "UseEngines")]
        public bool engines = HTUtils.engine;
        public bool Engines
        {
            get { return this.engines; }
            set
            {
                if (value != this.engines) { FlightMainPanel.Instance.Landing.ResetHeight(); }
                this.engines = value;
            }
        }

        [HydroSLNodeInfo(name = "SETTINGS"), HydroSLField(saveName = "PointUp")]
        public bool vabPod = HTUtils.vabPod;

        [HydroSLNodeInfo(name = "SETTIINGS"), HydroSLField(saveName = "BurnRetro", isTesting = true)]
        public bool burnRetro = HTUtils.burnRetro;

        [HydroSLNodeInfo(name = "SETTINGS"), HydroSLField(saveName = "Touchdown")]
        public bool touchdown = HTUtils.touchdown;

        [HydroSLNodeInfo(name = "SETTINGS"), HydroSLField(saveName = "TrueAlt")]
        public bool useTrueAlt = HTUtils.useTrueAlt;

        [HydroSLNodeInfo(name = "SETTINGS"), HydroSLField(saveName = "TouchdownSpeed")]
        public float safeTouchDownSpeed = HTUtils.safeTouchDownSpeed;

        [HydroSLNodeInfo(name = "SETTINGS"), HydroSLField(saveName = "MaxThrottle")]
        public float maxThrottle = HTUtils.maxThrottle;

        [HydroSLNodeInfo(name = "SETTINGS"), HydroSLField(saveName = "Altitude")]
        public float altKeep = HTUtils.altKeep;

        public float MaxThrottle
        {
            get { return this.maxThrottle * 100; }
            set { this.maxThrottle = value / 100; }
        }

        public override bool Engaged
        {
            set
            {
                if (value != this.engaged) { FlightMainPanel.Instance.Landing.ResetHeight(); }
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
            this.fileName = new FileName("landing", "cfg", FileName.autopilotSaveFolder);
        }
        #endregion

        #region Autopilot
        protected bool DriveHoldDir(FlightCtrlState ctrlState, Vector3 dir)
        {
            // Point up
            LandingCalculator stateCal = new LandingCalculator();
            stateCal.Calculate(this.vabPod, dir, Vector3d.zero, ActiveVessel);
            stateCal.SetCtrlStateRotation(ctrlState);
            ActiveRCS.MakeRotation(ctrlState, stateCal.Steer(0.05f /*2.87°*/) ? 5 : 20);

            // Kill rotation
            Vector3 angularVelocity = VectorTransform(ActiveVessel.GetComponent<Rigidbody>().angularVelocity, ActiveVessel.ReferenceTransform);
            SetRotationRoll(ctrlState, angularVelocity.z);

            return stateCal.Steer(0.05f); //2.87°
        }

        protected void DeployLandingGears()
        {
            HydroActionGroupManager.ActiveVessel.Gear = true;
        }

        protected void CheckAltitudeAndDeployLandingGears()
        {
            if (this.HoriSpeed < HTUtils.safeHorizontalSpeed && this.AltTrue <= 200) { DeployLandingGears(); }
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
            ctrlState.X = HTUtils.Clamp(HydroFlightManager.Instance.ActiveRCS.GetThrustRateFromAcc3(0, this.SurfXSpeed * 5), -1, 1);
            SetTranslationY(ctrlState, HTUtils.Clamp(HydroFlightManager.Instance.ActiveRCS.GetThrustRateFromAcc3(this.vabPod ? 1 : 2, this.SurfYSpeed * 5), -1, 1));
        }

        protected void DriveHorizontalBrake(FlightCtrlState ctrlState)
        {
            ctrlState.X = Mathf.Sign(this.SurfXSpeed);
            SetTranslationY(ctrlState, Mathf.Sign(this.SurfYSpeed));
        }

        protected void DriveFinalDescent(FlightCtrlState ctrlState)
        {
            DriveHorizontalDec(ctrlState);
            SetTranslationZ(ctrlState, HTUtils.Clamp(-this.hoverThrustRate + ((this.safeTouchDownSpeed + this.VertSpeed) / this.Twr), -1, 0));
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
            float modifier = Mathf.Max(this.AltDiff, -10) + this.VertSpeed;
            SetTranslationZ(ctrlState, HTUtils.Clamp(-this.hoverThrustRate + (modifier / this.Twr), -1, 0));
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
        protected float GetBodySyncAltitude(CelestialBody body)
        {
            return (float)Math.Pow((body.gravParameter * body.rotationPeriod * body.rotationPeriod) / (4 * Math.PI * Math.PI), 1d / 3);
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
                float h = -Vector3.Dot(r, this.Up);
                height = Mathf.Max(height, h);
            }
            return height;
        }
        #endregion

        #region Overrides
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
            this.TwrRcs = this.vabPod ? HydroFlightManager.Instance.ActiveRCS.maxAcc.zn : HydroFlightManager.Instance.ActiveRCS.maxAcc.yp;
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
            else if (this.AltDiff < HTUtils.finalDescentHeight) { this.indicator = this.touchdown ? Indicator.FINAL : Indicator.HOVER; }
            else //Ready for landing
            {
                this.cd.OnUpdate(HTUtils.finalDescentHeight, this.safeTouchDownSpeed, this.GAsl, this.Twr, -this.VertSpeed, this.AltDiff);
                this.indicator = (Indicator)this.cd.Indicator;
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
            this.vabPod = HTUtils.vabPod;
            this.engines = HTUtils.engine;
            this.burnRetro = HTUtils.burnRetro;
            this.touchdown = HTUtils.touchdown;
            this.useTrueAlt = HTUtils.useTrueAlt;
            this.safeTouchDownSpeed = HTUtils.safeTouchDownSpeed;
            this.maxThrottle = HTUtils.maxThrottle;
            this.altKeep = HTUtils.altKeep;
        }
        #endregion
    }
}
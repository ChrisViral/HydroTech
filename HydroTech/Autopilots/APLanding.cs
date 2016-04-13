using System.Collections.Generic;
using HydroTech.Autopilots.Calculators;
using HydroTech.Managers;
using HydroTech.Panels;
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

        #region Constants
        public const float finalDescentHeight = 10;
        #endregion

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

        #region Static properties
        private static Vector3 SurfVel
        {
            get { return ActiveVessel.srf_velocity; }
        }

        private static Vector3 SurfVelVessel
        {
            get { return HTUtils.VectorTransform(SurfVel, ActiveVessel.ReferenceTransform); }
        }

        private static float SurfXSpeed
        {
            get { return SurfVelVessel.x; }
        }

        private static CelestialBody MainBody
        {
            get { return FlightGlobals.currentMainBody; }
        }
        #endregion

        #region Fields
        public bool burnRetro;
        public bool vabPod = true;
        public bool touchdown = true;
        public bool useTrueAlt = true;
        public float safeTouchDownSpeed = 0.5f;
        public float maxThrottle = 1;
        public float altKeep = 10;
        private float hoverThrustRate;
        private float g;
        private Vector3 surfUpNormal;
        private Status status = Status.DISENGAGED;
        private Indicator indicator = Indicator.SAFE;
        private readonly GroundContactCalculator groundCalc = new GroundContactCalculator();
        private readonly DescentCalculator desCalc = new DescentCalculator();
        #endregion

        #region Properties
        public bool engines;
        public bool Engines
        {
            get { return this.engines; }
            set
            {
                if (value != this.engines) { FlightMainPanel.Instance.Landing.ResetHeight(); }
                this.engines = value;
            }
        }

        public float GeeASL { get; private set; }

        public float TwrRCS { get; private set; }

        public float TwrEng { get; private set; }

        public float TWR
        {
            get { return this.engines ? this.TwrRCS + this.TwrEng : this.TwrRCS; }
        }

        public float MaxThrottle
        {
            get { return this.maxThrottle * 100; }
            set { this.maxThrottle = value / 100; }
        }

        public bool SlopeDetection
        {
            get { return this.AltASL <= 2E5f; }
        }

        public float AltASL
        {
            get { return (float)MainBody.GetAltitude(ActiveVessel.CoM); }
        }

        public float AltTrue
        {
            get { return this.groundCalc.DistCenter; }
        }

        public float TerrainHeight
        {
            get { return this.AltASL - this.AltTrue; }
        }

        public float AltKeepTrue
        {
            get { return this.useTrueAlt ? this.altKeep : Mathf.Max(this.altKeep - this.TerrainHeight, finalDescentHeight); }
        }

        public float AltKeepASL
        {
            get { return this.useTrueAlt ? this.altKeep + this.TerrainHeight : this.altKeep; }
        }

        public float AltDiff
        {
            get { return this.touchdown ? this.AltTrue : this.AltTrue - this.AltKeepTrue; }
        }

        private float VesselHeight
        {
            get
            {
                float height = 0;
                foreach (Part p in ActiveVessel.parts)
                {
                    Vector3 r = p.WCoM - ActiveVessel.CoM;
                    float h = -Vector3.Dot(r, this.Up);
                    height = Mathf.Max(height, h);
                }
                return height;
            }
        }

        public bool Terrain
        {
            get { return this.groundCalc.terrain; }
        }

        public float DetectRadius
        {
            get { return this.groundCalc.Radius; }
        }

        public float Slope(Direction dir)
        {
            return this.groundCalc.Slope(dir);
        }

        public float VertSpeed
        {
            get { return (float)ActiveVessel.verticalSpeed; }
        }

        public float HorSpeed
        {
            get { return (float)ActiveVessel.horizontalSrfSpeed; }
        }

        private float SurfYSpeed
        {
            get { return this.vabPod ? SurfVelVessel.y : SurfVelVessel.z; }
        }

        public float AllowedHori
        {
            get { return 0.01f * this.AltKeepTrue; }
        }

        private float SafeHorizontalSpeed
        {
            get { return this.touchdown ? 0.1f : 0.01f * this.AltTrue; }
        }

        private Vector3 Up
        {
            get { return this.vabPod ? ActiveVessel.ReferenceTransform.up : -ActiveVessel.ReferenceTransform.forward; }
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

        protected override string NameString
        {
            get { return "LandingAP"; }
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

        #region Methods
        private bool DriveHoldDir(FlightCtrlState ctrlState, Vector3 dir)
        {
            // Point up
            LandingCalculator stateCal = new LandingCalculator();
            stateCal.Calculate(this.vabPod, dir, Vector3d.zero, ActiveVessel);
            stateCal.SetCtrlStateRotation(ctrlState);
            ActiveRCS.MakeRotation(ctrlState, stateCal.Steer(0.05f /*2.87°*/) ? 5 : 20);

            // Kill rotation
            Vector3 angularVelocity = HTUtils.VectorTransform(ActiveVessel.GetComponent<Rigidbody>().angularVelocity, ActiveVessel.ReferenceTransform);
            SetRotationRoll(ctrlState, angularVelocity.z);

            return stateCal.Steer(0.05f); //2.87°
        }

        private void CheckAltitudeAndDeployLandingGears()
        {
            if (this.HorSpeed < 0.1f && this.AltTrue <= 200) { HTUtils.SetState(FlightGlobals.ActiveVessel, KSPActionGroup.Gear, true); }
        }

        private void DriveHorizontalDec(FlightCtrlState ctrlState)
        {
            ctrlState.X = HTUtils.Clamp(HydroFlightManager.Instance.ActiveRCS.GetThrustRateFromAcc3(0, SurfXSpeed * 5), -1, 1);
            SetTranslationY(ctrlState, HTUtils.Clamp(HydroFlightManager.Instance.ActiveRCS.GetThrustRateFromAcc3(this.vabPod ? 1 : 2, this.SurfYSpeed * 5), -1, 1));
        }

        private void DriveHorizontalBrake(FlightCtrlState ctrlState)
        {
            ctrlState.X = Mathf.Sign(SurfXSpeed);
            SetTranslationY(ctrlState, Mathf.Sign(this.SurfYSpeed));
        }

        private void DriveFinalDescent(FlightCtrlState ctrlState)
        {
            DriveHorizontalDec(ctrlState);
            SetTranslationZ(ctrlState, HTUtils.Clamp(-this.hoverThrustRate + ((this.safeTouchDownSpeed + this.VertSpeed) / this.TWR), -1, 0));
        }

        private void DriveAvoidContact(FlightCtrlState ctrlState)
        {
            if (this.VertSpeed < 0) { SetTranslationZ(ctrlState, -1); }
            else
            {
                SetTranslationZ(ctrlState, -this.hoverThrustRate);
                DriveHorizontalDec(ctrlState);
            }
        }

        private void DriveHoverManeuver(FlightCtrlState ctrlState)
        {
            float modifier = Mathf.Max(this.AltDiff, -10) + this.VertSpeed;
            SetTranslationZ(ctrlState, HTUtils.Clamp(-this.hoverThrustRate + (modifier / this.TWR), -1, 0));
            if (this.HorSpeed > this.SafeHorizontalSpeed) { DriveHorizontalDec(ctrlState); }
        }

        private void SetTranslationY(FlightCtrlState ctrlState, float y)
        {
            if (this.vabPod) { ctrlState.Y = y; }
            else
            {
                ctrlState.Z = y;
            }
        }

        private void SetTranslationZ(FlightCtrlState ctrlState, float z)
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

        private void SetRotationRoll(FlightCtrlState ctrlState, float roll)
        {
            if (this.vabPod) { ctrlState.roll = roll; }
            else { ctrlState.yaw = roll; }
        }
        #endregion

        #region Overrides
        public override void OnUpdate()
        {
            base.OnUpdate();

            //Calculate vessel states
            Vector3 coM = ActiveVessel.CoM;
            this.surfUpNormal = (coM - MainBody.position).normalized;
            this.g = (float)FlightGlobals.getGeeForceAtPosition(coM).magnitude;
            this.GeeASL = (float)FlightGlobals.getGeeForceAtPosition(MainBody.position + ((Vector3d)this.surfUpNormal * MainBody.Radius)).magnitude;
            this.groundCalc.OnUpdate(ActiveVessel, this.VesselHeight, this.SlopeDetection);

            //Get vessel TWR
            this.TwrRCS = this.vabPod ? HydroFlightManager.Instance.ActiveRCS.maxAcc.zn : HydroFlightManager.Instance.ActiveRCS.maxAcc.yp;
            EngineCalculator cet = new EngineCalculator();
            cet.OnUpdate(ActiveVessel, this.vabPod ? Vector3.down : Vector3.forward);
            this.TwrEng = cet.MaxAcc * this.maxThrottle;
            this.hoverThrustRate = this.g / this.TWR;

            //Get vessel CurrentDescentBehaviour
            if (ActiveVessel.LandedOrSplashed && this.touchdown) { this.indicator = Indicator.LANDED; }
            else if (this.hoverThrustRate > 1.0F || this.GeeASL > this.TWR)
            {
                this.indicator = Indicator.LOWTWR;
                this.engaged = false;
            }
            else if (this.AltASL > HTUtils.GetBodySyncAltitude(MainBody))
            {
                this.indicator = Indicator.OUTSYNC;
                this.engaged = false;
            }
            else if (this.AltDiff < finalDescentHeight) { this.indicator = this.touchdown ? Indicator.FINAL : Indicator.HOVER; }
            else //Ready for landing
            {
                this.desCalc.OnUpdate(finalDescentHeight, this.safeTouchDownSpeed, this.GeeASL, this.TWR, -this.VertSpeed, this.AltDiff);
                this.indicator = (Indicator)this.desCalc.Indicator;
            }

            if (!this.engaged) { this.status = Status.DISENGAGED; }
            else if (TimeWarp.WarpMode == TimeWarp.Modes.HIGH && TimeWarp.CurrentRateIndex != 0) { this.status = Status.WARP; }
            else
            {
                if (this.indicator == Indicator.LANDED) { this.status = Status.LANDED; }
                else if (this.indicator == Indicator.HOVER) { this.status = Status.HOVER; }
                else if (this.indicator == Indicator.FINAL)
                {
                    this.status = this.HorSpeed < this.SafeHorizontalSpeed ? Status.DESCEND : Status.AVOID;
                }
                else if (this.desCalc.Behaviour == DescentCalculator.DescentBehaviour.IDLE)
                {
                    if (this.HorSpeed > 0.05f * this.AltTrue) { this.status = Status.HORIZONTAL; }
                    else if (this.HorSpeed < this.SafeHorizontalSpeed) { this.status = Status.IDLE; }
                    else { this.status = Status.DECELERATE; }
                }
                else if (this.desCalc.Behaviour == DescentCalculator.DescentBehaviour.BRAKE) { this.status = Status.VERTICAL; }
                else // CalculatorDescent.DescentBehaviour.NORMAL
                {
                    this.status = this.HorSpeed > 0.05f * this.AltTrue ? Status.HORIZONTAL : Status.DECELERATE;
                }
            }
        }

        protected override void DriveAutopilot(FlightCtrlState ctrlState)
        {
            base.DriveAutopilot(ctrlState);

            HTUtils.SetState(FlightGlobals.ActiveVessel, KSPActionGroup.SAS, false);

            if (this.touchdown || this.status != Status.HOVER) { RemoveUserInput(ctrlState); }

            if (this.Engines && this.burnRetro)
            {
                Vector3 decVector;
                switch (this.status)
                {
                    case Status.IDLE:
                        decVector = SurfVel.normalized;
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
                        decVector = new Vector3(SurfVel.x, SurfVel.y, 0).normalized;
                        break;
                    case Status.VERTICAL:
                        decVector = -this.surfUpNormal;
                        break;
                    case Status.DECELERATE:
                        decVector = SurfVel.normalized;
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
                        SetTranslationZ(ctrlState, (this.desCalc.ThrRate / SurfVel.z) * SurfVel.magnitude);
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
                    SetTranslationZ(ctrlState, -this.desCalc.ThrRate);
                    return;
                default:
                    return;
            }
        }
        #endregion
    }
}
using System.Collections.Generic;
using HydroTech.Autopilots.Calculators;
using HydroTech.Managers;
using HydroTech.Panels;
using UnityEngine;
using static System.Linq.Enumerable;
using static HydroTech.Extensions.CelestialBodyExtensions;
using static HydroTech.Extensions.VesselExtensions;
using static HydroTech.Utils.HTUtils;

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
        private static Vector3 SurfVel => ActiveVessel.srf_velocity;

        private static Vector3 SurfVelVessel => SwitchTransformCalculator.VectorTransform(SurfVel, ActiveVessel.ReferenceTransform);

        private static float SurfXSpeed => SurfVelVessel.x;

        private static CelestialBody MainBody => FlightGlobals.currentMainBody;
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
        public readonly GroundContactCalculator groundCalc = new GroundContactCalculator();
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

        public float TWR => this.engines ? this.TwrRCS + this.TwrEng : this.TwrRCS;

        public float AltASL => (float)MainBody.GetAltitude(ActiveVessel.CoM);

        public float AltTrue => this.groundCalc.DistCenter;

        public float TerrainHeight => this.AltASL - this.AltTrue;

        public float AltKeepTrue => this.useTrueAlt ? this.altKeep : Mathf.Max(this.altKeep - this.TerrainHeight, finalDescentHeight);

        public float AltKeepASL => this.useTrueAlt ? this.altKeep + this.TerrainHeight : this.altKeep;

        private float AltDiff => this.touchdown ? this.AltTrue : this.AltTrue - this.AltKeepTrue;

        private float SurfYSpeed => this.vabPod ? SurfVelVessel.y : SurfVelVessel.z;

        private float SafeHorizontalSpeed => this.touchdown ? 0.1f : 0.01f * this.AltTrue;

        private Vector3 Up => this.vabPod ? ActiveVessel.ReferenceTransform.up : -ActiveVessel.ReferenceTransform.forward;

        public string StatusString => statusDict[this.status];

        public string WarningString => indicatorDict[this.indicator];

        public Color WarningColor => colorDict[this.indicator];

        protected override string NameString => "LandingAP";

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
            Vector3 angularVelocity = SwitchTransformCalculator.VectorTransform(ActiveVessel.GetComponent<Rigidbody>().angularVelocity, ActiveVessel.ReferenceTransform);
            SetRotationRoll(ctrlState, angularVelocity.z);

            return stateCal.Steer(0.05f); //2.87°
        }

        private void CheckAltitudeAndDeployLandingGears()
        {
            if (ActiveVessel.horizontalSrfSpeed < 0.1 && this.AltTrue <= 200) { FlightGlobals.ActiveVessel.SetState(KSPActionGroup.Gear, true); }
        }

        private void DriveHorizontalDec(FlightCtrlState ctrlState)
        {
            ctrlState.X = Clamp(HydroFlightManager.Instance.ActiveRCS.GetThrustRateFromAcc3(0, SurfXSpeed * 5), -1, 1);
            SetTranslationY(ctrlState, Clamp(HydroFlightManager.Instance.ActiveRCS.GetThrustRateFromAcc3(this.vabPod ? 1 : 2, this.SurfYSpeed * 5), -1, 1));
        }

        private void DriveHorizontalBrake(FlightCtrlState ctrlState)
        {
            ctrlState.X = Mathf.Sign(SurfXSpeed);
            SetTranslationY(ctrlState, Mathf.Sign(this.SurfYSpeed));
        }

        private void DriveFinalDescent(FlightCtrlState ctrlState)
        {
            DriveHorizontalDec(ctrlState);
            SetTranslationZ(ctrlState, Clamp(-this.hoverThrustRate + ((this.safeTouchDownSpeed + (float)ActiveVessel.verticalSpeed) / this.TWR), -1, 0));
        }

        private void DriveAvoidContact(FlightCtrlState ctrlState)
        {
            if (ActiveVessel.verticalSpeed < 0) { SetTranslationZ(ctrlState, -1); }
            else
            {
                SetTranslationZ(ctrlState, -this.hoverThrustRate);
                DriveHorizontalDec(ctrlState);
            }
        }

        private void DriveHoverManeuver(FlightCtrlState ctrlState)
        {
            float modifier = Mathf.Max(this.AltDiff, -10) + (float)ActiveVessel.verticalSpeed;
            SetTranslationZ(ctrlState, Clamp(-this.hoverThrustRate + (modifier / this.TWR), -1, 0));
            if ((float)ActiveVessel.horizontalSrfSpeed > this.SafeHorizontalSpeed) { DriveHorizontalDec(ctrlState); }
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
            this.groundCalc.OnUpdate(ActiveVessel, ActiveVessel.parts.Select(p => -Vector3.Dot(p.WCoM - ActiveVessel.CoM, this.Up)).Max(), this.AltASL <= 2E5f);

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
            else if (this.AltASL > MainBody.SyncAltitude())
            {
                this.indicator = Indicator.OUTSYNC;
                this.engaged = false;
            }
            else if (this.AltDiff < finalDescentHeight) { this.indicator = this.touchdown ? Indicator.FINAL : Indicator.HOVER; }
            else //Ready for landing
            {
                this.desCalc.OnUpdate(finalDescentHeight, this.safeTouchDownSpeed, this.GeeASL, this.TWR, -(float)ActiveVessel.verticalSpeed, this.AltDiff);
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
                    this.status = (float)ActiveVessel.horizontalSrfSpeed < this.SafeHorizontalSpeed ? Status.DESCEND : Status.AVOID;
                }
                else if (this.desCalc.Behaviour == DescentCalculator.DescentBehaviour.IDLE)
                {
                    if (ActiveVessel.horizontalSrfSpeed > 0.05 * this.AltTrue) { this.status = Status.HORIZONTAL; }
                    else if ((float)ActiveVessel.horizontalSrfSpeed < this.SafeHorizontalSpeed) { this.status = Status.IDLE; }
                    else { this.status = Status.DECELERATE; }
                }
                else if (this.desCalc.Behaviour == DescentCalculator.DescentBehaviour.BRAKE) { this.status = Status.VERTICAL; }
                else // CalculatorDescent.DescentBehaviour.NORMAL
                {
                    this.status = (float)ActiveVessel.horizontalSrfSpeed > 0.05 * this.AltTrue ? Status.HORIZONTAL : Status.DECELERATE;
                }
            }
        }

        protected override void DriveAutopilot(FlightCtrlState ctrlState)
        {
            base.DriveAutopilot(ctrlState);

            FlightGlobals.ActiveVessel.SetState(KSPActionGroup.SAS, false);

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
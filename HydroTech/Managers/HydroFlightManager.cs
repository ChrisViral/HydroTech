using System.Collections.Generic;
using System.Linq;
using HydroTech.Autopilots;
using HydroTech.Autopilots.Calculators;
using HydroTech.Panels;
using HydroTech.Utils;
using UnityEngine;

namespace HydroTech.Managers
{
    /// <summary>
    /// HydroTech general flight manager
    /// </summary>
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class HydroFlightManager : MonoBehaviour
    {
        #region Instance
        /// <summary>
        /// Current instance
        /// </summary>
        public static HydroFlightManager Instance { get; private set; }
        #endregion

        #region Properties
        /// <summary>
        /// Current RCS calculator
        /// </summary>
        public RCSCalculator ActiveRCS { get; private set; }

        /// <summary>
        /// Current active core
        /// </summary>
        public HydroJebCore Active { get; private set; }

        /// <summary>
        /// All target cores on other vessels
        /// </summary>
        public List<HydroJebCore> TargetVessels { get; private set; }

        /// <summary>
        /// All cameras on this vessel
        /// </summary>
        public List<ModuleDockAssistCam> ActiveCams { get; private set; }

        /// <summary>
        /// All targets on nearby vessels
        /// </summary>
        public List<ModuleDockAssistTarget> NearbyTargets { get; private set; }

        /// <summary>
        /// Current camera manager
        /// </summary>
        public HydroCameraManager CameraManager { get; private set; }

        /// <summary>
        /// Current input manager
        /// </summary>
        public HydroInputManager InputManager { get; private set; }

        /// <summary>
        /// Current docking autopilot
        /// </summary>
        public APDockAssist DockingAutopilot { get; private set; }

        /// <summary>
        /// Current landing autopilot
        /// </summary>
        public APLanding LandingAutopilot { get; private set; }

        /// <summary>
        /// Current precise control autopilot
        /// </summary>
        public APPreciseControl PreciseControlAutopilot { get; private set; }

        /// <summary>
        /// Current translation autopilot
        /// </summary>
        public APTranslation TranslationAutopilot { get; private set; }

        /// <summary>
        /// All current autopilots
        /// </summary>
        public List<Autopilot> Autopilots { get; private set; }
        #endregion

        #region Methods
        /// <summary>
        /// If the current core is the active one
        /// </summary>
        /// <param name="core">Core to check</param>
        /// <returns>If the core is active</returns>
        public bool IsActiveJeb(HydroJebCore core) => this.Active == core;

        /// <summary>
        /// Pauses all autopilots
        /// </summary>
        private void OnPause() => this.Autopilots.ForEach(ap => ap.OnGamePause());

        /// <summary>
        /// Resumes all autopilots
        /// </summary>
        private void OnResume() => this.Autopilots.ForEach(ap => ap.OnGameResume());
        #endregion

        #region Functions
        /// <summary>
        /// Awake function
        /// </summary>
        private void Awake()
        {
            if (Instance != null) { Destroy(this); return; }

            Instance = this;
            this.ActiveRCS = new RCSCalculator();
            this.TargetVessels = new List<HydroJebCore>();
            this.ActiveCams = new List<ModuleDockAssistCam>();
            this.NearbyTargets = new List<ModuleDockAssistTarget>();

            this.CameraManager = new HydroCameraManager();
            this.InputManager = new HydroInputManager();
            this.DockingAutopilot = new APDockAssist();
            this.LandingAutopilot = new APLanding();
            this.PreciseControlAutopilot = new APPreciseControl();
            this.TranslationAutopilot = new APTranslation();
            this.Autopilots = new List<Autopilot>
            {
                this.DockingAutopilot,
                this.LandingAutopilot,
                this.PreciseControlAutopilot,
                this.TranslationAutopilot
            };

            GameEvents.onGamePause.Add(OnPause);
            GameEvents.onGameUnpause.Add(OnResume);
        }

        /// <summary>
        /// Start function
        /// </summary>
        private void Start()
        {
            this.CameraManager.Start();
            this.InputManager.Start();
            this.Autopilots.ForEach(ap => ap.OnFlightStart());
            FlightMainPanel.Instance.Panels.ForEach(p => p.OnFlightStart());
        }

        /// <summary>
        /// OnDestroy function
        /// </summary>
        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
                GameEvents.onGamePause.Remove(OnPause);
                GameEvents.onGameUnpause.Remove(OnResume);
            }
        }

        /// <summary>
        /// Update function
        /// </summary>
        private void Update()
        {
            HydroToolbarManager.Flight.Update(this.Active);
            this.CameraManager.Update();
            this.InputManager.Update();
        }

        /// <summary>
        /// FixedUpdate function
        /// </summary>
        private void FixedUpdate()
        {
            if (!FlightGlobals.ready) { return; }

            if (this.TargetVessels.Count != 0) { this.TargetVessels.Clear(); }
            if (this.ActiveCams.Count != 0)    { this.ActiveCams.Clear(); }
            if (this.NearbyTargets.Count != 0) { this.NearbyTargets.Clear(); }

            foreach (Vessel vessel in FlightGlobals.Vessels.Where(v => v.loaded))
            {
                HydroJebCore jeb = vessel.GetMasterJeb();
                if (jeb != null)
                {
                    if (vessel.isActiveVessel)
                    {
                        this.Active = jeb;
                        this.ActiveCams.AddRange(vessel.FindModulesImplementing<ModuleDockAssistCam>());
                    }
                    else
                    {
                        this.TargetVessels.Add(jeb);
                        this.NearbyTargets.AddRange(vessel.FindModulesImplementing<ModuleDockAssistTarget>());
                    }
                }
            }

            FlightMainPanel.Instance.DockAssist.TargetVessels.Update(this.TargetVessels);
            FlightMainPanel.Instance.DockAssist.Cameras.Update(this.ActiveCams);
            FlightMainPanel.Instance.DockAssist.Targets.Update(this.NearbyTargets);

            this.ActiveRCS.OnUpdate(FlightGlobals.ActiveVessel);
            this.Autopilots.ForEach(ap => ap.OnUpdate());
        }
        #endregion
    }
}
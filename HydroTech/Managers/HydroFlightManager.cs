using System.Collections.Generic;
using HydroTech.Autopilots;
using HydroTech.Autopilots.Calculators;
using HydroTech.PartModules;
using HydroTech.Storage;
using HydroTech.Utils;
using UnityEngine;

namespace HydroTech.Managers
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class HydroFlightManager : MonoBehaviour
    {
        #region Instance
        public static HydroFlightManager Instance { get; private set; }
        #endregion

        #region Properties
        public HydroCameraManager CameraManager { get; private set; }

        public HydroInputManager InputManager { get; private set; }

        public APDockAssist DockingAutopilot { get; private set; }

        public APLanding LandingAutopilot { get; private set; }

        public APPreciseControl PreciseControlAutopilot { get; private set; }

        public APTranslation TranslationAutopilot { get; private set; }

        public List<Autopilot> Autopilots { get; private set; }

        public RCSCalculator ActiveRCS { get; private set; }

        public HydroJebModule Active { get; private set; }

        public List<Part> Targets { get; private set; }

        public List<ModuleDockAssistCam> ActiveCams { get; private set; }

        public List<ModuleDockAssistCam> NearbyCams { get; private set; }

        public List<ModuleDockAssistTarget> ActiveTargets { get; private set; }

        public List<ModuleDockAssistTarget> NearbyTargets { get; private set; }
        #endregion

        #region Methods
        public bool IsActiveJeb(HydroJebModule module)
        {
            return this.Active == module;
        }
        #endregion

        #region Functions
        private void Awake()
        {
            if (Instance != null) { Destroy(this); return; }
            Instance = this;
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
            this.ActiveRCS = new RCSCalculator();
            this.Targets = new List<Part>();
            this.ActiveCams = new List<ModuleDockAssistCam>();
            this.NearbyCams = new List<ModuleDockAssistCam>();
            this.ActiveTargets = new List<ModuleDockAssistTarget>();
            this.NearbyTargets = new List<ModuleDockAssistTarget>();
        }

        private void Start()
        {
            this.CameraManager.Start();
            this.InputManager.Start();
        }

        private void OnDestroy()
        {
            Instance = null;
        }

        private void Update()
        {
            HydroToolbarManager.Flight.Update();
            this.CameraManager.Update();
            this.InputManager.Update();
        }

        private void FixedUpdate()
        {
            if (this.Targets.Count != 0) { this.Targets.Clear(); }
            if (this.ActiveCams.Count != 0) { this.ActiveCams.Clear(); }
            if (this.NearbyCams.Count != 0) { this.NearbyCams.Clear(); }
            if (this.ActiveTargets.Count != 0) { this.ActiveTargets.Clear(); }
            if (this.NearbyTargets.Count != 0) { this.NearbyTargets.Clear(); }

            foreach (Vessel vessel in FlightGlobals.Vessels)
            {
                HydroJebModule jeb = vessel.GetMasterJeb();
                List<ModuleDockAssistCam> cams = vessel.FindPartModulesImplementing<ModuleDockAssistCam>();
                List<ModuleDockAssistTarget> targets = vessel.FindPartModulesImplementing<ModuleDockAssistTarget>();
                if (jeb != null)
                {
                    if (vessel.isActiveVessel)
                    {
                        this.Active = jeb;
                        this.ActiveCams.AddRange(cams);
                        this.ActiveTargets.AddRange(targets);
                    }
                    else
                    {
                        this.Targets.Add(jeb.part);
                        this.NearbyCams.AddRange(cams);
                        this.NearbyTargets.AddRange(targets);
                    }
                }
            }
        }
        #endregion
    }
}
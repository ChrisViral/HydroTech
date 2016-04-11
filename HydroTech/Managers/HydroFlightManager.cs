using System.Collections.Generic;
using HydroTech.Autopilots;
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

        public HydroJebModule Active { get; private set; }

        public List<Part> Targets { get; private set; }
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
            this.Autopilots = new List<Autopilot>()
            {
                this.DockingAutopilot,
                this.LandingAutopilot,
                this.PreciseControlAutopilot,
                this.TranslationAutopilot
            };
            this.Targets = new List<Part>();
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
            foreach (Vessel vessel in FlightGlobals.Vessels)
            {
                HydroJebModule jeb = vessel.GetMasterJeb();
                if (jeb != null)
                {
                    if (vessel.isActiveVessel) { this.Active = jeb; }
                    else { this.Targets.Add(jeb.part); }
                }
            }
        }
        #endregion
    }
}
using HydroTech.Managers;
using HydroTech.Panels;
using UnityEngine;

namespace HydroTech
{
    public class ModuleDockAssistTarget : ModuleDockAssist, ITargetable
    {
        #region Static Properties
        private static ModuleDockAssistTarget Current
        {
            get { return HydroFlightManager.Instance.DockingAutopilot.target; }
            set { HydroFlightManager.Instance.DockingAutopilot.target = value; }
        }
        #endregion

        #region Properties
        private bool isNear;
        public bool IsNear
        {
            get
            {
                if (this.vessel.isActiveVessel)
                {
                    if (this.isNear && Current == this) { FlightMainPanel.Instance.DockAssist.ResetHeight(); }
                    this.isNear = false;
                }
                else { this.isNear = true; }
                return this.isNear;
            }
        }

        protected override string ModuleShort => "Tgt";
        #endregion

        #region Methods
        public Transform GetTransform() => this.assist;

        public Vector3 GetObtVelocity() => this.vessel.obt_velocity;

        public Vector3 GetSrfVelocity() => this.vessel.srf_velocity;

        public Vector3 GetFwdVector() => -this.assist.forward;

        public Vessel GetVessel() => this.vessel;

        public string GetName() => this.assistName + " target";

        public Orbit GetOrbit() => this.vessel.orbit;

        public OrbitDriver GetOrbitDriver() => this.vessel.orbitDriver;

        public VesselTargetModes GetTargetingMode() => VesselTargetModes.DirectionVelocityAndOrientation;
        #endregion

        #region Functions
        protected override void OnDestroy()
        {
            if (!HighLogic.LoadedSceneIsFlight) { return; }

            base.OnDestroy();
            if (this == Current)
            {
                Current = null;
                FlightMainPanel.Instance.DockAssist.ResetHeight();
            }         
        }
        #endregion

        #region Overrides
        public override string GetModuleTitle() => "Docking Target";

        public override string GetInfo() => "Active Docking Target";
        #endregion
    }
}
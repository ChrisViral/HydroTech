using HydroTech.Managers;
using HydroTech.Panels;
using UnityEngine;

namespace HydroTech
{
    /// <summary>
    /// Docking target module
    /// </summary>
    public class ModuleDockAssistTarget : ModuleDockAssist, ITargetable
    {
        #region Static Properties
        /// <summary>
        /// Current selected target
        /// </summary>
        private static ModuleDockAssistTarget Current
        {
            get { return HydroFlightManager.Instance.DockingAutopilot.Target; }
            set { HydroFlightManager.Instance.DockingAutopilot.Target = value; }
        }
        #endregion

        #region Properties
        private bool isNear;
        /// <summary>
        /// Not quite sure, but seems to indicate if on a nearby vessel
        /// </summary>
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

        /// <summary>
        /// Target module short
        /// </summary>
        protected override string ModuleShort => "Tgt";
        #endregion

        #region Methods
        /// <summary>
        /// Target transform
        /// </summary>
        /// <returns>Assist transform</returns>
        public Transform GetTransform() => this.assist;

        /// <summary>
        /// Target orbital velocity
        /// </summary>
        /// <returns>Vessel's orbital velocity</returns>
        public Vector3 GetObtVelocity() => this.vessel.obt_velocity;

        /// <summary>
        /// Target surface velocity
        /// </summary>
        /// <returns>Vessel's surface velocity</returns>
        public Vector3 GetSrfVelocity() => this.vessel.srf_velocity;

        /// <summary>
        /// Target forward vector (-assist.forward as the forward points into the target)
        /// </summary>
        /// <returns>Assist forward vecotr</returns>
        public Vector3 GetFwdVector() => -this.assist.forward;

        /// <summary>
        /// Target vessel
        /// </summary>
        /// <returns>Module's vessel</returns>
        public Vessel GetVessel() => this.vessel;

        /// <summary>
        /// Target name
        /// </summary>
        /// <returns>Assist target name</returns>
        public string GetName() => this.assistName + " target";

        /// <summary>
        /// Target orbit
        /// </summary>
        /// <returns>Vessel's orbit</returns>
        public Orbit GetOrbit() => this.vessel.orbit;

        /// <summary>
        /// Target orbit driver
        /// </summary>
        /// <returns>Vessel's orbit driver</returns>
        public OrbitDriver GetOrbitDriver() => this.vessel.orbitDriver;

        /// <summary>
        /// Targeting mode
        /// </summary>
        /// <returns>Direction, velocity, and orientation</returns>
        public VesselTargetModes GetTargetingMode() => VesselTargetModes.DirectionVelocityAndOrientation;
        #endregion

        #region Overrides
        /// <summary>
        /// Module title
        /// </summary>
        /// <returns>Module title</returns>
        public override string GetModuleTitle() => "Docking Target";

        /// <summary>
        /// Module description
        /// </summary>
        /// <returns>Description string</returns>
        public override string GetInfo() => "Active Docking Target";

        /// <summary>
        /// OnDestroy function
        /// </summary>
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
    }
}
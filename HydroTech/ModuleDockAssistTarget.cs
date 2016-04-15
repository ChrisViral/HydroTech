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

        protected override string ModuleShort
        {
            get { return "Tgt"; }
        }
        #endregion

        #region Methods
        public Transform GetTransform()
        {
            //Change to target transform
            return this.part.transform;
        }

        public Vector3 GetObtVelocity()
        {
            return this.vessel.obt_velocity;
        }

        public Vector3 GetSrfVelocity()
        {
            return this.vessel.srf_velocity;
        }

        public Vector3 GetFwdVector()
        {
            //Change to -assistTransform.fwd
            //Negative because forward points into the target (but the target faces the other way)
            return -this.assistFwd;
        }

        public Vessel GetVessel()
        {
            return this.vessel;
        }

        public string GetName()
        {
            return this.assistName + " target";
        }

        public Orbit GetOrbit()
        {
            return this.vessel.orbit;
        }

        public OrbitDriver GetOrbitDriver()
        {
            return this.vessel.orbitDriver;
        }

        public VesselTargetModes GetTargetingMode()
        {
            return VesselTargetModes.DirectionVelocityAndOrientation;
        }
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
        public override string GetModuleTitle()
        {
            return "Docking Target";
        }

        public override string GetInfo()
        {
            return "Active Docking Target";
        }
        #endregion
    }
}
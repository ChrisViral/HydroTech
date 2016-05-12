using System.Collections.Generic;
using System.Text;
using HydroTech.Autopilots.Calculators;
using HydroTech.Managers;
using HydroTech.Panels;
using HydroTech.Utils;
using UnityEngine;

namespace HydroTech
{
    /// <summary>
    /// Docking camera module
    /// </summary>
    public class ModuleDockAssistCam : ModuleDockAssist, IResourceConsumer
    {
        #region Static properties
        /// <summary>
        /// Current selected camera
        /// </summary>
        private static ModuleDockAssistCam Current
        {
            get { return HydroFlightManager.Instance.DockingAutopilot.Cam; }
            set { HydroFlightManager.Instance.DockingAutopilot.Cam = value; }
        }

        /// <summary>
        /// Current active camera
        /// </summary>
        private static ModuleDockAssistCam ActiveCam { get; set; }
        #endregion

        #region KSPFields
        [KSPField]
        public float camClip = 0.01f;

        [KSPField]
        public float camDefFoV = 60;

        [KSPField]
        public float electricityConsumption = 0.01f;
        #endregion

        #region Properties
        /// <summary>
        /// Camera magnification
        /// </summary>
        public int Mag { get; set; } = 1;

        protected bool isOnActiveVessel;
        /// <summary>
        /// If this camera is on the active vessel
        /// </summary>
        public bool IsOnActiveVessel
        {
            get
            {
                if (!this.vessel.isActiveVessel)
                {
                    if (this.isOnActiveVessel && Current == this)
                    {
                        FlightMainPanel.Instance.DockAssist.ResetHeight();
                        if (this.CamActive) { this.CamActive = false; }
                    }
                    this.isOnActiveVessel = false;
                }
                else { this.isOnActiveVessel = true; }

                return this.isOnActiveVessel;
            }
        }

        protected bool camActive;
        /// <summary>
        /// If the camera is active
        /// </summary>
        public bool CamActive
        {
            get { return this.camActive; }
            set
            {
                if (!this.camActive)
                {
                    if (value)
                    {
                        if (ActiveCam == null) { HydroFlightManager.Instance.CameraManager.SaveCurrent(); }
                        HydroFlightManager.Instance.CameraManager.CamCallback = ShowCamera;
                        ActiveCam = this;
                    }
                }
                else if (!value)
                {
                    HydroFlightManager.Instance.CameraManager.RetrieveLast();
                    this.Mag = 1;
                    ActiveCam = null;
                }

                this.camActive = value;
            }
        }

        /// <summary>
        /// Camera module shorthand
        /// </summary>
        protected override string ModuleShort => "Cam";
        #endregion

        #region Methods
        /// <summary>
        /// Switches to docking camera
        /// </summary>
        public void ShowCamera()
        {
            HydroCameraManager man = HydroFlightManager.Instance.CameraManager;
            man.Target = null;
            man.TransformParent = this.transform;
            man.Position = this.assist.position;
            man.SetLookRotation(this.assist.forward, this.assist.up);
            man.FoV = this.camDefFoV / this.Mag;
            man.NearClipPlane = this.camClip;
        }

        /// <summary>
        /// World/local vector transformation
        /// </summary>
        /// <param name="vec">Vector to transform</param>
        /// <returns>Vector in local coordinates</returns>
        public Vector3 VectorTransform(Vector3 vec) => SwitchTransformCalculator.VectorTransform(vec, this.Right, this.Down, this.Dir);

        /// <summary>
        /// Consumed resources
        /// </summary>
        /// <returns>Electricity</returns>
        public List<PartResourceDefinition> GetConsumedResources() => HTUtils.ElectrictyList;
        #endregion

        #region Functions
        /// <summary>
        /// FixedUpdate function
        /// </summary>
        private void FixedUpdate()
        {
            if (!FlightGlobals.ready || !this.CamActive) { return;}

            if (!CheatOptions.InfiniteElectricity && this.part.RequestResource(HTUtils.ElectricChargeID, this.electricityConsumption * TimeWarp.deltaTime) <= 0)
            {
                this.CamActive = false;
            }
        }
        #endregion

        #region Overrides
        /// <summary>
        /// Module title
        /// </summary>
        /// <returns>Module title</returns>
        public override string GetModuleTitle() => "Docking Camera";

        /// <summary>
        /// Module description
        /// </summary>
        /// <returns>Description string</returns>
        public override string GetInfo()
        {
            StringBuilder sb = new StringBuilder("Active Docking Camera");
            sb.AppendLine("\n\n<b><color=#99ff00ff>Input:</color></b>");
            sb.AppendLine("ElectricCharge");
            sb.Append($"Rate: {this.electricityConsumption:0.###}U/s");
            return sb.ToString();
        }

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
                if (this.CamActive) { this.CamActive = false; }
            }
        }
        #endregion
    }
}
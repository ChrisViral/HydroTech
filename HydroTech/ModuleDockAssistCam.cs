using System.Collections.Generic;
using System.Text;
using HydroTech.Autopilots.Calculators;
using HydroTech.Managers;
using HydroTech.Panels;
using HydroTech.Utils;
using UnityEngine;

namespace HydroTech
{
    public class ModuleDockAssistCam : ModuleDockAssist, IResourceConsumer
    {
        #region Static properties
        private static ModuleDockAssistCam Current
        {
            get { return HydroFlightManager.Instance.DockingAutopilot.Cam; }
            set { HydroFlightManager.Instance.DockingAutopilot.Cam = value; }
        }

        private static ModuleDockAssistCam ActiveCam { get; set; }
        #endregion

        #region KSPFields
        [KSPField]
        public float camClip = 0.01f;

        [KSPField]
        public Vector3 camCrossPos = Vector3.zero;

        [KSPField]
        public float camDefFoV = 60;

        [KSPField]
        public float electricityConsumption = 0.01f;
        #endregion

        #region Properties
        public int Mag { get; set; } = 1;

        protected bool isOnActiveVessel;
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

        public Vector3 CrossPos => this.part.GetComponentCached(ref this.rigidbody).worldCenterOfMass + ReverseTransform(this.camCrossPos);

        protected override string ModuleShort => "Cam";
        #endregion

        #region Methods
        public void ShowCamera()
        {
            HydroCameraManager camMngr = HydroFlightManager.Instance.CameraManager;
            camMngr.Target = null;
            camMngr.TransformParent = this.transform;
            camMngr.Position = this.assistPos;
            camMngr.SetLookRotation(this.assistFwd, this.assistUp);
            camMngr.FoV = this.camDefFoV / this.Mag;
            camMngr.NearClipPlane = this.camClip;
        }

        public Vector3 VectorTransform(Vector3 vec)
        {
            return SwitchTransformCalculator.VectorTransform(vec, this.Right, this.Down, this.Dir);
        }

        public List<PartResourceDefinition> GetConsumedResources()
        {
            return new List<PartResourceDefinition>(1) { HTUtils.Electricity };
        }
        #endregion

        #region Static methods
        public static Vector3 VectorTransform(Vector3 vec, ModuleDockAssistCam mcam)
        {
            return mcam.VectorTransform(vec);
        }
        #endregion

        #region Functions
        private void FixedUpdate()
        {
            if (!FlightGlobals.ready || !this.CamActive) { return;}

            if (!CheatOptions.InfiniteElectricity && this.part.RequestResource(HTUtils.ElectricChargeID, this.electricityConsumption * TimeWarp.deltaTime) <= 0)
            {
                this.CamActive = false;
            }
        }

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

        #region Overrides
        public override string GetModuleTitle()
        {
            return "Docking Camera";
        }

        public override string GetInfo()
        {
            StringBuilder sb = new StringBuilder("Active Docking Camera");
            sb.AppendLine("\n\n<b><color=#99ff00ff>Input:</color></b>");
            sb.AppendLine("ElectricCharge");
            sb.AppendFormat("Rate: {0:0.###}U/s", this.electricityConsumption);
            return sb.ToString();
        }
        #endregion
    }
}
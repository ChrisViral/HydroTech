using HydroTech.Managers;
using HydroTech.Panels;
using UnityEngine;

namespace HydroTech.PartModules
{
    public class ModuleDockAssistCam : ModuleDockAssist
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
        [KSPField(isPersistant = true)]
        public float camClip = 0.01f;

        [KSPField(isPersistant = true)]
        public Vector3 camCrossPos = Vector3.zero;

        [KSPField(isPersistant = true)]
        public float camDefFoV = 60;
        #endregion

        #region Fields
        public int mag = 1;
        #endregion

        #region Properties
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
                        if (this.CamActivate) { this.CamActivate = false; }
                    }
                    this.isOnActiveVessel = false;
                }
                else { this.isOnActiveVessel = true; }

                return this.isOnActiveVessel;
            }
        }

        protected bool camActivate;
        public bool CamActivate
        {
            get { return this.camActivate; }
            set
            {
                if (!this.camActivate && value)
                {
                    if (ActiveCam == null) { HydroFlightManager.Instance.CameraManager.SaveCurrent(); }
                    HydroFlightManager.Instance.CameraManager.CamCallback = ShowCamera;
                    ActiveCam = this;
                }
                else if (this.camActivate && !value)
                {
                    HydroFlightManager.Instance.CameraManager.RetrieveLast();
                    this.mag = 1;
                    ActiveCam = null;
                }
                this.camActivate = value;
            }
        }

        public Vector3 CrossPos
        {
            get { return this.part.GetComponentCached(ref this.rigidbody).worldCenterOfMass + ReverseTransform_PartConfig(this.camCrossPos); }
        }

        public Transform VesselTransform
        {
            get { return this.vessel.ReferenceTransform; }
        }

        protected override string ModuleShort
        {
            get { return "Cam"; }
        }
        #endregion

        #region Methods
        public void ShowCamera()
        {
            HydroFlightManager.Instance.CameraManager.Target = null;
            HydroFlightManager.Instance.CameraManager.TransformParent = this.transform;
            HydroFlightManager.Instance.CameraManager.Position = this.assistPos;
            HydroFlightManager.Instance.CameraManager.SetLookRotation(this.assistFwd, this.assistUp);
            HydroFlightManager.Instance.CameraManager.FoV = this.camDefFoV / this.mag;
            HydroFlightManager.Instance.CameraManager.NearClipPlane = this.camClip;
        }

        public Vector3 VectorTransform(Vector3 vec)
        {
            return SwitchTransformCalculator.VectorTransform(vec, this.Right, this.Down, this.Dir);
        }
        #endregion

        #region Static methods
        public static Vector3 VectorTransform(Vector3 vec, ModuleDockAssistCam mcam)
        {
            return mcam.VectorTransform(vec);
        }
        #endregion

        #region Functions
        public void FixedUpdate()
        {
            if (!FlightGlobals.ready || !this.CamActivate) { return;}

            this.part.RequestResource("ElectricCharge", 0.002 * TimeWarp.deltaTime);
        }

        public void OnDestroy()
        {
            if (!HighLogic.LoadedSceneIsFlight || this != Current) { return; }

            Current = null;
            FlightMainPanel.Instance.DockAssist.ResetHeight();
            if (this.CamActivate) { this.CamActivate = false; }
        }
        #endregion

        #region Overrides
        public override void OnStart(StartState state)
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                if (this.part.name == "HydroTech.DA.1m") { this.camCrossPos.Set(0, 0.065f, -0.75f); }
                if (this.part.name == "HydroTech.DA.2m")
                {
                    this.assistPos.Set(0, -0.025f, -1.375f);
                    this.camCrossPos.Set(0, 0.065f, -1.375f);
                }
            }
        }
        #endregion
    }
}
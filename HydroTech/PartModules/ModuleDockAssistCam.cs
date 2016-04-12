using HydroTech.Managers;
using HydroTech.Panels;
using HydroTech.PartModules.Base;
using UnityEngine;

namespace HydroTech.PartModules
{
    public class ModuleDockAssistCam : PartModule, IPartPreview, IDAPartEditorAid
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

        [KSPField(isPersistant = true)]
        public Vector3 camForward = Vector3.forward;
        
        [KSPField(isPersistant = true)]
        public Vector3 camPos = Vector3.zero;

        [KSPField(isPersistant = true)]
        public Vector3 camUp = Vector3.up;

        [KSPField(isPersistant = true)]
        public Vector3 previewForward = -Vector3.forward;

        [KSPField(isPersistant = true)]
        public float previewFoV = 90.0F;

        [KSPField(isPersistant = true)]
        public Vector3 previewPos = Vector3.forward;

        [KSPField(isPersistant = true)]
        public Vector3 previewUp = Vector3.up;
        #endregion

        #region Fields
        private LineRenderer lineDir, lineRight, lineUp;
        private Rigidbody rigidbody;
        public int mag = 1;
        #endregion

        #region Properties
        private ModulePartRename moduleRename;
        public ModulePartRename ModuleRename
        {
            get { return this.moduleRename ?? (this.moduleRename = (ModulePartRename)this.part.Modules["ModulePartRename"]); }
        }

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
                else
                { this.isOnActiveVessel = true; }
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
                    HydroFlightManager.Instance.CameraManager.CamCallback = DoCamera;
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

        public Vector3 RelPos
        {
            get { return SwitchTransformCalculator.VectorTransform(this.Pos - this.vessel.findWorldCenterOfMass(), this.vessel.ReferenceTransform); }
        }

        public Vector3 Dir
        {
            get { return ReverseTransform_PartConfig(this.camForward); }
        }

        public Vector3 Down
        {
            get { return ReverseTransform_PartConfig(-this.camUp); }
        }

        public Vector3 Right
        {
            get { return -Vector3.Cross(this.Down, this.Dir); }
        }

        public Vector3 Pos
        {
            get { return this.part.GetComponentCached(ref this.rigidbody).worldCenterOfMass + ReverseTransform_PartConfig(this.camPos); }
        }

        public Vector3 CrossPos
        {
            get { return this.part.GetComponentCached(ref this.rigidbody).worldCenterOfMass + ReverseTransform_PartConfig(this.camCrossPos); }
        }

        public Transform VesselTransform
        {
            get { return this.vessel.ReferenceTransform; }
        }
        #endregion

        #region Methods
        public void ShowEditorAid()
        {
            this.lineDir.SetWidth(0.01f, 0.01f);
            this.lineDir.SetPosition(0, Vector3.zero);
            this.lineDir.SetPosition(1, this.camForward);
            this.lineUp.SetWidth(0.01f, 0.01f);
            this.lineUp.SetPosition(0, Vector3.zero);
            this.lineUp.SetPosition(1, this.camUp);
            this.lineRight.SetWidth(0.01f, 0.01f);
            this.lineRight.SetPosition(0, Vector3.zero);
            this.lineRight.SetPosition(1, -Vector3.Cross(this.camForward, this.camUp));
        }

        public void HideEditorAid()
        {
            this.lineDir.SetWidth(0, 0);
            this.lineDir.SetPosition(0, Vector3.zero);
            this.lineDir.SetPosition(1, Vector3.zero);
            this.lineUp.SetWidth(0, 0);
            this.lineUp.SetPosition(0, Vector3.zero);
            this.lineUp.SetPosition(1, Vector3.zero);
            this.lineRight.SetWidth(0, 0);
            this.lineRight.SetPosition(0, Vector3.zero);
            this.lineRight.SetPosition(1, Vector3.zero);
        }

        public void DoPreview()
        {
            HydroFlightManager.Instance.CameraManager.Target = null;
            HydroFlightManager.Instance.CameraManager.TransformParent = this.transform;
            HydroFlightManager.Instance.CameraManager.FoV = this.previewFoV;
            HydroFlightManager.Instance.CameraManager.Position = this.previewPos;
            HydroFlightManager.Instance.CameraManager.SetLookRotation(this.previewForward, this.previewUp);
        }

        public void DoCamera()
        {
            HydroFlightManager.Instance.CameraManager.Target = null;
            HydroFlightManager.Instance.CameraManager.TransformParent = this.transform;
            HydroFlightManager.Instance.CameraManager.Position = this.camPos;
            HydroFlightManager.Instance.CameraManager.SetLookRotation(this.camForward, this.camUp);
            HydroFlightManager.Instance.CameraManager.FoV = this.camDefFoV / this.mag;
            HydroFlightManager.Instance.CameraManager.NearClipPlane = this.camClip;
        }

        private Vector3 ReverseTransform_PartConfig(Vector3 vec)
        {
            return SwitchTransformCalculator.ReverseVectorTransform(vec, this.transform.right, this.transform.up, this.transform.forward);
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
                    this.camPos.Set(0, -0.025f, -1.375f);
                    this.camCrossPos.Set(0, 0.065f, -1.375f);
                }
            }
            else if (HighLogic.LoadedSceneIsEditor)
            {
                GameObject obj = new GameObject("DockCamLine", typeof(LineRenderer));
                this.lineDir = obj.GetComponent<LineRenderer>();
                this.lineDir.transform.parent = this.transform;
                this.lineDir.useWorldSpace = false;
                this.lineDir.transform.localPosition = this.camPos;
                this.lineDir.transform.localEulerAngles = Vector3.zero;
                this.lineDir.material = new Material(Shader.Find("Particles/Additive"));
                this.lineDir.SetColors(Color.blue, Color.blue);
                this.lineDir.SetVertexCount(2);

                GameObject obj2 = new GameObject("DockCamLine2", typeof(LineRenderer));
                this.lineUp = obj2.GetComponent<LineRenderer>();
                this.lineUp.transform.parent = this.transform;
                this.lineUp.useWorldSpace = false;
                this.lineUp.transform.localPosition = this.camPos;
                this.lineUp.transform.localEulerAngles = Vector3.zero;
                this.lineUp.material = new Material(Shader.Find("Particles/Additive"));
                this.lineUp.SetColors(Color.green, Color.green);
                this.lineUp.SetVertexCount(2);

                GameObject obj3 = new GameObject("DockCamLine3", typeof(LineRenderer));
                this.lineRight = obj3.GetComponent<LineRenderer>();
                this.lineRight.transform.parent = this.transform;
                this.lineRight.useWorldSpace = false;
                this.lineRight.transform.localPosition = this.camPos;
                this.lineRight.transform.localEulerAngles = Vector3.zero;
                this.lineRight.material = new Material(Shader.Find("Particles/Additive"));
                this.lineRight.SetColors(Color.gray, Color.gray);
                this.lineRight.SetVertexCount(2);

                HideEditorAid();
            }
        }

        public override string ToString()
        {
            return this.ModuleRename.renamed ? this.ModuleRename.partName : this.RelPos.ToString("#0.00");
        }
        #endregion
    }
}
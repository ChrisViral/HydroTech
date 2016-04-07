using HydroTech_FC;
using HydroTech_RCS.Autopilots;
using HydroTech_RCS.Constants;
using HydroTech_RCS.Panels;
using HydroTech_RCS.PartModules.Base;
using UnityEngine;
using HMaths = HydroTech_RCS.Utils.HMaths;
using HydroPartModule = HydroTech_RCS.PartModules.Base.HydroPartModule;

namespace HydroTech_RCS.PartModules
{
    public class ModuleDockAssistCam : HydroPartModule, IPartPreview, IDAPartEditorAid
    {
        #region Static fields
        public static ModuleDockAssistCam activeCam;
        #endregion

        #region Static properties
        protected static APDockAssist DA
        {
            get { return APDockAssist.TheAutopilot; }
        }

        protected static PanelDockAssist Panel
        {
            get { return PanelDockAssist.ThePanel; }
        }

        protected static ModuleDockAssistCam CurCam
        {
            get { return DA.Cam; }
            set { DA.Cam = value; }
        }
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

        // Camera settings learned from Fixed Camera
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
                if (this.vessel != FlightGlobals.ActiveVessel)
                {
                    if (this.isOnActiveVessel && CurCam == this)
                    {
                        Panel.ResetHeight();
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
                    if (activeCam == null) { HydroFlightCameraManager.SaveCurrent(); }
                    HydroFlightCameraManager.SetCallback(DoCamera);
                    activeCam = this;
                }
                else if (this.camActivate && !value)
                {
                    HydroFlightCameraManager.RetrieveLast();
                    this.mag = 1;
                    activeCam = null;
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
            get { return HMaths.CrossProduct(this.Down, this.Dir); }
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
            this.lineRight.SetPosition(1, HMaths.CrossProduct(this.camForward, this.camUp));
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
            HydroFlightCameraManager.SetNullTarget();
            HydroFlightCameraManager.SetTransformParent(this.transform);
            HydroFlightCameraManager.SetFoV(this.previewFoV);
            HydroFlightCameraManager.SetPosition(this.previewPos);
            HydroFlightCameraManager.SetRotation(this.previewForward, this.previewUp);
        }

        public void DoCamera()
        {
            HydroFlightCameraManager.SetNullTarget();
            HydroFlightCameraManager.SetTransformParent(this.transform);
            HydroFlightCameraManager.SetPosition(this.camPos);
            HydroFlightCameraManager.SetRotation(this.camForward, this.camUp);
            HydroFlightCameraManager.SetFoV(this.camDefFoV / this.mag);
            HydroFlightCameraManager.SetNearClipPlane(this.camClip);
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

        #region Overrides
        public override void OnUpdate()
        {
            base.OnUpdate();
            HydroJebCore.OnUpdate(this);
            if (this.CamActivate) { this.part.RequestResource("ElectricCharge", CoreConsts.electricConsumptionCamera * TimeWarp.deltaTime); }
        }

        public override void OnFlightStart()
        {
            base.OnFlightStart();
            if (this.part.name == "HydroTech.DA.1m") { this.camCrossPos.Set(0, 0.065f, -0.75f); }
            if (this.part.name == "HydroTech.DA.2m")
            {
                this.camPos.Set(0, -0.025f, -1.375f);
                this.camCrossPos.Set(0, 0.065f, -1.375f);
            }
        }

        public override void OnDestroy()
        {
            if (!HighLogic.LoadedSceneIsFlight) { return; }
            if (this == CurCam)
            {
                CurCam = null;
                Panel.ResetHeight();
                if (this.CamActivate) { this.CamActivate = false; }
            }
        }

        public override string ToString()
        {
            return this.ModuleRename.Renamed ? this.ModuleRename.nameString : this.RelPos.ToString("#0.00");
        }

        public override void OnStart(StartState state)
        {
            if (state != StartState.Editor) { return; }

            GameObject obj = new GameObject("DockCamLine");
            this.lineDir = obj.AddComponent<LineRenderer>();
            this.lineDir.transform.parent = this.transform;
            this.lineDir.useWorldSpace = false;
            this.lineDir.transform.localPosition = this.camPos;
            this.lineDir.transform.localEulerAngles = Vector3.zero;
            this.lineDir.material = new Material(Shader.Find("Particles/Additive"));
            this.lineDir.SetColors(Color.blue, Color.blue);
            this.lineDir.SetVertexCount(2);
            GameObject obj2 = new GameObject("DockCamLine2");
            this.lineUp = obj2.AddComponent<LineRenderer>();
            this.lineUp.transform.parent = this.transform;
            this.lineUp.useWorldSpace = false;
            this.lineUp.transform.localPosition = this.camPos;
            this.lineUp.transform.localEulerAngles = Vector3.zero;
            this.lineUp.material = new Material(Shader.Find("Particles/Additive"));
            this.lineUp.SetColors(Color.green, Color.green);
            this.lineUp.SetVertexCount(2);
            GameObject obj3 = new GameObject("DockCamLine3");
            this.lineRight = obj3.AddComponent<LineRenderer>();
            this.lineRight.transform.parent = this.transform;
            this.lineRight.useWorldSpace = false;
            this.lineRight.transform.localPosition = this.camPos;
            this.lineRight.transform.localEulerAngles = Vector3.zero;
            this.lineRight.material = new Material(Shader.Find("Particles/Additive"));
            this.lineRight.SetColors(Color.gray, Color.gray);
            this.lineRight.SetVertexCount(2);
            HideEditorAid();
        }
        #endregion
    }
}
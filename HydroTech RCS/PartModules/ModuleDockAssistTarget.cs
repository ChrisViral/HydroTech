using HydroTech_FC;
using HydroTech_RCS.Autopilots;
using HydroTech_RCS.Panels;
using HydroTech_RCS.PartModules.Base;
using UnityEngine;
using HMaths = HydroTech_RCS.Utils.HMaths;
using HydroPartModule = HydroTech_RCS.PartModules.Base.HydroPartModule;

namespace HydroTech_RCS.PartModules
{
    public class ModuleDockAssistTarget : HydroPartModule, IPartPreview, IDAPartEditorAid
    {
        #region Static Properties
        protected static PanelDockAssist Panel
        {
            get { return PanelDockAssist.ThePanel; }
        }

        protected static ModuleDockAssistTarget CurTarget
        {
            get { return APDockAssist.TheAutopilot.target; }
            set { APDockAssist.TheAutopilot.target = value; }
        }
        #endregion

        #region KSPFields
        [KSPField(isPersistant = true)]
        public Vector3 previewForward = Vector3.forward;

        [KSPField(isPersistant = true)]
        public float previewFoV = 90;

        [KSPField(isPersistant = true)]
        public Vector3 previewPos = -Vector3.forward;

        [KSPField(isPersistant = true)]
        public Vector3 previewUp = Vector3.up;

        [KSPField(isPersistant = true)]
        public Vector3 targetForward = Vector3.forward;

        [KSPField(isPersistant = true)]
        public Vector3 targetPos = Vector3.zero;

        [KSPField(isPersistant = true)]
        public Vector3 targetUp = Vector3.up;
        #endregion

        #region Fields
        protected bool isNear;
        private LineRenderer lineDir, lineRight, lineUp;
        #endregion

        #region Properties
        private ModulePartRename moduleRename;
        public ModulePartRename ModuleRename
        {
            get { return this.moduleRename ?? (this.moduleRename = (ModulePartRename)this.part.Modules["ModulePartRename"]); }
        }

        public Vector3 Dir
        {
            get { return ReverseTransform_PartConfig(this.targetForward); }
        }

        public Vector3 Down
        {
            get { return ReverseTransform_PartConfig(-this.targetUp); }
        }

        public Vector3 Right
        {
            get { return HMaths.CrossProduct(this.Down, this.Dir); }
        }

        public Vector3 Pos
        {
            get { return this.part.Rigidbody.worldCenterOfMass + ReverseTransform_PartConfig(this.targetPos); }
        }

        public Vector3 RelPos
        {
            get { return SwitchTransformCalculator.VectorTransform(this.Pos - this.vessel.findWorldCenterOfMass(), this.vessel.ReferenceTransform); }
        }

        public bool IsNear
        {
            get
            {
                if (this.vessel == FlightGlobals.ActiveVessel) // || (vessel.findWorldCenterOfMass() - HydroJebCore.ActiveVessel.CoM).magnitude > Position.MaxDist
                {
                    if (this.isNear && CurTarget == this) { Panel.ResetHeight(); }
                    this.isNear = false;
                }
                else
                { this.isNear = true; }
                return this.isNear;
            }
        }
        #endregion

        #region Methods
        public void ShowEditorAid()
        {
            this.lineDir.SetWidth(0.01f, 0.01f);
            this.lineDir.SetPosition(0, Vector3.zero);
            this.lineDir.SetPosition(1, this.targetForward);
            this.lineUp.SetWidth(0.01f, 0.01f);
            this.lineUp.SetPosition(0, Vector3.zero);
            this.lineUp.SetPosition(1, this.targetUp);
            this.lineRight.SetWidth(0.01f, 0.01f);
            this.lineRight.SetPosition(0, Vector3.zero);
            this.lineRight.SetPosition(1, HMaths.CrossProduct(this.targetForward, this.targetUp));
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
        #endregion

        #region Overrides
        public override void OnUpdate()
        {
            base.OnUpdate();
            HydroJebCore.OnUpdate(this);
        }

        public override void OnFlightStart()
        {
            base.OnFlightStart();
            if (this.part.name == "HydroTech.DA.2m") { this.targetPos.Set(0, 0.07f, 1.375f); }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            if (!HighLogic.LoadedSceneIsFlight) { return; }
            if (this == CurTarget)
            {
                CurTarget = null;
                Panel.ResetHeight();
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
            this.lineDir.transform.localPosition = this.targetPos;
            this.lineDir.transform.localEulerAngles = Vector3.zero;
            this.lineDir.material = new Material(Shader.Find("Particles/Additive"));
            this.lineDir.SetColors(Color.blue, Color.blue);
            this.lineDir.SetVertexCount(2);
            GameObject obj2 = new GameObject("DockCamLine2");
            this.lineUp = obj2.AddComponent<LineRenderer>();
            this.lineUp.transform.parent = this.transform;
            this.lineUp.useWorldSpace = false;
            this.lineUp.transform.localPosition = this.targetPos;
            this.lineUp.transform.localEulerAngles = Vector3.zero;
            this.lineUp.material = new Material(Shader.Find("Particles/Additive"));
            this.lineUp.SetColors(Color.green, Color.green);
            this.lineUp.SetVertexCount(2);
            GameObject obj3 = new GameObject("DockCamLine3");
            this.lineRight = obj3.AddComponent<LineRenderer>();
            this.lineRight.transform.parent = this.transform;
            this.lineRight.useWorldSpace = false;
            this.lineRight.transform.localPosition = this.targetPos;
            this.lineRight.transform.localEulerAngles = Vector3.zero;
            this.lineRight.material = new Material(Shader.Find("Particles/Additive"));
            this.lineRight.SetColors(Color.gray, Color.gray);
            this.lineRight.SetVertexCount(2);
            HideEditorAid();
        }
        #endregion
    }
}
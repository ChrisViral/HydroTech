using HydroTech.Managers;
using HydroTech.Panels;
using HydroTech.PartModules.Base;
using UnityEngine;

namespace HydroTech.PartModules
{
    public class ModuleDockAssistTarget : HydroPartModule, IPartPreview, IDAPartEditorAid
    {
        #region Static Properties
        protected static ModuleDockAssistTarget Current
        {
            get { return HydroFlightManager.Instance.DockingAutopilot.target; }
            set { HydroFlightManager.Instance.DockingAutopilot.target = value; }
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
            get { return -Vector3.Cross(this.Down, this.Dir); }
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
                if (this.vessel.isActiveVessel)
                {
                    if (this.isNear && Current == this) { FlightMainPanel.Instance.DockAssist.ResetHeight(); }
                    this.isNear = false;
                }
                else { this.isNear = true; }
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
            this.lineRight.SetPosition(1, -Vector3.Cross(this.targetForward, this.targetUp));
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
        #endregion

        #region Overrides
        public override void OnFlightStart()
        {
            base.OnFlightStart();
            if (this.part.name == "HydroTech.DA.2m") { this.targetPos.Set(0, 0.07f, 1.375f); }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            if (!HighLogic.LoadedSceneIsFlight) { return; }
            if (this == Current)
            {
                Current = null;
                FlightMainPanel.Instance.DockAssist.ResetHeight();
            }
        }

        public override string ToString()
        {
            return this.ModuleRename.renamed ? this.ModuleRename.partName : this.RelPos.ToString("#0.00");
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
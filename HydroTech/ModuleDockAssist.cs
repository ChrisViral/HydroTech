using System.Linq;
using HydroTech.Autopilots.Calculators;
using HydroTech.Managers;
using HydroTech.Utils;
using UnityEngine;

namespace HydroTech
{
    public abstract class ModuleDockAssist : PartModule, IModuleInfo
    {
        #region Static fields
        private static readonly Color[] colours = { Color.blue, Color.green, Color.red };
        #endregion

        #region KSPFields
        [KSPField(isPersistant = true, guiName = "Name", guiActive = true, guiActiveEditor = true)]
        public string assistName;

        //TODO: use transforms instead of manually typing camera/target positions
        [KSPField]
        public Vector3 assistPos = Vector3.zero;

        [KSPField]
        public Vector3 assistUp = Vector3.up;

        [KSPField]
        public Vector3 assistFwd = Vector3.forward;

        [KSPField]
        public Vector3 previewPos = Vector3.back;

        [KSPField]
        public Vector3 previewUp = Vector3.up;

        [KSPField]
        public Vector3 previewFwd = Vector3.forward;

        [KSPField]
        public float previewFoV = 90;
        #endregion

        #region Fields
        private Rect pos, drag;
        private int id;
        private bool visible, hid;
        private LineRenderer[] lines;
        private Vector3[] directions;
        protected Rigidbody rigidbody;
        internal bool highlight;
        #endregion

        #region Properties
        public Vector3 Dir
        {
            get { return ReverseTransform(this.assistFwd); }
        }

        public Vector3 Down
        {
            get { return ReverseTransform(-this.assistUp); }
        }

        public Vector3 Right
        {
            get { return -Vector3.Cross(this.Down, this.Dir); }
        }

        public Vector3 Pos
        {
            get { return this.part.GetComponentCached(ref this.rigidbody).worldCenterOfMass + ReverseTransform(this.assistPos); }
        }

        public Vector3 RelPos
        {
            get { return SwitchTransformCalculator.VectorTransform(this.Pos - this.vessel.findWorldCenterOfMass(), this.vessel.ReferenceTransform); }
        }

        internal string TempName { get; set; }

        public bool InfoShown { get; set; }

        private bool aidShown;
        public bool AidShown
        {
            get { return this.aidShown; }
            set
            {
                if (value != this.aidShown)
                {
                    if (value) { ShowEditorAid(); }
                    else { HideEditorAid(); }
                    this.aidShown = value;
                }
            }
        }
        #endregion

        #region Abstract properties
        protected abstract string ModuleShort { get; }
        #endregion

        #region KSPEvents
        [KSPEvent(guiName = "Rename", active = true, guiActive = true)]
        public void GUIRename()
        {
            if (!this.visible)
            {
                ModuleDockAssist module = this.vessel.FindPartModulesImplementing<ModuleDockAssist>().FirstOrDefault(m => m.visible);
                if (module != null) { module.visible = false; }
                this.visible = true;
            }
        }
        #endregion

        #region Methods
        public void SetName()
        {
            if (this.assistName != this.TempName)
            {
                this.assistName = this.TempName;
                ScreenMessages.PostScreenMessage("Docking assist renamed", 3, ScreenMessageStyle.UPPER_LEFT);
            }
        }

        private void Window(int id)
        {
            GUI.DragWindow(this.drag);

            GUILayout.BeginVertical();
            this.TempName = GUILayout.TextField(this.TempName);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Ok") && !string.IsNullOrEmpty(this.TempName))
            {
                this.assistName = this.TempName;
                this.visible = false;
            }
            if (GUILayout.Button("Clear"))
            {
                this.TempName = string.Empty;
            }
            if (GUILayout.Button("Cancel"))
            {
                this.visible = false;
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        private void ShowUI()
        {
            this.hid = false;
        }

        private void HideUI()
        {
            this.hid = true;
        }

        protected Vector3 ReverseTransform(Vector3 vec)
        {
            return SwitchTransformCalculator.ReverseVectorTransform(vec, this.transform.right, this.transform.up, this.transform.forward);
        }

        private void CreateLineRenderers()
        {
            Shader shader = Shader.Find("Particles/Additive");
            this.lines = new LineRenderer[3];
            for (int i = 0; i < 3; i++)
            {
                LineRenderer lr = new GameObject(this.ModuleShort + "Assist" + i, typeof(LineRenderer)).GetComponent<LineRenderer>();
                this.lines[i] = lr;
                Color colour = colours[i];
                lr.transform.parent = this.transform;
                lr.useWorldSpace = false;
                lr.transform.localPosition = this.assistPos;
                lr.transform.localEulerAngles = Vector3.zero;
                lr.material = new Material(shader);
                lr.SetColors(colour, colour);
                lr.SetVertexCount(2);
            }
        }

        public void ShowEditorAid()
        {
            for (int i = 0; i < 3; i++)
            {
                LineRenderer lr = this.lines[i];
                lr.SetWidth(0.01f, 0.01f);
                lr.SetPosition(0, Vector3.zero);
                lr.SetPosition(1, this.directions[i]);
            }
        }

        public void HideEditorAid()
        {
            for (int i = 0; i < 3; i++)
            {
                LineRenderer lr = this.lines[i];
                lr.SetWidth(0, 0);
                lr.SetPosition(0, Vector3.zero);
                lr.SetPosition(1, Vector3.zero);
            }
        }

        public void ShowPreview()
        {
            HydroCameraManager camMngr = HydroFlightManager.Instance.CameraManager;
            camMngr.Target = null;
            camMngr.TransformParent = this.transform;
            camMngr.FoV = this.previewFoV;
            camMngr.Position = this.previewPos;
            camMngr.SetLookRotation(this.previewFwd, this.previewUp);
        }

        public Callback<Rect> GetDrawModulePanelCallback()
        {
            return null;
        }

        public string GetPrimaryField()
        {
            return string.Empty;
        }
        #endregion

        #region Abstract methods
        public abstract string GetModuleTitle();
        #endregion

        #region Functions
        private void Update()
        {
            if (FlightGlobals.ready && this.visible && !this.vessel.isActiveVessel)
            {
                this.visible = false;
            }        
        }

        private void OnGUI()
        {
            if (this.visible && !this.hid)
            {
                GUI.skin = GUIUtils.Skin;

                this.pos = KSPUtil.ClampRectToScreen(GUILayout.Window(this.id, this.pos, Window, "Rename Part"));
            }
        }
        #endregion

        #region Overrides
        public override void OnStart(StartState state)
        {
            if (HighLogic.LoadedScene != GameScenes.LOADING)
            {
                this.Fields["assistName"].guiName = this.ModuleShort + " name";
                if (string.IsNullOrEmpty(this.assistName)) { this.assistName = this.part.name.Replace('.', ' '); }
                this.TempName = this.assistName;
            }

            if (HighLogic.LoadedSceneIsEditor)
            {
                this.directions = new Vector3[] { this.assistFwd, this.assistUp, -Vector3.Cross(this.assistFwd, this.assistUp) };
                CreateLineRenderers();
                HideEditorAid();
            }
            else if (HighLogic.LoadedSceneIsFlight)
            {
                this.id = GuidProvider.GetGuid<ModuleDockAssist>();
                this.pos = new Rect(Screen.width * 0.5f, Screen.height * 0.45f, 250, 100);
                this.drag = new Rect(0, 0, 250, 30);
                GameEvents.onShowUI.Add(ShowUI);
                GameEvents.onHideUI.Add(HideUI);
            }
        }

        public override string ToString()
        {
            return this.assistName;
        }

        protected virtual void OnDestroy()
        {
            this.visible = false;
            GameEvents.onShowUI.Remove(ShowUI);
            GameEvents.onHideUI.Remove(HideUI);
        }
        #endregion
    }
}
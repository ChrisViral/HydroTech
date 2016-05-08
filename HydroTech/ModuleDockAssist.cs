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
        public string assistTransformName;

        [KSPField]
        public string previewTransformName;

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
        protected Transform assist, preview;
        internal bool highlight;
        #endregion

        #region Properties
        public Vector3 Dir => this.assist.forward;

        public Vector3 Down => -this.assist.up;

        public Vector3 Right => this.assist.right;

        public Vector3 Pos => this.assist.position;

        public Vector3 RelPos => SwitchTransformCalculator.VectorTransform(this.Pos - this.vessel.findWorldCenterOfMass(), this.vessel.ReferenceTransform);

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
            if (!string.IsNullOrEmpty(this.TempName) && this.assistName != this.TempName)
            {
                this.assistName = this.TempName;
                ScreenMessages.PostScreenMessage("Docking assist renamed", 3, ScreenMessageStyle.UPPER_RIGHT);
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

        private void ShowUI() => this.hid = false;

        private void HideUI() => this.hid = true;

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
                lr.transform.localPosition = this.assist.localPosition;
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
            camMngr.Position = this.preview.position;
            camMngr.SetLookRotation(this.preview.forward, this.preview.up);
        }

        public Callback<Rect> GetDrawModulePanelCallback() => null;

        public string GetPrimaryField() => string.Empty;
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
                this.assist = this.part.FindModelTransform(this.assistTransformName);
                this.preview = this.part.FindModelTransform(this.previewTransformName);
            }

            if (HighLogic.LoadedSceneIsEditor)
            {
                this.directions = new Vector3[] { this.assist.forward, this.assist.up, this.assist.right };
                CreateLineRenderers();
                HideEditorAid();
            }
            else if (HighLogic.LoadedSceneIsFlight)
            {
                this.id = IDProvider.GetID<ModuleDockAssist>();
                this.pos = new Rect(Screen.width * 0.5f, Screen.height * 0.45f, 250, 100);
                this.drag = new Rect(0, 0, 250, 30);
                GameEvents.onShowUI.Add(ShowUI);
                GameEvents.onHideUI.Add(HideUI);
            }
        }

        public override string ToString() => this.assistName;

        protected virtual void OnDestroy()
        {
            this.visible = false;
            GameEvents.onShowUI.Remove(ShowUI);
            GameEvents.onHideUI.Remove(HideUI);
        }
        #endregion
    }
}
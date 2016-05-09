using System.Linq;
using HydroTech.Autopilots.Calculators;
using HydroTech.Managers;
using HydroTech.Utils;
using UnityEngine;

namespace HydroTech
{
    /// <summary>
    /// Docking assist base class
    /// </summary>
    public abstract class ModuleDockAssist : PartModule, IModuleInfo
    {
        #region Static fields
        private static readonly Color[] colours = { Color.blue, Color.green, Color.red };   //Editor assists colours
        private static readonly Shader shader = Shader.Find("Particles/Additive");          //Editor assists shader
        #endregion

        #region KSPFields
        [KSPField(isPersistant = true, guiName = "Name", guiActive = true, guiActiveEditor = true)]
        public string assistName;
        
        [KSPField]
        public string assistTransformName, previewTransformName;

        [KSPField]
        public float previewFoV = 90;
        #endregion

        #region Fields
        //GUI stuff
        private Rect pos, drag;
        private int id;
        private bool visible, hid;

        //Editor assists
        private LineRenderer[] lines;
        private Vector3[] directions;
        internal bool highlight;

        //Flight stuff
        protected Rigidbody rigidbody;
        protected Transform assist, preview;
        #endregion

        #region Properties
        /// <summary>
        /// Assist world forward vector
        /// </summary>
        public Vector3 Dir => this.assist.forward;

        /// <summary>
        /// Assist world down vector
        /// </summary>
        public Vector3 Down => -this.assist.up;

        /// <summary>
        /// Assist world right vector
        /// </summary>
        public Vector3 Right => this.assist.right;

        /// <summary>
        /// Assist world position
        /// </summary>
        public Vector3 Pos => this.assist.position;

        /// <summary>
        /// Assist local vessel position
        /// </summary>
        public Vector3 RelPos => SwitchTransformCalculator.VectorTransform(this.Pos - this.vessel.findWorldCenterOfMass(), this.vessel.ReferenceTransform);

        /// <summary>
        /// Temporary name edit GUI field
        /// </summary>
        internal string TempName { get; set; }

        /// <summary>
        /// GUI toggle state
        /// </summary>
        public bool InfoShown { get; set; }

        private bool aidShown;
        /// <summary>
        /// Editor assist aid state
        /// </summary>
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
        /// <summary>
        /// Module type shorthand
        /// </summary>
        protected abstract string ModuleShort { get; }
        #endregion

        #region KSPEvents
        /// <summary>
        /// Shows rename window in flight
        /// </summary>
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
        /// <summary>
        /// Sets assist name
        /// </summary>
        public void SetName()
        {
            if (!string.IsNullOrEmpty(this.TempName) && this.assistName != this.TempName)
            {
                this.assistName = this.TempName;
                ScreenMessages.PostScreenMessage("Docking assist renamed", 3, ScreenMessageStyle.UPPER_RIGHT);
            }
        }

        /// <summary>
        /// Flight name editing
        /// </summary>
        /// <param name="id">Window ID</param>
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

        /// <summary>
        /// Shows UI in flight (F2)
        /// </summary>
        private void ShowUI() => this.hid = false;

        /// <summary>
        /// Hides UI in flight (F2)
        /// </summary>
        private void HideUI() => this.hid = true;

        /// <summary>
        /// Initiates the editor LineRenderer assists
        /// </summary>
        private void CreateLineRenderers()
        {
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

        /// <summary>
        /// Shows editor aid
        /// </summary>
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

        /// <summary>
        /// Hides editor aid
        /// </summary>
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

        /// <summary>
        /// Shows preview camera
        /// </summary>
        public void ShowPreview()
        {
            HydroFlightManager.Instance.CameraManager.Target = null;
            HydroFlightManager.Instance.CameraManager.TransformParent = this.transform;
            HydroFlightManager.Instance.CameraManager.FoV = this.previewFoV;
            HydroFlightManager.Instance.CameraManager.Position = this.preview.position;
            HydroFlightManager.Instance.CameraManager.SetLookRotation(this.preview.forward, this.preview.up);
        }

        /// <summary>
        /// Unused
        /// </summary>
        /// <returns>null</returns>
        public Callback<Rect> GetDrawModulePanelCallback() => null;

        /// <summary>
        /// Unused
        /// </summary>
        /// <returns>Empty string</returns>
        public string GetPrimaryField() => string.Empty;
        #endregion

        #region Abstract methods
        /// <summary>
        /// Forces IModuleInfo.GetModuleTitle() implementation in child classes
        /// </summary>
        /// <returns>Module title</returns>
        public abstract string GetModuleTitle();
        #endregion

        #region Functions
        /// <summary>
        /// Update function
        /// </summary>
        private void Update()
        {
            if (FlightGlobals.ready && this.visible && !this.vessel.isActiveVessel)
            {
                this.visible = false;
            }        
        }
        
        /// <summary>
        /// OnGUI function
        /// </summary>
        private void OnGUI()
        {
            if (this.visible && !this.hid)
            {
                GUI.skin = GUIUtils.Skin;
                this.pos = GUIUtils.ClampedWindow(this.id, this.pos, Window, "Rename Part");
            }
        }
        #endregion

        #region Overrides
        /// <summary>
        /// Initializes PartModule
        /// </summary>
        /// <param name="state">Current starting state</param>
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
                this.directions = new[] { this.assist.forward, this.assist.up, this.assist.right };
                CreateLineRenderers();
                HideEditorAid();
            }
            else if (HighLogic.LoadedSceneIsFlight)
            {
                this.id = GUIUtils.GetID<ModuleDockAssist>();
                this.pos = new Rect(Screen.width * 0.5f, Screen.height * 0.45f, 250, 100);
                this.drag = new Rect(0, 0, 250, 30);
                GameEvents.onShowUI.Add(ShowUI);
                GameEvents.onHideUI.Add(HideUI);
            }
        }

        /// <summary>
        /// Module name
        /// </summary>
        /// <returns>The modules name identifier</returns>
        public override string ToString() => this.assistName;

        /// <summary>
        /// OnDestroy function
        /// </summary>
        protected virtual void OnDestroy()
        {
            this.visible = false;
            GameEvents.onShowUI.Remove(ShowUI);
            GameEvents.onHideUI.Remove(HideUI);
        }
        #endregion
    }
}
using HydroTech.Autopilots.Calculators;
using HydroTech.Constants;
using HydroTech.Managers;
using HydroTech.Storage;
using HydroTech.Utils;
using UnityEngine;

namespace HydroTech.Panels
{
    public class PanelRCSThrustInfo : Panel
    {
        #region Fields
        [HydroSLNodeInfo(name = "PANELEDITOR"), HydroSLField(saveName = "Minimized")]
        public bool editorHide;        

        [HydroSLNodeInfo(name = "PANELEDITOR"), HydroSLNodeInfo(i = 1, name = "SETTINGS"), HydroSLField(saveName = "ShowRotation")]
        public bool showRotation = true;

        [HydroSLNodeInfo(name = "PANELEDITOR"), HydroSLField(saveName = "WindowPos", cmd = CMD.RECT_TOP_LEFT)]
        public Rect windowRectEditor;

        private readonly bool editor;
        private bool panelShownEditor;
        #endregion

        #region Properties
        protected RCSCalculator ActiveRCS
        {
            get { return this.editor ? HydroEditorManager.Instance.ActiveRCS : HydroFlightManager.Instance.ActiveRCS; }
        }

        public override string PanelTitle
        {
            get { return (this.editor && this.editorHide) ? PanelConsts.rcsInfoEditorHideTitle : PanelConsts.rcsInfoTitle; }
        }

        public override bool PanelShown
        {
            get { return this.editor ? this.panelShownEditor : base.PanelShown; }
            set
            {
                if (this.editor)
                {
                    if (!this.Active) { return; }
                    this.panelShownEditor = value;
                }
                else { base.PanelShown = value; }
            }
        }

        private readonly int id;
        protected override int ID
        {
            get { return this.id; }
        }
        #endregion

        #region Constructor
        public PanelRCSThrustInfo(bool editor)
        {
            this.fileName = new FileName("rcsinfo", "cfg", FileName.panelSaveFolder);
            this.id = GuidProvider.GetGuid<PanelRCSThrustInfo>();
            this.editor = editor;
        }
        #endregion

        #region Methods
        public void ShowInEditor()
        {
            this.Active = true;
            Load();
        }

        public void HideInEditor()
        {
            this.PanelShown = false;
            this.Active = false;
        }

        public void OnEditorUpdate()
        {
            if (this.needSave) { Save(); }
        }
        #endregion

        #region Overrides
        public override void OnUpdate()
        {
            base.OnUpdate();
            if (this.ActiveRCS.AllRcsEnabledChanged) { ResetHeight(); }
        }

        protected override void LoadDefault()
        {
            base.LoadDefault();
            this.panelShownEditor = true;
            this.editorHide = false;
            this.showRotation = true;
        }

        protected override void SetDefaultWindowRect()
        {
            this.windowRect = PanelConsts.rcsInfo;
            this.windowRectEditor = PanelConsts.rcsInfoEditor;
        }

        protected override void WindowGUI(int windowId)
        {
            GUILayout.BeginHorizontal();
            if (this.editor)
            {
                if (GUILayout.Button(this.editorHide ? "Maximize" : "Minimize"))
                {
                    this.editorHide = !this.editorHide;
                    this.needSave = true;
                    this.windowRectEditor.width = this.editorHide ? 100 : 250;
                    this.windowRectEditor.height = 0;
                }
                if (this.editorHide)
                {
                    GUILayout.EndHorizontal();
                    GUI.DragWindow();
                    return;
                }
            }
            if (GUILayout.Button("Rotation", this.showRotation ? GUIUtils.ButtonStyle(Color.green) : GUIUtils.Skin.button))
            {
                this.showRotation = true;
                this.needSave = true;
            }
            if (GUILayout.Button("Translation", this.showRotation ? GUIUtils.Skin.button : GUIUtils.ButtonStyle(Color.green)))
            {
                this.showRotation = false;
                this.needSave = true;
            }
            GUILayout.EndHorizontal();
            if (this.showRotation)
            {
                GUILayout.Label(string.Format("Max torque ({0}) and\nangular acceleration ({1})", UnitConsts.torque, UnitConsts.angularAcc));
                GUILayout.Label(string.Format("Pitch down : {0:#0.00} , {1:#0.00}", this.ActiveRCS.maxTorque.xp, this.ActiveRCS.maxAngularAcc.xp));
                GUILayout.Label(string.Format("Pitch up : {0:#0.00} , {1:#0.00}", this.ActiveRCS.maxTorque.xn, this.ActiveRCS.maxAngularAcc.xn));
                GUILayout.Label(string.Format("yaw left : {0:#0.00} , {1:#0.00}", this.ActiveRCS.maxTorque.yp, this.ActiveRCS.maxAngularAcc.yp));
                GUILayout.Label(string.Format("yaw right : {0:#0.00} , {1:#0.00}", this.ActiveRCS.maxTorque.yn, this.ActiveRCS.maxAngularAcc.yn));
                GUILayout.Label(string.Format("Roll left : {0:#0.00} , {1:#0.00}", this.ActiveRCS.maxTorque.zp, this.ActiveRCS.maxAngularAcc.zp));
                GUILayout.Label(string.Format("Roll right : {0:#0.00} , {1:#0.00}", this.ActiveRCS.maxTorque.zn, this.ActiveRCS.maxAngularAcc.zn));
            }
            else
            {
                GUILayout.Label(string.Format("Max thrust ({0}) and\nacceleration ({1})", UnitConsts.force, UnitConsts.acceleration));
                GUILayout.Label(string.Format("Translate left : {0:#0.00} , {1:#0.00}", this.ActiveRCS.maxForce.xp, this.ActiveRCS.maxAcc.xp));
                GUILayout.Label(string.Format("Translate right : {0:#0.00} , {1:#0.00}", this.ActiveRCS.maxForce.xn, this.ActiveRCS.maxAcc.xn));
                GUILayout.Label(string.Format("Translate up : {0:#0.00} , {1:#0.00}", this.ActiveRCS.maxForce.yp, this.ActiveRCS.maxAcc.yp));
                GUILayout.Label(string.Format("Translate down : {0:#0.00} , {1:#0.00}", this.ActiveRCS.maxForce.yn, this.ActiveRCS.maxAcc.yn));
                GUILayout.Label(string.Format("Translate backward : {0:#0.00} , {1:#0.00}", this.ActiveRCS.maxForce.zp, this.ActiveRCS.maxAcc.zp));
                GUILayout.Label(string.Format("Translate forward : {0:#0.00} , {1:#0.00}", this.ActiveRCS.maxForce.zn, this.ActiveRCS.maxAcc.zn));
            }
            if (!this.editor && !this.ActiveRCS.AllRcsEnabled)
            {
                GUILayout.Label("Some RCS thrusters are not enabled.", GUIUtils.ColouredLabel(Color.red));
                if (GUILayout.Button("Enable all")) { this.ActiveRCS.EnableAllRcs(); }
            }
            GUI.DragWindow();
        }

        public override void DrawGUI()
        {
            if (this.editor)
            {
                GUI.skin = HighLogic.Skin;
                Rect newWindowRect = GUILayout.Window(this.id, this.windowRectEditor, WindowGUI, this.PanelTitle);
                if (newWindowRect.yMin != this.windowRectEditor.yMin || newWindowRect.yMin != this.windowRectEditor.yMin) { this.needSave = true; }
                this.windowRectEditor = newWindowRect;
            }
            else { base.DrawGUI(); }
        }
        #endregion
    }
}
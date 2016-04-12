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
        #region Static Properties
        protected static RCSCalculator ActiveRCS
        {
            get { return HydroFlightManager.Instance.ActiveRCS; }
        }
        #endregion

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
                    if (value != this.panelShownEditor)
                    {
                        if (value) { AddPanel(); }
                        else { RemovePanel(); }
                    }
                    this.panelShownEditor = value;
                }
                else { base.PanelShown = value; }
            }
        }
        #endregion

        #region Constructor
        public PanelRCSThrustInfo(bool editor)
        {
            this.fileName = new FileName("rcsinfo", "cfg", FileName.panelSaveFolder);
            this.editor = editor;
        }
        #endregion

        #region Methods
        public void ShowInEditor()
        {
            this.Active = true;
            Load();
            AddPanel();
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
            if (ActiveRCS.AllRcsEnabledChanged) { ResetHeight(); }
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
                GUILayout.Label(string.Format("Pitch down : {0:#0.00} , {1:#0.00}", ActiveRCS.maxTorque.xp, ActiveRCS.maxAngularAcc.xp));
                GUILayout.Label(string.Format("Pitch up : {0:#0.00} , {1:#0.00}", ActiveRCS.maxTorque.xn, ActiveRCS.maxAngularAcc.xn));
                GUILayout.Label(string.Format("yaw left : {0:#0.00} , {1:#0.00}", ActiveRCS.maxTorque.yp, ActiveRCS.maxAngularAcc.yp));
                GUILayout.Label(string.Format("yaw right : {0:#0.00} , {1:#0.00}", ActiveRCS.maxTorque.yn, ActiveRCS.maxAngularAcc.yn));
                GUILayout.Label(string.Format("Roll left : {0:#0.00} , {1:#0.00}", ActiveRCS.maxTorque.zp, ActiveRCS.maxAngularAcc.zp));
                GUILayout.Label(string.Format("Roll right : {0:#0.00} , {1:#0.00}", ActiveRCS.maxTorque.zn, ActiveRCS.maxAngularAcc.zn));
            }
            else
            {
                GUILayout.Label(string.Format("Max thrust ({0}) and\nacceleration ({1})", UnitConsts.force, UnitConsts.acceleration));
                GUILayout.Label(string.Format("Translate left : {0:#0.00} , {1:#0.00}", ActiveRCS.maxForce.xp, ActiveRCS.maxAcc.xp));
                GUILayout.Label(string.Format("Translate right : {0:#0.00} , {1:#0.00}", ActiveRCS.maxForce.xn, ActiveRCS.maxAcc.xn));
                GUILayout.Label(string.Format("Translate up : {0:#0.00} , {1:#0.00}", ActiveRCS.maxForce.yp, ActiveRCS.maxAcc.yp));
                GUILayout.Label(string.Format("Translate down : {0:#0.00} , {1:#0.00}", ActiveRCS.maxForce.yn, ActiveRCS.maxAcc.yn));
                GUILayout.Label(string.Format("Translate backward : {0:#0.00} , {1:#0.00}", ActiveRCS.maxForce.zp, ActiveRCS.maxAcc.zp));
                GUILayout.Label(string.Format("Translate forward : {0:#0.00} , {1:#0.00}", ActiveRCS.maxForce.zn, ActiveRCS.maxAcc.zn));
            }
            if (!this.editor && !ActiveRCS.AllRcsEnabled)
            {
                GUILayout.Label("Some RCS thrusters are not enabled.", GUIUtils.ColouredLabel(Color.red));
                if (GUILayout.Button("Enable all")) { ActiveRCS.EnableAllRcs(); }
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
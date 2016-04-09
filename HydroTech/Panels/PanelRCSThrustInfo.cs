using HydroTech.Autopilots.Calculators;
using HydroTech.Constants;
using HydroTech.Storage;
using HydroTech.Utils;
using UnityEngine;

namespace HydroTech.Panels
{
    public class PanelRCSThrustInfo : Panel, IPanelEditor
    {
        #region Static Properties
        public static PanelRCSThrustInfo ThePanel
        {
            get { return (PanelRCSThrustInfo)HydroJebCore.panels[CoreConsts.rcsInfo]; }
        }

        protected static RCSCalculator TheCalculator
        {
            get { return HydroJebCore.activeVesselRcs; }
        }
        #endregion

        #region Fields
        [HydroSLNodeInfo(name = "PANELEDITOR"), HydroSLField(saveName = "Minimized")]
        public bool editorHide;        

        [HydroSLNodeInfo(name = "PANELEDITOR"), HydroSLNodeInfo(i = 1, name = "SETTINGS"), HydroSLField(saveName = "ShowRotation")]
        public bool showRotation = true;

        [HydroSLNodeInfo(name = "PANELEDITOR"), HydroSLField(saveName = "WindowPos", cmd = CMD.RECT_TOP_LEFT)]
        public Rect windowRectEditor;

        protected bool editor;
        protected bool panelShownEditor;
        #endregion

        #region Properties
        protected override int PanelID
        {
            get { return CoreConsts.rcsInfo; }
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
        public PanelRCSThrustInfo()
        {
            this.fileName = new FileName("rcsinfo", "cfg", HydroJebCore.panelSaveFolder);
        }
        #endregion

        #region Methods
        public void ShowInEditor()
        {
            this.Active = true;
            this.editor = true;
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
            if (TheCalculator.AllRcsEnabledChanged) { ResetHeight(); }
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
                GUILayout.Label(string.Format("Max torque ({0}) and\nangular acceleration ({1})", GeneralConsts.torque, GeneralConsts.angularAcc));
                GUILayout.Label(string.Format("Pitch down : {0:#0.00} , {1:#0.00}", TheCalculator.maxTorque.xp, TheCalculator.maxAngularAcc.xp));
                GUILayout.Label(string.Format("Pitch up : {0:#0.00} , {1:#0.00}", TheCalculator.maxTorque.xn, TheCalculator.maxAngularAcc.xn));
                GUILayout.Label(string.Format("yaw left : {0:#0.00} , {1:#0.00}", TheCalculator.maxTorque.yp, TheCalculator.maxAngularAcc.yp));
                GUILayout.Label(string.Format("yaw right : {0:#0.00} , {1:#0.00}", TheCalculator.maxTorque.yn, TheCalculator.maxAngularAcc.yn));
                GUILayout.Label(string.Format("Roll left : {0:#0.00} , {1:#0.00}", TheCalculator.maxTorque.zp, TheCalculator.maxAngularAcc.zp));
                GUILayout.Label(string.Format("Roll right : {0:#0.00} , {1:#0.00}", TheCalculator.maxTorque.zn, TheCalculator.maxAngularAcc.zn));
            }
            else
            {
                GUILayout.Label(string.Format("Max thrust ({0}) and\nacceleration ({1})", GeneralConsts.force, GeneralConsts.acceleration));
                GUILayout.Label(string.Format("Translate left : {0:#0.00} , {1:#0.00}", TheCalculator.maxForce.xp, TheCalculator.maxAcc.xp));
                GUILayout.Label(string.Format("Translate right : {0:#0.00} , {1:#0.00}", TheCalculator.maxForce.xn, TheCalculator.maxAcc.xn));
                GUILayout.Label(string.Format("Translate up : {0:#0.00} , {1:#0.00}", TheCalculator.maxForce.yp, TheCalculator.maxAcc.yp));
                GUILayout.Label(string.Format("Translate down : {0:#0.00} , {1:#0.00}", TheCalculator.maxForce.yn, TheCalculator.maxAcc.yn));
                GUILayout.Label(string.Format("Translate backward : {0:#0.00} , {1:#0.00}", TheCalculator.maxForce.zp, TheCalculator.maxAcc.zp));
                GUILayout.Label(string.Format("Translate forward : {0:#0.00} , {1:#0.00}", TheCalculator.maxForce.zn, TheCalculator.maxAcc.zn));
            }
            if (!this.editor && !TheCalculator.AllRcsEnabled)
            {
                GUILayout.Label("Some RCS thrusters are not enabled.", GUIUtils.ColouredLabel(Color.red));
                if (GUILayout.Button("Enable all")) { TheCalculator.EnableAllRcs(); }
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
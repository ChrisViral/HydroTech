using HydroTech.Autopilots.Calculators;
using HydroTech.Managers;
using HydroTech.Utils;
using UnityEngine;

namespace HydroTech.Panels
{
    public class PanelRCSThrustInfo : Panel
    {
        #region Fields
        public bool editorHide;              
        public bool showRotation = true;        
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
            get { return this.editor && this.editorHide ? "RCS" : "RCS Info"; }
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
            this.id = GuidProvider.GetGuid<PanelRCSThrustInfo>();
            this.editor = editor;
        }
        #endregion

        #region Methods
        public void ShowInEditor()
        {
            this.Active = true;
        }

        public void HideInEditor()
        {
            this.PanelShown = false;
            this.Active = false;
        }
        #endregion

        #region Overrides
        public override void OnUpdate()
        {
            base.OnUpdate();
            if (this.ActiveRCS.AllRCSEnabledChanged) { ResetHeight(); }
        }

        protected override void SetDefaultWindowRect()
        {
            this.windowRect = new Rect(747, 80, 250, 280);
            this.windowRectEditor = new Rect((Screen.width * 0.95f) - 250, 80, 250, 0);
        }

        protected override void WindowGUI(int windowId)
        {
            GUILayout.BeginHorizontal();
            if (this.editor)
            {
                if (GUILayout.Button(this.editorHide ? "Maximize" : "Minimize"))
                {
                    this.editorHide = !this.editorHide;
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
            }
            if (GUILayout.Button("Translation", this.showRotation ? GUIUtils.Skin.button : GUIUtils.ButtonStyle(Color.green)))
            {
                this.showRotation = false;
            }
            GUILayout.EndHorizontal();
            if (this.showRotation)
            {
                GUILayout.Label("Max torque (rad/s²) and\nangular acceleration (rad/s²)");
                GUILayout.Label(string.Format("Pitch down : {0:#0.00} , {1:#0.00}", this.ActiveRCS.maxTorque.xp, this.ActiveRCS.maxAngularAcc.xp));
                GUILayout.Label(string.Format("Pitch up : {0:#0.00} , {1:#0.00}", this.ActiveRCS.maxTorque.xn, this.ActiveRCS.maxAngularAcc.xn));
                GUILayout.Label(string.Format("yaw left : {0:#0.00} , {1:#0.00}", this.ActiveRCS.maxTorque.yp, this.ActiveRCS.maxAngularAcc.yp));
                GUILayout.Label(string.Format("yaw right : {0:#0.00} , {1:#0.00}", this.ActiveRCS.maxTorque.yn, this.ActiveRCS.maxAngularAcc.yn));
                GUILayout.Label(string.Format("Roll left : {0:#0.00} , {1:#0.00}", this.ActiveRCS.maxTorque.zp, this.ActiveRCS.maxAngularAcc.zp));
                GUILayout.Label(string.Format("Roll right : {0:#0.00} , {1:#0.00}", this.ActiveRCS.maxTorque.zn, this.ActiveRCS.maxAngularAcc.zn));
            }
            else
            {
                GUILayout.Label("Max thrust (N) and\nacceleration (m/s²)");
                GUILayout.Label(string.Format("Translate left : {0:#0.00} , {1:#0.00}", this.ActiveRCS.maxForce.xp, this.ActiveRCS.maxAcc.xp));
                GUILayout.Label(string.Format("Translate right : {0:#0.00} , {1:#0.00}", this.ActiveRCS.maxForce.xn, this.ActiveRCS.maxAcc.xn));
                GUILayout.Label(string.Format("Translate up : {0:#0.00} , {1:#0.00}", this.ActiveRCS.maxForce.yp, this.ActiveRCS.maxAcc.yp));
                GUILayout.Label(string.Format("Translate down : {0:#0.00} , {1:#0.00}", this.ActiveRCS.maxForce.yn, this.ActiveRCS.maxAcc.yn));
                GUILayout.Label(string.Format("Translate backward : {0:#0.00} , {1:#0.00}", this.ActiveRCS.maxForce.zp, this.ActiveRCS.maxAcc.zp));
                GUILayout.Label(string.Format("Translate forward : {0:#0.00} , {1:#0.00}", this.ActiveRCS.maxForce.zn, this.ActiveRCS.maxAcc.zn));
            }
            if (!this.editor && !this.ActiveRCS.AllRCSEnabled)
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
                this.windowRectEditor = KSPUtil.ClampRectToScreen(GUILayout.Window(this.id, this.windowRectEditor, WindowGUI, this.PanelTitle));
            }
            else { base.DrawGUI(); }
        }
        #endregion
    }
}
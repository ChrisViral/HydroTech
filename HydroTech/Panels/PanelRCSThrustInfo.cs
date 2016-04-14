using HydroTech.Autopilots.Calculators;
using HydroTech.Managers;
using HydroTech.Utils;
using UnityEngine;

namespace HydroTech.Panels
{
    public class PanelRCSThrustInfo : Panel
    {
        #region Fields         
        public bool showRotation = true;
        private readonly bool editor;
        #endregion

        #region Properties
        protected RCSCalculator ActiveRCS
        {
            get { return this.editor ? HydroEditorManager.Instance.ActiveRCS : HydroFlightManager.Instance.ActiveRCS; }
        }
        #endregion

        #region Constructor
        public PanelRCSThrustInfo(bool editor) : base(editor ? new Rect((Screen.width * 0.95f) - 250, 80, 250, 0) : new Rect(747, 80, 250, 280), GuidProvider.GetGuid<PanelRCSThrustInfo>(), "RCS Info")
        {
            this.editor = editor;
        }
        #endregion

        #region Methods
        public void ShowInEditor()
        {
            this.Visible = true;
        }

        public void HideInEditor()
        {
            this.Visible = false;
        }
        #endregion

        #region Overrides
        public override void OnUpdate()
        {
            base.OnUpdate();
            if (this.ActiveRCS.AllRCSEnabledChanged) { ResetHeight(); }
        }

        protected override void Window(int id)
        {
            GUI.DragWindow(this.drag);

            GUILayout.BeginHorizontal();
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
        }
        #endregion
    }
}
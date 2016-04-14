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
        protected override void Window(int id)
        {
            GUI.DragWindow(this.drag);

            GUILayout.BeginHorizontal();
            this.showRotation = GUIUtils.TwinToggle(this.showRotation, "Rotation", "Translation", GUI.skin.button);
            GUILayout.EndHorizontal();

            GUILayout.BeginVertical(GUI.skin.box);
            if (this.showRotation)
            {
                Vector6 torque = this.ActiveRCS.maxTorque;
                Vector6 angular = this.ActiveRCS.maxAngularAcc;
                GUILayout.Label("Torque(N·m), angular acc(rad/s²)");
                GUILayout.Label(string.Format("Pitch down: {0:#0.00}, {1:#0.00}", torque.xp, angular.xp));
                GUILayout.Label(string.Format("Pitch up: {0:#0.00}, {1:#0.00}",   torque.xn, angular.xn));
                GUILayout.Label(string.Format("Yaw left: {0:#0.00}, {1:#0.00}",   torque.yp, angular.yp));
                GUILayout.Label(string.Format("Yaw right: {0:#0.00}, {1:#0.00}",  torque.yn, angular.yn));
                GUILayout.Label(string.Format("Roll left: {0:#0.00}, {1:#0.00}",  torque.zp, angular.zp));
                GUILayout.Label(string.Format("Roll right: {0:#0.00}, {1:#0.00}", torque.zn, angular.zn));
            }
            else
            {
                Vector6 thrust = this.ActiveRCS.maxForce;
                Vector6 acc = this.ActiveRCS.maxAcc;
                GUILayout.Label("Thrust(N), acceleration(m/s²)");
                GUILayout.Label(string.Format("Translate left: {0:#0.00}, {1:#0.00}",  thrust.xp, acc.xp));
                GUILayout.Label(string.Format("Translate right: {0:#0.00}, {1:#0.00}", thrust.xn, acc.xn));
                GUILayout.Label(string.Format("Translate up: {0:#0.00}, {1:#0.00}",    thrust.yp, acc.yp));
                GUILayout.Label(string.Format("Translate down: {0:#0.00}, {1:#0.00}",  thrust.yn, acc.yn));
                GUILayout.Label(string.Format("Translate back: {0:#0.00}, {1:#0.00}",  thrust.zp, acc.zp));
                GUILayout.Label(string.Format("Translate fwd: {0:#0.00}, {1:#0.00}",   thrust.zn, acc.zn));
            }
            GUILayout.EndVertical();

            if (!this.editor && !this.ActiveRCS.AllRCSEnabled)
            {
                GUILayout.Label("Some RCS thrusters are disabled.", GUIUtils.ColouredLabel(Color.red));
                if (GUILayout.Button("Enable all"))
                {
                    this.ActiveRCS.EnableAllRcs();
                }
            }
        }
        #endregion
    }
}
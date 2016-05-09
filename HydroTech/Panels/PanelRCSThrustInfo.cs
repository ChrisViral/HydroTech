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
        private readonly RCSCalculator activeRCS;
        #endregion

        #region Properties
        public override string Title => "RCS Info";
        #endregion

        #region Constructor
        public PanelRCSThrustInfo(bool editor) : base(editor ? new Rect((Screen.width * 0.95f) - 250, 80, 250, 0) : new Rect(747, 80, 250, 280), IDProvider.GetID<PanelRCSThrustInfo>())
        {
            this.editor = editor;
            this.activeRCS = this.editor ? HydroEditorManager.Instance.ActiveRCS : HydroFlightManager.Instance.ActiveRCS;
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
                Vector6 torque = this.activeRCS.maxTorque;
                Vector6 angular = this.activeRCS.maxAngularAcc;
                GUILayout.Label("Torque(N·m), angular acc(rad/s²)");
                GUILayout.Label($"Pitch down: {torque.xp:#0.00}, {angular.xp:#0.00}");
                GUILayout.Label($"Pitch up: {torque.xn:#0.00}, {angular.xn:#0.00}");
                GUILayout.Label($"Yaw left: {torque.yp:#0.00}, {angular.yp:#0.00}");
                GUILayout.Label($"Yaw right: {torque.yn:#0.00}, {angular.yn:#0.00}");
                GUILayout.Label($"Roll left: {torque.zp:#0.00}, {angular.zp:#0.00}");
                GUILayout.Label($"Roll right: {torque.zn:#0.00}, {angular.zn:#0.00}");
            }
            else
            {
                Vector6 thrust = this.activeRCS.maxForce;
                Vector6 acc = this.activeRCS.maxAcc;
                GUILayout.Label("Thrust(N), acceleration(m/s²)");
                GUILayout.Label($"Translate left: {thrust.xp:#0.00}, {acc.xp:#0.00}");
                GUILayout.Label($"Translate right: {thrust.xn:#0.00}, {acc.xn:#0.00}");
                GUILayout.Label($"Translate up: {thrust.yp:#0.00}, {acc.yp:#0.00}");
                GUILayout.Label($"Translate down: {thrust.yn:#0.00}, {acc.yn:#0.00}");
                GUILayout.Label($"Translate back: {thrust.zp:#0.00}, {acc.zp:#0.00}");
                GUILayout.Label($"Translate fwd: {thrust.zn:#0.00}, {acc.zn:#0.00}");
            }
            GUILayout.EndVertical();

            if (!this.editor && !this.activeRCS.AllRCSEnabled)
            {
                GUILayout.Label("Some RCS thrusters are disabled.", GUIUtils.ColouredLabel(Color.red));
                if (GUILayout.Button("Enable all"))
                {
                    this.activeRCS.EnableAllRcs();
                }
            }
        }
        #endregion
    }
}
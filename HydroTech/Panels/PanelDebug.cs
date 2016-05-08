#if DEBUG
using System.Collections.Generic;
using HydroTech.Autopilots;
using HydroTech.Managers;
using HydroTech.Utils;
using UnityEngine;

namespace HydroTech.Panels
{
    public class PanelDebug : Panel
    {
        #region Fields
        private bool apStatus, peektop;
        private bool cameraMgr, cameraState;
        private bool ctrlState, flightInput;
        private readonly Dictionary<string, object> watchList = new Dictionary<string, object>();
        #endregion

        #region Properties
        private bool Peektop
        {
            get { return this.cameraMgr && this.peektop; }
            set
            {
                if (this.cameraMgr) { this.peektop = value; }
            }
        }
        #endregion

        #region Constructor
        public PanelDebug() : base(new Rect(Screen.width - 200, 0, 200, 0), IDProvider.GetID<PanelDebug>(), "Debug") { }
        #endregion

        #region Methods
        public void AddWatch(string name, object obj)
        {
            RemoveWatch(name);
            this.watchList.Add(name, obj);
        }

        public void RemoveWatch(string name)
        {
            if (this.watchList.ContainsKey(name)) { this.watchList.Remove(name); }
        }
        #endregion

        #region Overrides
        protected override void Window(int id)
        {
            GUI.DragWindow(this.drag);

            this.ctrlState = GUILayout.Toggle(this.ctrlState, "Control state", GUI.skin.button);
            if (this.ctrlState) { GUILayout.Label(Autopilot.StringCtrlState(FlightGlobals.ActiveVessel.ctrlState)); }

            this.apStatus = GUILayout.Toggle(this.apStatus, "AP Status", GUI.skin.button);
            if (this.apStatus) { GUILayout.Label(Autopilot.StringAllAPStatus()); }

            this.flightInput = GUILayout.Toggle(this.flightInput, "Flight Input", GUI.skin.button);
            if (this.flightInput) { GUILayout.Label(HydroFlightManager.Instance.InputManager.StringList()); }

            this.cameraState = GUILayout.Toggle(this.cameraState, "Camera State", GUI.skin.button);
            if (this.cameraState) { GUILayout.Label(HydroFlightManager.Instance.CameraManager.StringCameraState()); }

            this.cameraMgr = GUILayout.Toggle(this.cameraMgr, "Camera Manager", GUI.skin.button);

            if (this.cameraMgr)
            {
                GUILayout.Label(HydroFlightManager.Instance.CameraManager.StringCameraStack());

                this.Peektop = GUILayout.Toggle(this.Peektop, "Peek Top", GUI.skin.button);
                if (this.Peektop) { GUILayout.Label(HydroFlightManager.Instance.CameraManager.StringTopState()); }
            }

            if (this.watchList.Count != 0)
            {
                GUILayout.Label("Watch list:");
                foreach (KeyValuePair<string, object> pair in this.watchList)
                {
                    GUILayout.Label(pair.Key + " = " + pair.Value);
                }
            }
        }
        #endregion
    }
}
#endif
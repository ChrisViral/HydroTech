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
        public PanelDebug() : base(new Rect(Screen.width - 200, 0, 200, 0), GuidProvider.GetGuid<PanelDebug>(), "Debug") { }
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

            if (GUILayout.Button("Control State"))
            {
                this.ctrlState = !this.ctrlState;
                ResetHeight();
            }
            if (this.ctrlState) { GUILayout.Label(Autopilot.StringCtrlState(FlightGlobals.ActiveVessel.ctrlState)); }

            if (GUILayout.Button("AP Status"))
            {
                this.apStatus = !this.apStatus;
                ResetHeight();
            }
            if (this.apStatus) { GUILayout.Label(Autopilot.StringAllAPStatus()); }

            if (GUILayout.Button("Flight Input"))
            {
                this.flightInput = !this.flightInput;
                ResetHeight();
            }
            if (this.flightInput) { GUILayout.Label(HydroFlightManager.Instance.InputManager.StringList()); }

            if (GUILayout.Button("Camera State"))
            {
                this.cameraState = !this.cameraState;
                ResetHeight();
            }
            if (this.cameraState) { GUILayout.Label(HydroFlightManager.Instance.CameraManager.StringCameraState()); }

            if (GUILayout.Button("Camera Manager"))
            {
                this.cameraMgr = !this.cameraMgr;
                ResetHeight();
            }
            if (this.cameraMgr)
            {
                GUILayout.Label(HydroFlightManager.Instance.CameraManager.StringCameraStack());

                if (GUILayout.Button("Peek top"))
                {
                    this.Peektop = !this.Peektop;
                    ResetHeight();
                }
                if (this.Peektop) { GUILayout.Label(HydroFlightManager.Instance.CameraManager.StringTopState()); }
            }

            GUILayout.Label("Watch list:");
            foreach (KeyValuePair<string, object> pair in this.watchList)
            {
                GUILayout.Label(string.Format("{0} = {1}", pair.Key, pair.Value));
            }
        }
        #endregion
    }
}
#endif
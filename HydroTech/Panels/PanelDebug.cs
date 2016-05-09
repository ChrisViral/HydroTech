#if DEBUG
using System.Collections.Generic;
using HydroTech.Autopilots;
using HydroTech.Managers;
using HydroTech.Utils;
using UnityEngine;

namespace HydroTech.Panels
{
    /// <summary>
    /// Debug panel
    /// </summary>
    public class PanelDebug : Panel
    {
        #region Fields
        //GUI fields
        private bool apStatus, peektop;
        private bool cameraMgr, cameraState;
        private bool ctrlState, flightInput;

        //Watched objects
        private readonly Dictionary<string, object> watchList = new Dictionary<string, object>();
        #endregion

        #region Properties
        /// <summary>
        /// I guess this is related to camera stuff
        /// </summary>
        private bool Peektop
        {
            get { return this.cameraMgr && this.peektop; }
            set { if (this.cameraMgr) { this.peektop = value; } }
        }

        /// <summary>
        /// Panel title
        /// </summary>
        public override string Title => "Debug";
        #endregion

        #region Constructor
        /// <summary>
        /// Initiates window size and ID
        /// </summary>
        public PanelDebug() : base(new Rect(Screen.width - 200, 0, 200, 0), GUIUtils.GetID<PanelDebug>()) { }
        #endregion

        #region Methods
        /// <summary>
        /// Adds an object to observation
        /// </summary>
        /// <param name="name">Name of object</param>
        /// <param name="obj">Object to observe</param>
        public void AddWatch(string name, object obj) => this.watchList[name] = obj;

        /// <summary>
        /// Removes an object from observation
        /// </summary>
        /// <param name="name">Name of object to remove</param>
        public void RemoveWatch(string name) => this.watchList.Remove(name);
        #endregion

        #region Overrides
        /// <summary>
        /// Window function
        /// </summary>
        /// <param name="id"></param>
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
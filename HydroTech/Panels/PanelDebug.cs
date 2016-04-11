#if DEBUG
using System.Collections.Generic;
using HydroTech.Autopilots;
using HydroTech.Constants;
using HydroTech.Managers;
using HydroTech.Storage;
using UnityEngine;

namespace HydroTech.Panels
{
    public class PanelDebug : Panel
    {
        #region Fields
        private bool apStatus;
        private bool cameraMgr;
        private bool cameraState;
        private bool ctrlState;
        private bool flightInput;
        private bool peektop;
        private readonly Dictionary<string, object> watchList = new Dictionary<string, object>();
        #endregion

        #region Properties
        protected override int PanelID
        {
            get { return CoreConsts.debug; }
        }

        public override string PanelTitle
        {
            get { return PanelConsts.debugTitle; }
        }

        protected bool Peektop
        {
            get { return this.cameraMgr && this.peektop; }
            set { if (this.cameraMgr) { this.peektop = value; } }
        }
        #endregion

        #region Constructor
        public PanelDebug()
        {
            this.fileName = new FileName("debug", "cfg", HydroJebCore.panelSaveFolder);
        }
        #endregion

        #region Methods
        public void AddWatch(string name, object obj)
        {
            if (this.watchList.ContainsKey(name)) { this.watchList.Remove(name); }
            this.watchList.Add(name, obj);
        }

        public void RemoveWatch(string name)
        {
            if (this.watchList.ContainsKey(name)) { this.watchList.Remove(name); }
        }
        #endregion

        #region Overrides
        protected override void WindowGUI(int windowId)
        {
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
            if (GUILayout.Button("FlightInput"))
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

            foreach (string name in this.watchList.Keys)
            {
                GUILayout.Label(name + " = " + this.watchList[name]);
            }

            GUI.DragWindow();
        }

        protected override void SetDefaultWindowRect()
        {
            this.windowRect.Set(Screen.width - 200, 0, 200, 0);
        }
        #endregion
    }
}
#endif
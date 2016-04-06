#if DEBUG

using System.Collections.Generic;
using HydroTech_FC;
using HydroTech_RCS.Autopilots;
using HydroTech_RCS.Constants.Core;
using HydroTech_RCS.Constants.Panels;
using UnityEngine;

namespace HydroTech_RCS.Panels
{
    public class PanelDebug : Panel
    {
        protected bool _peektop;
        protected bool apStatus;
        protected bool cameraMgr;
        protected bool cameraState;

        protected bool ctrlState;
        protected bool flightInput;

        protected Dictionary<string, object> watchList = new Dictionary<string, object>();
#if DEBUG
        public static PanelDebug ThePanel
        {
            get { return (PanelDebug)HydroJebCore.panels[PanelIDs.debug]; }
        }
#endif

        protected override int PanelID
        {
            get { return PanelIDs.debug; }
        }

        public override string PanelTitle
        {
            get { return PanelTitles.debug; }
        }

        protected bool Peektop
        {
            get { return this.cameraMgr && this._peektop; }
            set { if (this.cameraMgr) { this._peektop = value; } }
        }

        public PanelDebug()
        {
            this.fileName = new FileName("debug", "cfg", HydroJebCore.panelSaveFolder);
        }

        public void AddWatch(string name, object obj)
        {
            if (this.watchList.ContainsKey(name)) { this.watchList.Remove(name); }
            this.watchList.Add(name, obj);
        }

        public void RemoveWatch(string name)
        {
            if (this.watchList.ContainsKey(name)) { this.watchList.Remove(name); }
        }

        protected override void WindowGUI(int windowId)
        {
            if (GUILayout.Button("Control State"))
            {
                this.ctrlState = !this.ctrlState;
                ResetHeight();
            }
            if (this.ctrlState) { GUILayout.Label(RcsAutopilot.StringCtrlState(GameStates.ActiveVessel.ctrlState)); }
            if (GUILayout.Button("AP Status"))
            {
                this.apStatus = !this.apStatus;
                ResetHeight();
            }
            if (this.apStatus) { GUILayout.Label(RcsAutopilot.StringAllAPStatus()); }
            if (GUILayout.Button("FlightInput"))
            {
                this.flightInput = !this.flightInput;
                ResetHeight();
            }
            if (this.flightInput) { GUILayout.Label(HydroFlightInputManager.StringList()); }
            if (GUILayout.Button("Camera State"))
            {
                this.cameraState = !this.cameraState;
                ResetHeight();
            }
            if (this.cameraState) { GUILayout.Label(HydroFlightCameraManager.StringCameraState()); }
            if (GUILayout.Button("Camera Manager"))
            {
                this.cameraMgr = !this.cameraMgr;
                ResetHeight();
            }
            if (this.cameraMgr)
            {
                GUILayout.Label(HydroFlightCameraManager.StringCameraStack());
                if (GUILayout.Button("Peek top"))
                {
                    this.Peektop = !this.Peektop;
                    ResetHeight();
                }
                if (this.Peektop) { GUILayout.Label(HydroFlightCameraManager.StringTopState()); }
            }

            foreach (string name in this.watchList.Keys) { GUILayout.Label(name + " = " + this.watchList[name]); }

            GUI.DragWindow();
        }

        protected override void SetDefaultWindowRect()
        {
            this.windowRect.Set(Screen.width - 200, 0, 200, 0);
        }
    }
}

#endif
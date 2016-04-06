#if DEBUG

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HydroTech_RCS.Panels
{
    using UnityEngine;
    using HydroTech_FC;
    using Autopilots;
    using Constants.Core;
    using Constants.Panels;

    public class PanelDebug : Panel
    {
#if DEBUG
        static public PanelDebug thePanel { get { return (PanelDebug)HydroJebCore.panels[PanelIDs.Debug]; } }
#endif
        public PanelDebug()
        {
            fileName = new FileName("debug", "cfg", HydroJebCore.PanelSaveFolder);
        }

        protected override int PanelID { get { return PanelIDs.Debug; } }
        public override string PanelTitle { get { return PanelTitles.Debug; } }

        protected bool ctrlState = false;
        protected bool APStatus = false;
        protected bool flightInput = false;
        protected bool cameraState = false;
        protected bool cameraMGR = false;
        protected bool _peektop = false;
        protected bool peektop
        {
            get { return cameraMGR && _peektop; }
            set { if (cameraMGR)_peektop = value; }
        }

        protected Dictionary<string, object> watchList = new Dictionary<string, object>();
        public void AddWatch(string name, object obj)
        {
            if (watchList.ContainsKey(name))
                watchList.Remove(name);
            watchList.Add(name, obj);
        }
        public void RemoveWatch(string name)
        {
            if (watchList.ContainsKey(name))
                watchList.Remove(name);
        }

        protected override void WindowGUI(int WindowID)
        {
            if (GUILayout.Button("Control State"))
            {
                ctrlState = !ctrlState;
                ResetHeight();
            }
            if (ctrlState)
                GUILayout.Label(RCSAutopilot.StringCtrlState(GameStates.ActiveVessel.ctrlState));
            if (GUILayout.Button("AP Status"))
            {
                APStatus = !APStatus;
                ResetHeight();
            }
            if (APStatus)
                GUILayout.Label(RCSAutopilot.StringAllAPStatus());
            if (GUILayout.Button("FlightInput"))
            {
                flightInput = !flightInput;
                ResetHeight();
            }
            if (flightInput)
                GUILayout.Label(HydroFlightInputManager.StringList());
            if (GUILayout.Button("Camera State"))
            {
                cameraState = !cameraState;
                ResetHeight();
            }
            if (cameraState)
                GUILayout.Label(HydroFlightCameraManager.StringCameraState());
            if (GUILayout.Button("Camera Manager"))
            {
                cameraMGR = !cameraMGR;
                ResetHeight();
            }
            if (cameraMGR)
            {
                GUILayout.Label(HydroFlightCameraManager.StringCameraStack());
                if (GUILayout.Button("Peek top"))
                {
                    peektop = !peektop;
                    ResetHeight();
                }
                if (peektop)
                    GUILayout.Label(HydroFlightCameraManager.StringTopState());
            }

            foreach (string name in watchList.Keys)
                GUILayout.Label(name + " = " + watchList[name]);

            GUI.DragWindow();
        }

        protected override void SetDefaultWindowRect()
        {
            windowRect.Set(Screen.width - 200, 0, 200, 0);
        }
    }
}

#endif
using System;
using System.Collections.Generic;
using HydroTech.Autopilots;
using HydroTech.Autopilots.Calculators;
using HydroTech.Constants;
using HydroTech.Data;
using HydroTech.File;
using HydroTech.Managers;
using HydroTech.Panels;
using HydroTech.PartModules;
using HydroTech.Parts;
using HydroTech.Utils;
using UnityEngine;

namespace HydroTech
{
    public static class HydroJebCore
    {
        #region Folders
        public static readonly FileName.Folder autopilotSaveFolder = new FileName.Folder(FileName.hydroTechFolder, "PluginData", "rcsautopilot", "autopilots");
        public static readonly FileName.Folder panelSaveFolder = new FileName.Folder(FileName.hydroTechFolder, "PluginData", "rcsautopilot", "panels");
        #endregion

        #region Constructor
        static HydroJebCore()
        {
            try
            {
                //Autopilots
                autopilots.Add(CoreConsts.apTranslation, new APTranslation());
                autopilots.Add(CoreConsts.apLanding, new APLanding());
                autopilots.Add(CoreConsts.apDock, new APDockAssist());
                autopilots.Add(CoreConsts.precise, new APPreciseControl());

                //Panels
                panels.Add(CoreConsts.main, new PanelMain());
                panels.Add(CoreConsts.mainThrottle, new PanelMainThrottle());
                panels.Add(CoreConsts.rcsInfo, new PanelRCSThrustInfo());
                panels.Add(CoreConsts.preciseControl, new PanelPreciseControl());
                panels.Add(CoreConsts.pTranslation, new PanelTranslation());
                panels.Add(CoreConsts.pLanding, new PanelLanding());
                panels.Add(CoreConsts.pDock, new PanelDockAssist());
#if DEBUG
                panels.Add(CoreConsts.debug, new PanelDebug());
#endif
                //Editor
                panelsEditor.Add(CoreConsts.rcsInfo, new PanelRCSThrustInfo());
                panelsEditor.Add(CoreConsts.pDock, new PanelDockAssistEditor());
            }
            catch (Exception e)
            {
                Debug.LogError("[HydroTech]: Error thrown in HydroJebCore\n" + e.StackTrace);
            }
        }
        #endregion

        #region Core #1: Autopilot (HydroJeb)
        #region Fields
        public static bool isReady = true;
        public static bool electricity;
        private static Color mainBtnColor = Color.green;
        public static HydroPartList jebs = new HydroPartList();
        public static Dictionary<int, RCSAutopilot> autopilots = new Dictionary<int, RCSAutopilot>();
        public static Dictionary<int, Panel> panels = new Dictionary<int, Panel>();

        public static HydroJeb ActiveJeb
        {
            get { return (HydroJeb)jebs.FirstActive; }
        }

        public static bool IsActiveJeb(HydroJeb jeb)
        {
            return ActiveJeb == jeb;
        }

        public static RCSCalculator activeVesselRcs = new RCSCalculator();
        public static HydroPartListEditor jebsEditor = new HydroPartListEditor();
        public static Dictionary<int, IPanelEditor> panelsEditor = new Dictionary<int, IPanelEditor>();
        #endregion

        #region Properties
        private static bool MainPanel
        {
            get { return panels[CoreConsts.main].PanelShown; }
            set { panels[CoreConsts.main].PanelShown = value; }
        }
        #endregion

        #region Methods
        //TODO: change from deprecated RenderingManager to the AppLauncher
        private static void AddMainButton()
        {
            HydroRenderingManager.AddToPostDrawQueue(CoreConsts.renderMgrQueueSpot, DrawGUI);
        }
        private static void RemoveMainButton()
        {
            HydroRenderingManager.RemoveFromPostDrawQueue(CoreConsts.renderMgrQueueSpot, DrawGUI);
        }

        public static void OnEditorUpdate(HydroJeb jeb)
        {
            try
            {
                jebsEditor.OnUpdate();
                activeVesselRcs.OnEditorUpdate();
                foreach (IPanelEditor p in panelsEditor.Values) { p.OnEditorUpdate(); }
            }
            catch (Exception e)
            {
                Debug.LogError("[HydroTech]: Error thrown in HydroJebCore\n" + e.StackTrace);
            }
        }

        public static void OnFlightStart(HydroJeb jeb)
        {
            try
            {
                if (jebsEditor.Count != 0) //Check for jebsEditor
                {
                    jebsEditor.Clear();
                    foreach (IPanelEditor p in panelsEditor.Values) { p.HideInEditor(); }
                }
                jebs.OnStart();
                if (!jebs.Contains(jeb)) { jebs.Add(jeb); }
                if (jeb.vessel != FlightGlobals.ActiveVessel || jebs.CountActive != 1) { return; }
                HydroFlightCameraManager.OnFlightStart();
                foreach (RCSAutopilot ap in autopilots.Values) { ap.OnFlightStart(); }
                foreach (Panel panel in panels.Values) { panel.OnFlightStart(); }
                HydroFlightInputManager.OnFlightStart();
                if (HydroRenderingManager.Contains(CoreConsts.renderMgrQueueSpot)) { RemoveMainButton(); }
                AddMainButton();
                electricity = true;
            }
            catch (Exception e)
            {
                Debug.LogError("[HydroTech]: Error thrown in HydroJebCore\n" + e.StackTrace);
            }
        }

        public static void OnGamePause(HydroJeb jeb)
        {
            try
            {
                if (!jebs.Contains(jeb)) { jebs.Remove(jeb); }
                else
                {
                    return;
                }
                if (jeb.vessel != FlightGlobals.ActiveVessel || jebs.CountActive != 0) { return; }
                foreach (Panel panel in panels.Values) { panel.OnGamePause(); }
                foreach (RCSAutopilot ap in autopilots.Values) { ap.OnGamePause(); }
                RemoveMainButton();
            }
            catch (Exception e)
            {
                Debug.LogError("[HydroTech]: Error thrown in HydroJebCore\n" + e.StackTrace);
            }
        }

        public static void OnGameResume(HydroJeb jeb)
        {
            try
            {
                if (!jebs.Contains(jeb)) { jebs.Add(jeb); }
                if (jeb.vessel != FlightGlobals.ActiveVessel || jebs.CountActive != 1) { return; }
                foreach (RCSAutopilot ap in autopilots.Values) { ap.OnGameResume(); }
                foreach (Panel panel in panels.Values) { panel.OnGameResume(); }
                AddMainButton();
            }
            catch (Exception e)
            {
                Debug.LogError("[HydroTech]: Error thrown in HydroJebCore\n" + e.StackTrace);
            }
        }

        public static void OnPartDestroy(HydroJeb jeb)
        {
            try
            {
                if (HighLogic.LoadedSceneIsEditor)
                {
                    if (jebsEditor.Contains(jeb)) { jebsEditor.Remove(jeb); }
                    else
                    {
                        return;
                    }
                    if (jebsEditor.Count != 0) { return; }
                    foreach (IPanelEditor p in panelsEditor.Values) { p.HideInEditor(); }
                }
                else if (HighLogic.LoadedSceneIsFlight)
                {
                    if (jebs.Contains(jeb)) { jebs.Remove(jeb); }
                    else
                    {
                        return;
                    }
                    if (jebs.CountActive != 0) { return; }
                    foreach (Panel panel in panels.Values) { panel.OnDeactivate(); }
                    foreach (RCSAutopilot ap in autopilots.Values) { ap.OnDeactivate(); }
                    RemoveMainButton();
                }
            }
            catch (Exception e)
            {
                Debug.LogError("[HydroTech]: Error thrown in HydroJebCore\n" + e.StackTrace);
            }
        }

        public static void OnPartStart(HydroJeb jeb)
        {
            try
            {
                if (!HighLogic.LoadedSceneIsEditor) { return; }
                bool clear;
                jebsEditor.OnStart(out clear);
                if (clear) { foreach (IPanelEditor p in panelsEditor.Values) { p.HideInEditor(); } }
                jebsEditor.Add(jeb);
                if (jebsEditor.Count == 1) { foreach (IPanelEditor p in panelsEditor.Values) { p.ShowInEditor(); } }
            }
            catch (Exception e)
            {
                Debug.LogError("[HydroTech]: Error thrown in HydroJebCore\n" + e.StackTrace);
            }
        }

        public static void OnPartUpdate(HydroJeb jeb)
        {
            try
            {
                jebs.OnUpdate();
                if (jebs.CountActive == 0)
                {
                    foreach (Panel panel in panels.Values) { panel.OnDeactivate(); }
                    foreach (RCSAutopilot ap in autopilots.Values) { ap.OnDeactivate(); }
                    if (HydroRenderingManager.Contains(CoreConsts.renderMgrQueueSpot)) { RemoveMainButton(); }
                }
                else
                {
                    if (!IsActiveJeb(jeb)) { return; }
                    activeVesselRcs.OnUpdate(FlightGlobals.ActiveVessel);
                    HydroFlightCameraManager.OnUpdate();
                    HydroFlightInputManager.OnUpdate();
                    foreach (RCSAutopilot ap in autopilots.Values) { ap.OnUpdate(); }
                    foreach (Panel panel in panels.Values) { panel.OnUpdate(); }
                    if (!HydroRenderingManager.Contains(CoreConsts.renderMgrQueueSpot)) { AddMainButton(); }
                    if (!isReady) { mainBtnColor = Color.yellow; }
                    else
                    {
                        mainBtnColor = electricity ? Color.green : Color.red;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("[HydroTech]: Error thrown in HydroJebCore\n" + e.StackTrace);
            }
        }

        private static void DrawGUI()
        {
            GUI.skin = HighLogic.Skin;
            bool mainBtnRespond = electricity && isReady;
            if (GUI.Button(PanelConsts.mainButton, mainBtnRespond ? (MainPanel ? "/\\ HydroJeb /\\" : "\\/ HydroJeb \\/") : "HydroJeb", GUIUtils.ButtonStyle(mainBtnColor)) && mainBtnRespond) { MainPanel = !MainPanel; }
        }
        #endregion
        #endregion

        #region Core #2: Docking Assistant (HydroDockAssistCam, HydroDockAssistTarget)
        #region Fields
        public static HydroPartModuleList dockCams = new HydroPartModuleList();
        public static HydroPartModuleList dockTargets = new HydroPartModuleList();
        #endregion

        #region Methods
        public static void OnUpdate(ModuleDockAssistCam mcam)
        {
            try
            {
                if (!dockCams.Contains(mcam)) { dockCams.Add(mcam); }
                dockCams.OnUpdate();
            }
            catch (Exception e)
            {
                Debug.LogError("[HydroTech]: Error thrown in HydroJebCore\n" + e.StackTrace);
            }
        }

        public static void OnUpdate(ModuleDockAssistTarget mtgt)
        {
            try
            {
                if (!dockTargets.Contains(mtgt)) { dockTargets.Add(mtgt); }
                dockTargets.OnUpdate();
            }
            catch (Exception e)
            {
                Debug.LogError("[HydroTech]: Error thrown in HydroJebCore\n" + e.StackTrace);
            }
        }
        #endregion
        #endregion
    }
}
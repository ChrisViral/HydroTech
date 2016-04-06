﻿using System;
using System.Collections.Generic;
using UnityEngine;
using HydroTech_FC;
using HydroTech_RCS.Autopilots;
using HydroTech_RCS.Panels;
using HydroTech_RCS.Constants.Core;
using HydroTech_RCS.Constants.Panels;
using HydroTech_RCS.Autopilots.Modules;

namespace HydroTech_RCS
{
    public static class HydroJebCore
    {
        #region Constructor
        static HydroJebCore()
        {
            try
            {
                //Autopilots
                autopilots.Add(AutopilotIDs.Translation, new APTranslation());
                autopilots.Add(AutopilotIDs.Landing, new APLanding());
                autopilots.Add(AutopilotIDs.Dock, new APDockAssist());
                autopilots.Add(AutopilotIDs.Precise, new APPreciseControl());

                //Panels
                panels.Add(PanelIDs.Main, new PanelMain());
                panels.Add(PanelIDs.MainThrottle, new PanelMainThrottle());
                panels.Add(PanelIDs.RCSInfo, new PanelRCSThrustInfo());
                panels.Add(PanelIDs.PreciseControl, new PanelPreciseControl());
                panels.Add(PanelIDs.Translation, new PanelTranslation());
                panels.Add(PanelIDs.Landing, new PanelLanding());
                panels.Add(PanelIDs.Dock, new PanelDockAssist());
#if DEBUG
                panels.Add(PanelIDs.Debug, new PanelDebug());
#endif
                //Editor
                panelsEditor.Add(PanelIDs.RCSInfo, new PanelRCSThrustInfo());
                panelsEditor.Add(PanelIDs.Dock, new PanelDockAssistEditorAid());
            }
            catch (Exception e)
            {
                ExceptionHandler(e, "HydroJebCore.HydroJebCore()");
            }
        }
        #endregion

        #region Folders
        public static readonly FileName.Folder AutopilotSaveFolder = new FileName.Folder(FileName.HydroTechFolder, "PluginData", "rcsautopilot", "autopilots");
        public static readonly FileName.Folder PanelSaveFolder = new FileName.Folder(FileName.HydroTechFolder, "PluginData", "rcsautopilot", "panels");
        #endregion

        // Core #1: Autopilot (HydroJeb)
        #region Fields
        public static bool isReady = true;
        public static bool electricity;
        private static Color mainBtnColor = Color.green;
        public static HydroPartList jebs = new HydroPartList();
        public static Dictionary<int,RCSAutopilot> autopilots = new Dictionary<int,RCSAutopilot>();
        public static Dictionary<int, Panel> panels = new Dictionary<int, Panel>();
        public static HydroJeb ActiveJeb { get { return (HydroJeb)jebs.FirstActive; } }
        public static bool isActiveJeb(HydroJeb jeb) { return ActiveJeb == jeb; }
        public static CalculatorRCSThrust activeVesselRCS = new CalculatorRCSThrust();
        public static HydroPartListEditor jebsEditor = new HydroPartListEditor();
        public static Dictionary<int, IPanelEditor> panelsEditor = new Dictionary<int, IPanelEditor>();
        #endregion

        #region Methods
        //TODO: change from deprecated RenderingManager to the AppLauncher
        private static void AddMainButton()
        {
            
            //HydroRenderingManager.AddToPostDrawQueue(ManagerConsts.RenderMgr_queueSpot, new Callback(drawGUI));
        }
        private static void RemoveMainButton()
        {
            //HydroRenderingManager.RemoveFromPostDrawQueue(ManagerConsts.RenderMgr_queueSpot, new Callback(drawGUI));
        }

        public static void onEditorUpdate(HydroJeb jeb)
        {
            try
            {
                jebsEditor.OnUpdate();
                activeVesselRCS.OnEditorUpdate();
                foreach (IPanelEditor p in panelsEditor.Values)
                    p.OnEditorUpdate();
            }
            catch (Exception e)
            {
                ExceptionHandler(e, "HydroJebCore.onEditorUpdate(HydroJeb)");
            }
        }

        public static void onFlightStart(HydroJeb jeb)
        {
            try
            {
                if (jebsEditor.Count != 0) // Check for jebsEditor
                {
                    jebsEditor.Clear();
                    foreach (IPanelEditor p in panelsEditor.Values)
                        p.HideInEditor();
                }
                jebs.OnStart();
                if (!jebs.Contains(jeb))
                    jebs.Add(jeb);
                if (jeb.vessel != GameStates.ActiveVessel || jebs.CountActive != 1)
                    return;
                HydroFlightCameraManager.onFlightStart();
                foreach (RCSAutopilot ap in autopilots.Values)
                    ap.OnFlightStart();
                foreach (Panel panel in panels.Values)
                    panel.onFlightStart();
                HydroFlightInputManager.onFlightStart();
                if (HydroRenderingManager.Contains(ManagerConsts.RenderMgr_queueSpot))
                    RemoveMainButton();
                AddMainButton();
                electricity = true;
            }
            catch (Exception e)
            {
                ExceptionHandler(e, "HydroJebCore.onFlightStart(HydroJeb)");
            }
        }

        public static void onGamePause(HydroJeb jeb)
        {
            try
            {
                if (jebs.Contains(jeb))
                    jebs.Remove(jeb);
                else
                    return;
                if (jeb.vessel != GameStates.ActiveVessel)
                    return;
                if (jebs.CountActive != 0)
                    return;
                foreach (Panel panel in panels.Values)
                    panel.onGamePause();
                foreach (RCSAutopilot ap in autopilots.Values)
                    ap.onGamePause();
                RemoveMainButton();
            }
            catch (Exception e)
            {
                ExceptionHandler(e, "HydroJebCore.onGamePause(HydroJeb)");
            }
        }

        public static void onGameResume(HydroJeb jeb)
        {
            try
            {
                if (!jebs.Contains(jeb))
                    jebs.Add(jeb);
                if (jeb.vessel != GameStates.ActiveVessel || jebs.CountActive != 1)
                    return;
                foreach (RCSAutopilot ap in autopilots.Values)
                    ap.onGameResume();
                foreach (Panel panel in panels.Values)
                    panel.onGameResume();
                AddMainButton();
            }
            catch (Exception e)
            {
                ExceptionHandler(e, "HydroJebCore.onGameResume(HydroJeb)");
            }
        }

        public static void onPartDestroy(HydroJeb jeb)
        {
            try
            {
                if (GameStates.InEditor)
                {
                    if (jebsEditor.Contains(jeb))
                        jebsEditor.Remove(jeb);
                    else
                        return;
                    if (jebsEditor.Count != 0)
                        return;
                    foreach (IPanelEditor p in panelsEditor.Values)
                        p.HideInEditor();
                }
                else if (GameStates.InFlight)
                {
                    if (jebs.Contains(jeb))
                        jebs.Remove(jeb);
                    else
                        return;
                    if (jebs.CountActive != 0)
                        return;
                    foreach (Panel panel in panels.Values)
                        panel.OnDeactivate();
                    foreach (RCSAutopilot ap in autopilots.Values)
                        ap.OnDeactivate();
                    RemoveMainButton();
                }
            }
            catch (Exception e)
            {
                ExceptionHandler(e, "HydroJebCore.onPartDestroy(HydroJeb)");
            }
        }

        public static void onPartStart(HydroJeb jeb)
        {
            try
            {
                if (!GameStates.InEditor)
                    return;
                bool clear;
                jebsEditor.OnStart(out clear);
                if (clear)
                    foreach (IPanelEditor p in panelsEditor.Values)
                        p.HideInEditor();
                jebsEditor.Add(jeb);
                if (jebsEditor.Count == 1)
                {
                    foreach (IPanelEditor p in panelsEditor.Values)
                        p.ShowInEditor();
                }
            }
            catch (Exception e)
            {
                ExceptionHandler(e, "HydroJebCore.onPartStart(HydroJeb)");
            }
        }

        public static void onPartUpdate(HydroJeb jeb)
        {
            try
            {
                jebs.OnUpdate();
                if (jebs.CountActive == 0)
                {
                    foreach (Panel panel in panels.Values)
                        panel.OnDeactivate();
                    foreach (RCSAutopilot ap in autopilots.Values)
                        ap.OnDeactivate();
                    if (HydroRenderingManager.Contains(ManagerConsts.RenderMgr_queueSpot))
                        RemoveMainButton();
                }
                else
                {
                    if (!isActiveJeb(jeb))
                        return;
                    activeVesselRCS.OnUpdate(GameStates.ActiveVessel);
                    HydroFlightCameraManager.OnUpdate();
                    HydroFlightInputManager.OnUpdate();
                    foreach (RCSAutopilot ap in autopilots.Values)
                        ap.OnUpdate();
                    foreach (Panel panel in panels.Values)
                        panel.OnUpdate();
                    if (!HydroRenderingManager.Contains(ManagerConsts.RenderMgr_queueSpot))
                        AddMainButton();
                    if (!isReady)
                        mainBtnColor = Color.yellow;
                    else if (!electricity)
                        mainBtnColor = Color.red;
                    else
                        mainBtnColor = Color.green;
                }
            }
            catch (Exception e)
            {
                ExceptionHandler(e, "HydroJebCore.onPartUpdate(HydroJeb)");
            }
        }

        private static bool MainPanel
        {
            get { return panels[PanelIDs.Main].PanelShown; }
            set { panels[PanelIDs.Main].PanelShown = value; }
        }

        private static void drawGUI()
        {
            GUI.skin = HighLogic.Skin;
            bool mainBtnRespond = electricity && isReady;
            if (GUI.Button(
                    WindowPositions.MainButton,
                    mainBtnRespond ? (MainPanel ? "/\\ HydroJeb /\\" : "\\/ HydroJeb \\/") : "HydroJeb",
                    Panel.BtnStyle(mainBtnColor)
                ) && mainBtnRespond)
                MainPanel = !MainPanel;
        }
        #endregion

        //Core #2: Docking Assistant (HydroDockAssistCam, HydroDockAssistTarget)
        #region Fields
        public static HydroPartModuleList dockCams = new HydroPartModuleList();
        public static HydroPartModuleList dockTargets = new HydroPartModuleList();
        #endregion

        #region Methods
        public static void OnUpdate(ModuleDockAssistCam mcam)
        {
            try
            {
                if (!dockCams.Contains(mcam))
                    dockCams.Add(mcam);
                dockCams.OnUpdate();
            }
            catch (Exception e)
            {
                ExceptionHandler(e, "HydroJebCore.OnUpdate(ModuleDockAssistCam");
            }
        }

        public static void OnUpdate(ModuleDockAssistTarget mtgt)
        {
            try
            {
                if (!dockTargets.Contains(mtgt))
                    dockTargets.Add(mtgt);
                dockTargets.OnUpdate();
            }
            catch (Exception e)
            {
                ExceptionHandler(e, "HydroJebCore.OnUpdate(ModuleDockAssistTarget)");
            }
        }

        private static void ExceptionHandler(Exception e, String funcName) { GameBehaviours.ExceptionHandler(e, funcName); }
        #endregion      
    }
}

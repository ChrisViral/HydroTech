#if DEBUG
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HydroTech_RCS
{
    using UnityEngine;
    using HydroTech_FC;
    using Autopilots;
    using Panels;
    using Constants.Core;
    using Constants.Panels;
    using Autopilots.Modules;

    static public class HydroJebCore
    {
        static HydroJebCore()
        {
            try
            {
                autopilots.Add(AutopilotIDs.Translation, new APTranslation());
                autopilots.Add(AutopilotIDs.Landing, new APLanding());
                autopilots.Add(AutopilotIDs.Dock, new APDockAssist());
                autopilots.Add(AutopilotIDs.Precise, new APPreciseControl());

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

                panelsEditor.Add(PanelIDs.RCSInfo, new PanelRCSThrustInfo());
                panelsEditor.Add(PanelIDs.Dock, new PanelDockAssistEditorAid());
            }
            catch (Exception e)
            {
                ExceptionHandler(e, "HydroJebCore.HydroJebCore()");
            }
        }

        static public readonly FileName.Folder AutopilotSaveFolder = new FileName.Folder(FileName.HydroTechFolder, "PluginData", "rcsautopilot", "autopilots");
        static public readonly FileName.Folder PanelSaveFolder = new FileName.Folder(FileName.HydroTechFolder, "PluginData", "rcsautopilot", "panels");

/*  First part of core: Autopilot System
 *  Corresponding part: HydroJeb
 */
        static public bool isReady = true;
        static public bool electricity;
        static private Color mainBtnColor = Color.green;

        static public HydroPartList jebs = new HydroPartList();
        static public Dictionary<int,RCSAutopilot> autopilots = new Dictionary<int,RCSAutopilot>();
        static public Dictionary<int, Panel> panels = new Dictionary<int, Panel>();

        static public HydroJeb ActiveJeb { get { return (HydroJeb)jebs.FirstActive; } }
        static public bool isActiveJeb(HydroJeb jeb) { return ActiveJeb == jeb; }

        static public CalculatorRCSThrust activeVesselRCS = new CalculatorRCSThrust();

        static public HydroPartListEditor jebsEditor = new HydroPartListEditor();
        static public Dictionary<int, IPanelEditor> panelsEditor = new Dictionary<int, IPanelEditor>();

        static private void AddMainButton()
        {
            HydroRenderingManager.AddToPostDrawQueue(ManagerConsts.RenderMgr_queueSpot, new Callback(drawGUI));
        }
        static private void RemoveMainButton()
        {
            HydroRenderingManager.RemoveFromPostDrawQueue(ManagerConsts.RenderMgr_queueSpot, new Callback(drawGUI));
        }

        static public void onEditorUpdate(HydroJeb jeb)
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

        static public void onFlightStart(HydroJeb jeb)
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
                    ap.onFlightStart();
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

        static public void onGamePause(HydroJeb jeb)
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

        static public void onGameResume(HydroJeb jeb)
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

        static public void onPartDestroy(HydroJeb jeb)
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

        static public void onPartStart(HydroJeb jeb)
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

        static public void onPartUpdate(HydroJeb jeb)
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

        static private bool MainPanel
        {
            get { return panels[PanelIDs.Main].PanelShown; }
            set { panels[PanelIDs.Main].PanelShown = value; }
        }

        static private void drawGUI()
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

/*  Second part of core: DockingAssistant
 *  Corresponding parts: HydroDockAssistCam, HydroDockAssistTarget
 */
        static public HydroPartModuleList dockCams = new HydroPartModuleList();
        static public HydroPartModuleList dockTargets = new HydroPartModuleList();

        static public void OnUpdate(ModuleDockAssistCam mcam)
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
        static public void OnUpdate(ModuleDockAssistTarget mtgt)
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

        static private void ExceptionHandler(Exception e, String funcName) { GameBehaviours.ExceptionHandler(e, funcName); }
    }
}

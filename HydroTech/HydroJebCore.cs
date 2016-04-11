using System;
using System.Collections.Generic;
using HydroTech.Autopilots;
using HydroTech.Autopilots.Calculators;
using HydroTech.Constants;
using HydroTech.Data;
using HydroTech.Managers;
using HydroTech.Panels;
using HydroTech.PartModules;
using HydroTech.Parts;
using HydroTech.Storage;
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
                Debug.LogError(string.Format("[HydroJebCore]: An exception has been thrown:\n{0} at {1}", e.GetType().Name, e.StackTrace));
            }
        }
        #endregion

        #region Core #1: Autopilot (HydroJeb)
        #region Fields
        public static bool isReady = true;
        public static bool electricity;
        public static Dictionary<int, Panel> panels = new Dictionary<int, Panel>();
        public static RCSCalculator activeVesselRcs = new RCSCalculator();
        public static Dictionary<int, IPanelEditor> panelsEditor = new Dictionary<int, IPanelEditor>();
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
                dockCams.Update();
            }
            catch (Exception e)
            {
                Debug.LogError(string.Format("[HydroJebCore]: An exception has been thrown:\n{0} at {1}", e.GetType().Name, e.StackTrace));
            }
        }

        public static void OnUpdate(ModuleDockAssistTarget mtgt)
        {
            try
            {
                if (!dockTargets.Contains(mtgt)) { dockTargets.Add(mtgt); }
                dockTargets.Update();
            }
            catch (Exception e)
            {
                Debug.LogError(string.Format("[HydroJebCore]: An exception has been thrown:\n{0} at {1}", e.GetType().Name, e.StackTrace));
            }
        }
        #endregion
        #endregion
    }
}
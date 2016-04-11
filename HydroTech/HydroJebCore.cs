using System;
using System.Collections.Generic;
using HydroTech.Autopilots.Calculators;
using HydroTech.Constants;
using HydroTech.Data;
using HydroTech.Panels;
using HydroTech.PartModules;
using HydroTech.Storage;
using UnityEngine;

namespace HydroTech
{
    public static class HydroJebCore
    {
        #region Folders
        public static readonly FileName.Folder autopilotSaveFolder = new FileName.Folder(FileName.hydroTechFolder, "PluginData", "rcsautopilot", "autopilots");
        public static readonly FileName.Folder panelSaveFolder = new FileName.Folder(FileName.hydroTechFolder, "PluginData", "rcsautopilot", "panels");
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
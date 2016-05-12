using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using HydroTech.Autopilots.Calculators;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace HydroTech.Utils
{
    /// <summary>
    /// A collection of general utility fields and methods
    /// </summary>
    public static class HTUtils
    {
        #region Constants
        /// <summary>
        /// Radians to degrees conversion constant
        /// </summary>
        public const float radToDeg = 180 / Mathf.PI;
        /// <summary>
        /// Local HydroTech PluginData folder URL
        /// </summary>
        private const string localPluginDataURL = "GameData/HydroTech/Plugins/PluginData";
        /// <summary>
        /// Local HydroTech default icon URL
        /// </summary>
        private const string defaultIconName = "default.png";
        /// <summary>
        /// Local HydroTech active icon URL
        /// </summary>
        private const string activeIconName = "active.png";
        /// <summary>
        /// Local HydroTech inactive icon URL
        /// </summary>
        private const string inactiveIconName = "inactive.png";
        /// <summary>
        /// Local HydroTech reticle URL
        /// </summary>
        private const string reticleName = "reticle.png";
        /// <summary>
        /// The HydroTech prefix to debug messages
        /// </summary>
        private const string prefix = "[HydroTech]: ";
        /// <summary>
        /// PopupDialog anchor
        /// </summary>
        private static readonly Vector2 anchor = new Vector2(0.5f, 0.5f);
        #endregion

        #region Properties
        /// <summary>
        /// Absolute HydroTech PluginData URL
        /// </summary>
        public static string PluginDataURL { get; }

        /// <summary>
        /// Default HydroTech AppLauncher icon
        /// </summary>
        public static Texture2D LauncherIcon { get; }

        /// <summary>
        /// Active HydroTech AppLauncher icon
        /// </summary>
        public static Texture2D ActiveIcon { get; }

        /// <summary>
        /// Inactive HydroTech AppLauncher icon
        /// </summary>
        public static Texture2D InactiveIcon { get; }

        /// <summary>
        /// The on screen HydroTech reticle
        /// </summary>
        public static Texture2D Reticle { get; }

        /// <summary>
        /// The ElectricCharge PartResourceDefinition ID
        /// </summary>
        public static int ElectricChargeID { get; }

        /// <summary>
        /// A list containing only the ElectricCharge PartResourceDefinition, for IResourceConsumer usage
        /// </summary>
        public static List<PartResourceDefinition> ElectrictyList { get; }

        /// <summary>
        /// The current HydroTech version string
        /// </summary>
        public static string AssemblyVersion { get; }
        #endregion

        #region Constructor
        /// <summary>
        /// Initiates the HTUtils properties
        /// </summary>
        static HTUtils()
        {
            PluginDataURL = Path.Combine(KSPUtil.ApplicationRootPath, localPluginDataURL);

            LauncherIcon = new Texture2D(38, 38, TextureFormat.ARGB32, false);
            LauncherIcon.LoadImage(File.ReadAllBytes(Path.Combine(PluginDataURL, defaultIconName)));

            ActiveIcon = new Texture2D(38, 38, TextureFormat.ARGB32, false);
            ActiveIcon.LoadImage(File.ReadAllBytes(Path.Combine(PluginDataURL, activeIconName)));

            InactiveIcon = new Texture2D(38, 38, TextureFormat.ARGB32, false);
            InactiveIcon.LoadImage(File.ReadAllBytes(Path.Combine(PluginDataURL, inactiveIconName)));

            Reticle = new Texture2D(900, 900, TextureFormat.ARGB32, false);
            Reticle.LoadImage(File.ReadAllBytes(Path.Combine(PluginDataURL, reticleName)));

            PartResourceDefinition electricity = PartResourceLibrary.Instance.resourceDefinitions.First(r => r.name == "ElectricCharge");
            ElectricChargeID = electricity.id;
            ElectrictyList = new List<PartResourceDefinition>(1) { electricity };

            Version version = new Version(FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion);
            AssemblyVersion = "v" + version.ToString(version.Revision == 0 ? (version.Build == 0 ? 2 : 3) : 4);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Clamps the value between the given minimum and maximum
        /// </summary>
        /// <param name="f">Value to clamp</param>
        /// <param name="min">Minimum value</param>
        /// <param name="max">Maximum value</param>
        /// <returns>The clamped value</returns>
        public static float Clamp(float f, float min, float max) => f < max ? (f > min ? f : min) : max;

        /// <summary>
        /// Clamps the given value to a minimum of 0
        /// </summary>
        /// <param name="f">Value to clamp</param>
        /// <returns>The clamped value</returns>
        public static float Clamp0(float f) => Mathf.Max(0, f);

        /// <summary>
        /// Clamps the given value to a maximum of 100
        /// </summary>
        /// <param name="f">Value to clamp</param>
        /// <returns>The clamped value</returns>
        public static float Clamp100(float f) => Mathf.Min(f, 100);

        /// <summary>
        /// Spawns a PopupDialog of the given title, message, and button text
        /// </summary>
        /// <param name="title">Title of the PopupDialog</param>
        /// <param name="message">Message of the popupDialog</param>
        /// <param name="button">Button text of the PopupDialog</param>
        public static void SpawnPopupDialog(string title, string message, string button)
        {
            PopupDialog.SpawnPopupDialog(anchor, anchor, title, message, button, false, HighLogic.UISkin);
        }

        /// <summary>
        /// Logs a message with the correct header
        /// </summary>
        /// <param name="message">Message to log</param>
        public static void Log(string message) => Debug.Log(prefix + message);

        /// <summary>
        /// Logs a warning message with the correct header
        /// </summary>
        /// <param name="message">Message to log</param>
        public static void LogWarning(string message) => Debug.LogWarning(prefix + message);

        /// <summary>
        /// Logs an error message with the correct header
        /// </summary>
        /// <param name="message">Message to log</param>
        public static void LogError(string message) => Debug.LogError(prefix + message);

        /// <summary>
        /// Sets the camera's rotation to the vessel's
        /// </summary>
        /// <param name="ctrlState">Current vessel's FlightCtrlState</param>
        /// <param name="cam">Camera to set the rotation for</param>
        public static void CamToVesselRot(FlightCtrlState ctrlState, ModuleDockAssistCam cam)
        {
            SwitchTransformCalculator sCal = new SwitchTransformCalculator();
            sCal.GetRotation(ctrlState);
            sCal.ChangeTransformRotation(cam.Right, cam.Down, cam.Dir, cam.vessel.ReferenceTransform);
            sCal.SetRotation(ctrlState);
        }

        /// <summary>
        /// Sets the camera's translation to the vessel's
        /// </summary>
        /// <param name="ctrlState">Current vessel's FlightCtrlState</param>
        /// <param name="cam">Camera to set the translation for</param>
        public static void CamToVesselTrans(FlightCtrlState ctrlState, ModuleDockAssistCam cam)
        {
            SwitchTransformCalculator sCal = new SwitchTransformCalculator();
            sCal.GetTranslation(ctrlState);
            sCal.ChangeTransformTranslation(cam.Right, cam.Down, cam.Dir, cam.vessel.ReferenceTransform);
            sCal.SetTranslation(ctrlState);
        }
        #endregion
    }
}
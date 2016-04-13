using System.IO;
using System.Linq;
using UnityEngine;

namespace HydroTech.Utils
{
    public static class HTUtils
    {
        #region Constants
        //General
        public const float radToDeg = 180 / Mathf.PI;
        public const string localPluginDataURL = "GameData/HydroTech/Plugins/PluginData";
        public const string localIconURL = "GameData/HydroTech/Plugins/PluginData/HydroTech_icon.png";
        public const string localActiveIconURL = "GameData/HydroTech/Plugins/PluginData/HydroTech_active_icon.png";
        public const string localInactiveIconURL = "GameData/HydroTech/Plugins/PluginData/HydroTech_inactive_icon.png";

        //Units
        public const string length = "m";
        public const string speedSimple = "m/s";
        public const string acceleration = "m/s²";
        public const string angularAcc = "rad/s²";

        //Docking
        public const bool autoOrient = false;
        public const bool camView = false;
        public const bool driveTarget = false;
        public const bool killRelV = false;
        public const bool manual = true;
        public const bool showLine = true;
        public const float finalStageSpeed = 0.4f;
        public const float dockingAngularAcc = 0.5f;
        public const float dockingAcc = 0.5f;
        public const float finalStageErr = 0.05f;
        public static readonly Vector3 finalStagePos = new Vector3(0, 0, 15);
        public const float vel0 = 1;
        public const float safeSpeed = 0.5f;
        public const float stopSpeed = 0.05f;

        //Landing
        public const bool vabPod = true;
        public const bool engine = false;
        public const bool burnRetro = false;
        public const bool touchdown = true;
        public const bool useTrueAlt = true;
        public const float safeTouchDownSpeed = 0.5f;   //Default vertical speed for final touchdown
        public const float maxThrottle = 1;
        public const float altKeep = 10;
        public const float safeHorizontalSpeed = 0.1f; //Maximum horizontal speed for touchdown
        public const float finalDescentHeight = 10;

        //Precise Control
        public const bool byRate = true;
        public const float rotationRate = 0.1f;
        public const float translationRate = 0.1f;
        public const float pcAngularAcc = 1;
        public const float pcAcc = 1;

        //Translation
        public const bool mainThrottleRespond = true;
        public const bool holdOrient = true;
        public const float thrustRate = 1;
        #endregion

        #region Properties
        private static readonly string pluginDataURL;
        public static string PluginDataURL
        {
            get { return pluginDataURL; }
        }

        private static readonly Texture2D launcherIcon;
        public static Texture2D LauncherIcon
        {
            get { return launcherIcon; }
        }

        private static readonly Texture2D activeIcon;
        public static Texture2D ActiveIcon
        {
            get { return activeIcon; }
        }

        private static readonly Texture2D inactiveIcon;
        public static Texture2D InactiveIcon
        {
            get { return inactiveIcon; }
        }

        private static readonly int electricChargeID;
        public static int ElectricChargeID
        {
            get { return electricChargeID; }
        }
        #endregion

        #region Constructor
        static HTUtils()
        {
            pluginDataURL = Path.Combine(KSPUtil.ApplicationRootPath, localPluginDataURL);

            launcherIcon = new Texture2D(38, 38, TextureFormat.ARGB32, false);
            launcherIcon.LoadImage(File.ReadAllBytes(Path.Combine(KSPUtil.ApplicationRootPath, localIconURL)));

            activeIcon = new Texture2D(38, 38, TextureFormat.ARGB32, false);
            activeIcon.LoadImage(File.ReadAllBytes(Path.Combine(KSPUtil.ApplicationRootPath, localActiveIconURL)));

            inactiveIcon = new Texture2D(38, 38, TextureFormat.ARGB32, false);
            inactiveIcon.LoadImage(File.ReadAllBytes(Path.Combine(KSPUtil.ApplicationRootPath, localInactiveIconURL)));

            electricChargeID = PartResourceLibrary.Instance.resourceDefinitions.First(r => r.name == "ElectricCharge").id;
        }
        #endregion

        #region Methods
        public static float Clamp(float x, float min, float max)
        {
            if (x >= max) { return max; }
            return x <= min ? min : x;
        }
        #endregion
    }
}
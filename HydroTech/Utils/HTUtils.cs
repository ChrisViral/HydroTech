using System.IO;
using System.Linq;
using UnityEngine;

namespace HydroTech.Utils
{
    public static class HTUtils
    {
        #region Constants
        public const float radToDeg = 180 / Mathf.PI;
        public const string localPluginDataURL = "GameData/HydroTech/Plugins/PluginData";
        public const string localIconURL = "GameData/HydroTech/Plugins/PluginData/HydroTech_icon.png";
        public const string localActiveIconURL = "GameData/HydroTech/Plugins/PluginData/HydroTech_active_icon.png";
        public const string localInactiveIconURL = "GameData/HydroTech/Plugins/PluginData/HydroTech_inactive_icon.png";
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

        public static bool GetState(Vessel vessel, KSPActionGroup action)
        {
            return vessel.ActionGroups[action];
        }

        public static void SetState(Vessel vessel, KSPActionGroup action, bool active)
        {
            vessel.ActionGroups.SetGroup(action, active);
        }
        #endregion
    }
}
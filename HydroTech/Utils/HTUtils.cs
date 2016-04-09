using System.IO;
using UnityEngine;

namespace HydroTech.Utils
{
    public static class HTUtils
    {
        #region Constants
        public const float radToDeg = 180 / Mathf.PI;
        public const string localPluginDataURL = @"GameData\HydroTech\Plugins\PluginData";
        public const string localIconURL = @"GameData\HydroTech\Plugins\PluginData\HydroTech_icon.png";
        public const string localInactiveIconURL = @"GameData\HydroTech\Plugins\PluginData\HydroTech_inactive_icon.png";
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

        private static readonly Texture2D inactiveIcon;
        public static Texture2D InactiveIcon
        {
            get { return inactiveIcon; }
        }
        #endregion

        #region Constructor
        static HTUtils()
        {
            pluginDataURL = Path.Combine(KSPUtil.ApplicationRootPath, localPluginDataURL);

            launcherIcon = new Texture2D(38, 38, TextureFormat.ARGB32, false);
            launcherIcon.LoadImage(File.ReadAllBytes(Path.Combine(KSPUtil.ApplicationRootPath, localIconURL)));

            inactiveIcon = new Texture2D(38, 38, TextureFormat.ARGB32, false);
            inactiveIcon.LoadImage(File.ReadAllBytes(Path.Combine(KSPUtil.ApplicationRootPath, localInactiveIconURL)));
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
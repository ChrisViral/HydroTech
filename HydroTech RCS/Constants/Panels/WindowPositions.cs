using UnityEngine;

namespace HydroTech_RCS.Constants.Panels
{
    public static class WindowPositions
    {
        public static readonly Rect mainButton = new Rect(200, 0, 100, 25);
        public static readonly Rect main = new Rect(150, 25, 200, 216);
        public static readonly Rect mainThrottle = new Rect(100, 240, 250, 236);
        public static readonly Rect rcsInfo = new Rect(747, 80, 250, 280);
        public static readonly Rect rcsInfoEditor = new Rect((Screen.width * 0.95F) - 250, 80, 250, 0);
        public static readonly Rect preciseControl = new Rect(349, 60, 200, 122);
        public static readonly Rect translation = new Rect(142, 475, 200, 260);
        public static readonly Rect landing = new Rect(548, 80, 200, 184);
        public static readonly Rect docking = new Rect(349, 215, 200, 252);
        public static readonly Rect landingInfo = new Rect(548, 300, 200, 0);
        public static readonly Rect dockAssistEditor = new Rect((Screen.width * 0.95F) - 250, 360, 250, 0);
    }
}
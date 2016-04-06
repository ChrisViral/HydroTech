using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HydroTech_RCS.Constants.Panels
{
    using UnityEngine;

    static public class WindowPositions
    {
        static public readonly Rect MainButton = new Rect(200, 0, 100, 25);
        static public readonly Rect Main = new Rect(150, 25, 200, 216);
        static public readonly Rect MainThrottle = new Rect(100, 240, 250, 236);
        static public readonly Rect RCSInfo = new Rect(747, 80, 250, 280);
        static public readonly Rect RCSInfo_Editor = new Rect(Screen.width * 0.95F - 250, 80, 250, 0);
        static public readonly Rect PreciseControl = new Rect(349, 60, 200, 122);
        static public readonly Rect Translation = new Rect(142, 475, 200, 260);
        static public readonly Rect Landing = new Rect(548, 80, 200, 184);
        static public readonly Rect Docking = new Rect(349, 215, 200, 252);
        static public readonly Rect LandingInfo = new Rect(548, 300, 200, 0);
        static public readonly Rect DockAssistEditor = new Rect(Screen.width * 0.95F - 250, 360, 250, 0);
    }
}

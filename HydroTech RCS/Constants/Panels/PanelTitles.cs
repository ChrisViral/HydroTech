using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HydroTech_RCS.Constants.Panels
{
    static public class PanelTitles
    {
        public const String Main = "HydroTech RCS Autopilot";
        public const String MainThrottle = "Main Throttle Control";
        public const String RCSInfo = "RCS Thrust Info";
        public const String RCSInfo_EditorHide = "RCS";
        public const String PreciseControl = "Precise Control";
        public const String Translation = "Automatic Translation";
        public const String Landing = "Landing Autopilot";
        public const String Docking = "Docking Assistant";
        public const String LandingInfo = "Advanced Information";
        public const String DockAssistEditorAid = "Docking Assistance System";
        public const String DockAssistEditorAid_Hide = "DA";

#if DEBUG
        public const String Debug = "Debug";
#endif
    }
}

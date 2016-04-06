using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HydroTech_RCS.Constants.Core
{
    static public class AutopilotIDs
    {
        public const int Translation = 0;
        public const int Landing = 1;
        public const int Dock = 2;
        public const int Precise = 3;
    }
    static public class PanelIDs
    {
#if DEBUG
        public const int Debug = 15;
#endif

        public const int Main = 1;
        public const int MainThrottle = 2;
        public const int RCSInfo = 3;
        public const int PreciseControl = 4;
        public const int Translation = 5;
        public const int Landing = 6;
        public const int Dock = 7;
        public const int LandingInfo = 8;
    }
    static public class ManagerConsts
    {
        public const int RenderMgr_queueSpot = 3;
        public const int RenderMgr_ModulePartRename = 20;
        public const int RenderMgr_ModuleHydroASAS = 21;
    }
}

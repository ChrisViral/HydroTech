namespace HydroTech_RCS.Constants.Core
{
    public static class AutopilotIDs
    {
        public const int translation = 0;
        public const int landing = 1;
        public const int dock = 2;
        public const int precise = 3;
    }

    public static class PanelIDs
    {
#if DEBUG
        public const int debug = 15;
#endif

        public const int main = 1;
        public const int mainThrottle = 2;
        public const int rcsInfo = 3;
        public const int preciseControl = 4;
        public const int translation = 5;
        public const int landing = 6;
        public const int dock = 7;
        public const int landingInfo = 8;
    }

    public static class ManagerConsts
    {
        public const int renderMgrQueueSpot = 3;
        public const int renderMgrModulePartRename = 20;
        public const int renderMgrModuleHydroAsas = 21;
    }
}
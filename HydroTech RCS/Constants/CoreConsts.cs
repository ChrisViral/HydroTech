
namespace HydroTech_RCS.Constants
{
    public static class CoreConsts
    {
        #region Autopilot IDs
        public const int apTranslation = 0;
        public const int apLanding = 1;
        public const int apDock = 2;
        public const int precise = 3;
        #endregion

        #region Behaviours
        public const double electricConsumptionAutopilot = 0.01;
        public const double electricConsumptionCamera = 0.002;
        public const double electricConsumptionLaser = 0.005;
        public const float defaultFoVPreviewVessel = 60;
        #endregion

        #region Events
        public const string lowerLandingLeg = "LowerLeg";
        public const string lowerLandingGear = "LowerLandingGear";
        #endregion
       
        #region Manager
        public const int renderMgrQueueSpot = 3;
        public const int renderMgrModulePartRename = 20;
        public const int renderMgrModuleHydroAsas = 21;
        #endregion

        #region Panel IDs
        public const int main = 1;
        public const int mainThrottle = 2;
        public const int rcsInfo = 3;
        public const int preciseControl = 4;
        public const int pTranslation = 5;
        public const int pLanding = 6;
        public const int pDock = 7;
        public const int landingInfo = 8;
#if DEBUG
        public const int debug = 15;
#endif
        #endregion
    }
}

using HydroTech_FC;
using HydroTech_RCS.Constants.Autopilots.Landing;
using HydroTech_RCS.Constants.Core;
using HydroTech_RCS.Panels;

namespace HydroTech_RCS.Autopilots
{
    public partial class APLanding : RcsAutopilot
    {
        public enum Indicator
        {
            LANDED,
            SAFE,
            WARP,
            OK,
            DANGER,
            LOWTWR,
            OUTSYNC,
            FINAL,
            HOVER
        }

        public enum Status
        {
            DISENGAGED,
            IDLE,
            HORIZONTAL,
            VERTICAL,
            DECELERATE,
            DESCEND,
            AVOID,
            WARP,
            LANDED,
            HOVER
        }

        public static APLanding TheAutopilot
        {
            get { return (APLanding)HydroJebCore.autopilots[AutopilotIDs.landing]; }
        }

        public static PanelLanding Panel
        {
            get { return PanelLanding.ThePanel; }
        }

        protected override string nameString
        {
            get { return Str.nameString; }
        }

        public APLanding()
        {
            this.fileName = new FileName("landing", "cfg", HydroJebCore.autopilotSaveFolder);
        }

        protected override void LoadDefault()
        {
            base.LoadDefault();
            this.vabPod = Default.Bool.vabPod;
            this.engines = Default.Bool.engine;
            this.burnRetro = Default.Bool.burnRetro;
            this.touchdown = Default.Bool.touchdown;
            this.useTrueAlt = Default.Bool.useTrueAlt;
            this.safeTouchDownSpeed = Default.Float.safeTouchDownSpeed;
            this.maxThrottle = Default.Float.maxThrottle;
            this.altKeep = Default.Float.altKeep;
        }

        #region public variables for user input

        #region bool
        [HydroSLNodeInfo(name = "SETTINGS"), HydroSLField(saveName = "PointUp")]
        public bool vabPod = Default.Bool.vabPod;

        [HydroSLNodeInfo(name = "SETTINGS"), HydroSLField(saveName = "UseEngines")]
        public bool engines = Default.Bool.engine;

        public bool Engines
        {
            get { return this.engines; }
            set
            {
                if (value != this.engines) { Panel.ResetHeight(); }
                this.engines = value;
            }
        }

        [HydroSLNodeInfo(name = "SETTIINGS"), HydroSLField(saveName = "BurnRetro", isTesting = true)]
        public bool burnRetro = Default.Bool.burnRetro;

        [HydroSLNodeInfo(name = "SETTINGS"), HydroSLField(saveName = "Touchdown")]
        public bool touchdown = Default.Bool.touchdown;

        [HydroSLNodeInfo(name = "SETTINGS"), HydroSLField(saveName = "TrueAlt")]
        public bool useTrueAlt = Default.Bool.useTrueAlt;
        #endregion

        #region float
        [HydroSLNodeInfo(name = "SETTINGS"), HydroSLField(saveName = "TouchdownSpeed")]
        public float safeTouchDownSpeed = Default.Float.safeTouchDownSpeed;

        [HydroSLNodeInfo(name = "SETTINGS"), HydroSLField(saveName = "MaxThrottle")]
        public float maxThrottle = Default.Float.maxThrottle;

        public float MaxThrottle
        {
            get { return this.maxThrottle * 100; }
            set { this.maxThrottle = value / 100; }
        }

        [HydroSLNodeInfo(name = "SETTINGS"), HydroSLField(saveName = "Altitude")]
        public float altKeep = Default.Float.altKeep;
        #endregion

        #region override
        public override bool engaged
        {
            set
            {
                if (value != this._Engaged) { Panel.ResetHeight(); }
                this.Engaged = value;
            }
        }
        #endregion

        #endregion
    }
}
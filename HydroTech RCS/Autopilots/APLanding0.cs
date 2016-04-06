using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HydroTech_RCS.Autopilots
{
    using HydroTech_FC;
    using Constants.Core;
    using Constants.Autopilots.Landing;
    using Panels;

    public partial class APLanding : RCSAutopilot
    {
        static public APLanding theAutopilot { get { return (APLanding)HydroJebCore.autopilots[AutopilotIDs.Landing]; } }
        static public PanelLanding panel { get { return PanelLanding.thePanel; } }

        public APLanding()
        {
            fileName = new FileName("landing", "cfg", HydroJebCore.AutopilotSaveFolder);
        }

        protected override string nameString { get { return Str.nameString; } }

        public enum Status { DISENGAGED, IDLE, HORIZONTAL, VERTICAL, DECELERATE, DESCEND, AVOID, WARP, LANDED, HOVER }
        public enum Indicator { LANDED, SAFE, WARP, OK, DANGER, LOWTWR, OUTSYNC, FINAL, HOVER }

        #region public variables for user input

        #region bool

        [HydroSLNodeInfo(name = "SETTINGS")]
        [HydroSLField(saveName = "PointUp")]
        public bool VABPod = Default.BOOL.VABPod;

        [HydroSLNodeInfo(name = "SETTINGS")]
        [HydroSLField(saveName = "UseEngines")]
        public bool _Engines = Default.BOOL.Engine;
        public bool Engines
        {
            get { return _Engines; }
            set
            {
                if (value != _Engines)
                    panel.ResetHeight();
                _Engines = value;
            }
        }

        [HydroSLNodeInfo(name = "SETTIINGS")]
        [HydroSLField(saveName = "BurnRetro", isTesting = true)]
        public bool burnRetro = Default.BOOL.burnRetro;

        [HydroSLNodeInfo(name = "SETTINGS")]
        [HydroSLField(saveName = "Touchdown")]
        public bool touchdown = Default.BOOL.touchdown;

        [HydroSLNodeInfo(name = "SETTINGS")]
        [HydroSLField(saveName = "TrueAlt")]
        public bool useTrueAlt = Default.BOOL.useTrueAlt;

        #endregion

        #region float

        [HydroSLNodeInfo(name = "SETTINGS")]
        [HydroSLField(saveName = "TouchdownSpeed")]
        public float safeTouchDownSpeed = Default.FLOAT.safeTouchDownSpeed;

        [HydroSLNodeInfo(name = "SETTINGS")]
        [HydroSLField(saveName = "MaxThrottle")]
        public float maxThrottle = Default.FLOAT.maxThrottle;
        public float MaxThrottle
        {
            get { return maxThrottle * 100; }
            set { maxThrottle = value / 100; }
        }

        [HydroSLNodeInfo(name = "SETTINGS")]
        [HydroSLField(saveName = "Altitude")]
        public float altKeep = Default.FLOAT.altKeep;

        #endregion

        #region override

        public override bool Engaged
        {
            set
            {
                if (value != _Engaged)
                    panel.ResetHeight();
                base.Engaged = value;
            }
        }

        #endregion

        #endregion

        protected override void LoadDefault()
        {
            base.LoadDefault();
            VABPod = Default.BOOL.VABPod;
            _Engines = Default.BOOL.Engine;
            burnRetro = Default.BOOL.burnRetro;
            touchdown = Default.BOOL.touchdown;
            useTrueAlt = Default.BOOL.useTrueAlt;
            safeTouchDownSpeed = Default.FLOAT.safeTouchDownSpeed;
            maxThrottle = Default.FLOAT.maxThrottle;
            altKeep = Default.FLOAT.altKeep;
        }
    }
}
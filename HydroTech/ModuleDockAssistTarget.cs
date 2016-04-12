using HydroTech.Managers;
using HydroTech.Panels;

namespace HydroTech
{
    public class ModuleDockAssistTarget : ModuleDockAssist
    {
        #region Static Properties
        private static ModuleDockAssistTarget Current
        {
            get { return HydroFlightManager.Instance.DockingAutopilot.target; }
            set { HydroFlightManager.Instance.DockingAutopilot.target = value; }
        }
        #endregion

        #region Properties
        protected bool isNear;
        public bool IsNear
        {
            get
            {
                if (this.vessel.isActiveVessel)
                {
                    if (this.isNear && Current == this) { FlightMainPanel.Instance.DockAssist.ResetHeight(); }
                    this.isNear = false;
                }
                else { this.isNear = true; }
                return this.isNear;
            }
        }

        protected override string ModuleShort
        {
            get { return "Tgt"; }
        }
        #endregion

        #region Functions
        protected override void OnDestroy()
        {
            if (!HighLogic.LoadedSceneIsFlight) { return; }

            base.OnDestroy();
            if (this == Current)
            {
                Current = null;
                FlightMainPanel.Instance.DockAssist.ResetHeight();
            }         
        }
        #endregion

        #region Overrides
        public override string GetModuleTitle()
        {
            return "Docking Target";
        }
        #endregion
    }
}
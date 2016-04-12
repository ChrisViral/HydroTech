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
        public void OnDestroy()
        {
            if (!HighLogic.LoadedSceneIsFlight || this != Current) { return; }

            Current = null;
            FlightMainPanel.Instance.DockAssist.ResetHeight();
        }
        #endregion

        #region Overrides
        public override void OnStart(StartState state)
        {
            if (HighLogic.LoadedSceneIsFlight && this.part.name == "HydroTech.DA.2m")
            {
                this.assistPos.Set(0, 0.07f, 1.375f);
            }
        }
        #endregion
    }
}
namespace HydroTech.PartModules.Base
{
    public abstract class ModuleActiveIndicator : PartModule
    {
        public enum Status
        {
            ACTIVE,
            IDLE
        }

        #region KSPFields
        [KSPField(guiActive = true, guiName = "Status")]
        public Status status = Status.IDLE;
        #endregion

        #region Abstract methods
        protected abstract Status GetStatus();
        #endregion

        #region Overrides
        public override void OnUpdate()
        {
            this.status = GetStatus();
        }
        #endregion
    }
}
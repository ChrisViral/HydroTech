namespace HydroTech_RCS.PartModules.Base
{
    public abstract class ModuleActiveIndicator : PartModule
    {
        public enum Status
        {
            ACTIVE,
            IDLE
        }

        [KSPField(guiActive = true, guiName = "Status")]
        public Status status = Status.IDLE;

        public override void OnUpdate()
        {
            base.OnUpdate();
            this.status = GetStatus();
        }

        protected abstract Status GetStatus();
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HydroTech_RCS.PartModules.Base
{
    public abstract class ModuleActiveIndicator : PartModule
    {
        protected ModuleActiveIndicator() { }

        public enum STATUS { Active, Idle }

        [KSPField(guiActive = true, guiName = "Status")]
        public STATUS status = STATUS.Idle;

        public override void OnUpdate()
        {
            base.OnUpdate();
            status = GetStatus();
        }

        protected abstract STATUS GetStatus();
    }
}
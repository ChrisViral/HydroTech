using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HydroTech_RCS;
using HydroTech_RCS.PartModules.Base;

public class ModuleActiveHydroJebIndicator : ModuleActiveIndicator
{
    protected override ModuleActiveIndicator.STATUS GetStatus()
    {
        if (HydroJebCore.isActiveJeb((HydroJeb)part))
            return STATUS.Active;
        else
            return STATUS.Idle;
    }
}
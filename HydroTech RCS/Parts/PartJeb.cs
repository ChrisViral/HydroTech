using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HydroTech_RCS;
using HydroTech_RCS.Autopilots;
using HydroTech_RCS.Constants.Core;

public class HydroJeb : Part
{
    protected virtual void SetIcon()
    {
        stackIcon.SetIcon(DefaultIcons.ADV_SAS);
        stackIcon.SetIconColor(XKCDColors.SkyBlue); // Just for fun
    }
    protected override void onEditorUpdate()
    {
        base.onEditorUpdate();
        HydroJebCore.OnEditorUpdate(this);
    }
    protected override void onFlightStart()
    {
        base.onFlightStart();
        SetIcon();
        HydroJebCore.OnFlightStart(this);
    }
    protected override void onGamePause()
    {
        base.onGamePause();
        HydroJebCore.OnGamePause(this);
    }
    protected override void onGameResume()
    {
        base.onGameResume();
        HydroJebCore.OnGameResume(this);
    }
    protected override void onPartDestroy()
    {
        base.onPartDestroy();
        HydroJebCore.OnPartDestroy(this);
    }
    protected override void onPartStart()
    {
        base.onPartStart();
        SetIcon();
        HydroJebCore.OnPartStart(this);
    }
    protected override void onPartUpdate()
    {
        base.onPartUpdate();
        HydroJebCore.OnPartUpdate(this);
        double electricChargeConsumptionRate = 0.0;
        if (isActiveJeb())
        {
            if (RCSAutopilot.AutopilotEngaged)
                electricChargeConsumptionRate += Behaviours.Electric_Consumption_Autopilot;
            if (DA.showLine)
                electricChargeConsumptionRate += Behaviours.Electric_Consumption_Laser;
        }
        else // !isActiveJeb()
        {
            if (DA.engaged && DA.driveTarget && vessel == DA.target.vessel
                && this == DA.jebsTargetVessel.FirstOrDefault()) // driving target vessel
                electricChargeConsumptionRate += Behaviours.Electric_Consumption_Autopilot;
        }
        if (electricChargeConsumptionRate > 0.0 && TimeWarp.deltaTime != 0)
            HydroJebCore.electricity = RequestResource(
                "ElectricCharge",
                electricChargeConsumptionRate * TimeWarp.deltaTime
                ) > 0.0;
    }

    public bool isActiveJeb() { return HydroJebCore.isActiveJeb(this); }

    protected static APDockAssist DA { get { return APDockAssist.theAutopilot; } }
}
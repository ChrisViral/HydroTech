using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HydroTech_RCS;
using HydroTech_RCS.Autopilots;
using HydroTech_RCS.Constants.Core;

public class HydroJeb : Part
{
    virtual protected void SetIcon()
    {
        stackIcon.SetIcon(DefaultIcons.ADV_SAS);
        stackIcon.SetIconColor(XKCDColors.SkyBlue); // Just for fun
    }
    protected override void onEditorUpdate()
    {
        base.onEditorUpdate();
        HydroJebCore.onEditorUpdate(this);
    }
    protected override void onFlightStart()
    {
        base.onFlightStart();
        SetIcon();
        HydroJebCore.onFlightStart(this);
    }
    protected override void onGamePause()
    {
        base.onGamePause();
        HydroJebCore.onGamePause(this);
    }
    protected override void onGameResume()
    {
        base.onGameResume();
        HydroJebCore.onGameResume(this);
    }
    protected override void onPartDestroy()
    {
        base.onPartDestroy();
        HydroJebCore.onPartDestroy(this);
    }
    protected override void onPartStart()
    {
        base.onPartStart();
        SetIcon();
        HydroJebCore.onPartStart(this);
    }
    protected override void onPartUpdate()
    {
        base.onPartUpdate();
        HydroJebCore.onPartUpdate(this);
        double electricChargeConsumptionRate = 0.0;
        if (isActiveJeb())
        {
            if (RCSAutopilot.AutopilotEngaged)
                electricChargeConsumptionRate += Behaviours.Electric_Consumption_Autopilot;
            if (DA.ShowLine)
                electricChargeConsumptionRate += Behaviours.Electric_Consumption_Laser;
        }
        else // !isActiveJeb()
        {
            if (DA.Engaged && DA.DriveTarget && vessel == DA.Target.vessel
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

    static protected APDockAssist DA { get { return APDockAssist.theAutopilot; } }
}
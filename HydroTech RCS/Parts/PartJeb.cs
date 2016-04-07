using HydroTech_RCS;
using HydroTech_RCS.Autopilots;
using HydroTech_RCS.Constants.Core;

public class HydroJeb : Part
{
    protected static APDockAssist Da
    {
        get { return APDockAssist.TheAutopilot; }
    }

    protected virtual void SetIcon()
    {
        this.stackIcon.SetIcon(DefaultIcons.ADV_SAS);
        this.stackIcon.SetIconColor(XKCDColors.SkyBlue); // Just for fun
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
        if (IsActiveJeb())
        {
            if (RCSAutopilot.AutopilotEngaged) { electricChargeConsumptionRate += Behaviours.electricConsumptionAutopilot; }
            if (Da.ShowLine) { electricChargeConsumptionRate += Behaviours.electricConsumptionLaser; }
        }
        else // !isActiveJeb()
        {
            if (Da.engaged && Da.DriveTarget && this.vessel == Da.target.vessel && this == Da.jebsTargetVessel.FirstOrDefault()) // driving target vessel
            {
                electricChargeConsumptionRate += Behaviours.electricConsumptionAutopilot;
            }
        }
        if (electricChargeConsumptionRate > 0.0 && TimeWarp.deltaTime != 0) { HydroJebCore.electricity = RequestResource("ElectricCharge", electricChargeConsumptionRate * TimeWarp.deltaTime) > 0.0; }
    }

    public bool IsActiveJeb()
    {
        return HydroJebCore.IsActiveJeb(this);
    }
}
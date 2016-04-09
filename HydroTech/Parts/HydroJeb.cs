using System.Linq;
using HydroTech.Autopilots;
using HydroTech.Constants;

namespace HydroTech.Parts
{
    //TODO: transform this into a PartModule
    public class HydroJeb : Part
    {
        #region Static properties
        protected static APDockAssist DA
        {
            get { return APDockAssist.TheAutopilot; }
        }
        #endregion

        #region Properties
        public bool IsActiveJeb
        {
            get { return HydroJebCore.IsActiveJeb(this); }
        }
        #endregion

        #region Overrides
        protected virtual void SetIcon()
        {
            this.stackIcon.SetIcon(DefaultIcons.ADV_SAS);
            this.stackIcon.SetIconColor(XKCDColors.SkyBlue); // Just for fun
        }

        protected override void onEditorUpdate()
        {
            HydroJebCore.OnEditorUpdate(this);
        }

        protected override void onFlightStart()
        {
            SetIcon();
            HydroJebCore.OnFlightStart(this);
        }

        protected override void onGamePause()
        {
            HydroJebCore.OnGamePause(this);
        }

        protected override void onGameResume()
        {
            HydroJebCore.OnGameResume(this);
        }

        protected override void onPartDestroy()
        {
            HydroJebCore.OnPartDestroy(this);
        }

        protected override void onPartStart()
        {
            SetIcon();
            HydroJebCore.OnPartStart(this);
        }

        protected override void onPartUpdate()
        {
            HydroJebCore.OnPartUpdate(this);
            double electricChargeConsumptionRate = 0;
            if (this.IsActiveJeb)
            {
                if (RCSAutopilot.AutopilotEngaged) { electricChargeConsumptionRate += CoreConsts.electricConsumptionAutopilot; }
                if (DA.ShowLine) { electricChargeConsumptionRate += CoreConsts.electricConsumptionLaser; }
            }
            else
            {
                if (DA.Engaged && DA.DriveTarget && this.vessel == DA.target.vessel && this == DA.jebsTargetVessel.FirstOrDefault()) // driving target vessel
                {
                    electricChargeConsumptionRate += CoreConsts.electricConsumptionAutopilot;
                }
            }
            if (electricChargeConsumptionRate > 0 && TimeWarp.deltaTime != 0) { HydroJebCore.electricity = RequestResource("ElectricCharge", electricChargeConsumptionRate * TimeWarp.deltaTime) > 0; }
        }
        #endregion
    }
}
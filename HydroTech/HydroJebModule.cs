using System.Linq;
using System.Text;
using HydroTech.Managers;
using HydroTech.Utils;
using UnityEngine;

namespace HydroTech
{
    public class HydroJebModule : PartModule, IModuleInfo
    {
        public enum AutopilotStatus
        {
            Online,
            Offline,
            Idle
        }

        #region Static fields
        private static readonly int ecID = PartResourceLibrary.Instance.resourceDefinitions.First(r => r.name == "ElectricCharge").id;
        #endregion

        #region KSPFields
        [KSPField]
        public float consumptionRate = 0.1f;

        [KSPField(guiActive = true, guiName = "Status")]
        public string status = "Online";
        #endregion

        #region Properties
        private AutopilotStatus state;
        public AutopilotStatus State
        {
            get { return this.state; }
            set
            {
                this.status = EnumUtils.GetName(value);
                this.state = value;
            }
        }

        public bool IsOnline
        {
            get { return this.state == AutopilotStatus.Online; }
        }
        #endregion

        #region Methods
        public string GetModuleTitle()
        {
            return "HydroTech Autopilot";
        }

        public Callback<Rect> GetDrawModulePanelCallback()
        {
            return null;
        }

        public string GetPrimaryField()
        {
            return string.Empty;
        }
        #endregion

        #region Functions
        private void Update()
        {
            if (!HighLogic.LoadedSceneIsFlight || !this.vessel.loaded || !this.vessel.IsControllable) { return; }
            if (this == this.vessel.GetMasterJeb())
            {
                if (this.State == AutopilotStatus.Idle) { this.State = AutopilotStatus.Online; }
            }
            else if (this.IsOnline) { this.State = AutopilotStatus.Idle; }
        }

        private void FixedUpdate()
        {
            if (!HighLogic.LoadedSceneIsFlight || (!this.IsOnline && this != this.vessel.GetMasterJeb()) || !this.vessel.loaded || !this.vessel.IsControllable) { return; }

            if ((CheatOptions.InfiniteElectricity ? 1 : this.part.RequestResource(ecID, this.consumptionRate * TimeWarp.fixedDeltaTime)) <= 0)
            {
                if (this.IsOnline)
                {
                    this.vessel.FindPartModulesImplementing<HydroJebModule>().ForEach(m => m.State = AutopilotStatus.Offline);
                }
            }
            else
            {
                if (!this.IsOnline)
                {
                    this.State = AutopilotStatus.Online;
                    foreach (HydroJebModule jeb in this.vessel.FindPartModulesImplementing<HydroJebModule>())
                    {
                        if (jeb == this) { continue; }
                        jeb.State = AutopilotStatus.Idle;
                    }
                }
            }
        }

        private void OnDestroy()
        {
            if (HighLogic.LoadedSceneIsEditor) { HydroToolbarManager.Editor.RemoveEnabler(); }
        }
        #endregion

        #region Overrides
        public override void OnStart(StartState state)
        {
            if (HighLogic.LoadedSceneIsEditor)
            {
                HydroToolbarManager.Editor.AddEnabler();
            }
            else if (HighLogic.LoadedSceneIsFlight)
            {
                this.state = EnumUtils.GetValue<AutopilotStatus>(this.status);
                if (this.IsOnline && (!this.vessel.isActiveVessel || this != this.vessel.GetMasterJeb()))
                {
                    this.State = AutopilotStatus.Idle;
                }
            }
        }

        public override string GetInfo()
        {
            StringBuilder sb = new StringBuilder("HydroJeb Autopilot Unit");
            sb.AppendLine("\n\n<b><color=#99ff00ff>Input:</color></b>");
            sb.AppendLine("ElectricCharge");
            sb.Append(string.Format("Rate: {0:0.###}U/s", this.consumptionRate));
            return sb.ToString();
        }
        #endregion
    }
}

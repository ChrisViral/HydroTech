using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using HydroTech.Managers;
using HydroTech.Utils;
using UnityEngine;
using static HydroTech.Extensions.VesselExtensions;
using static HydroTech.Extensions.PartExtensions;
using static HydroTech.Utils.HTUtils;

namespace HydroTech
{
    /// <summary>
    /// HydroTech autopilot core
    /// </summary>
    public class HydroJebCore : PartModule, IModuleInfo, IResourceConsumer
    {
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public enum AutopilotStatus
        {
            Online,     //Running
            Offline,    //Out of EC
            Idle        //Inactive/other core on vessel is active
        }

        #region KSPFields
        [KSPField]
        public float electricityConsumption = 0.1f;

        [KSPField(guiActive = true, guiName = "Status")]
        public string status = "Online";
        #endregion

        #region Properties
        private AutopilotStatus state;
        /// <summary>
        /// Current state
        /// </summary>
        public AutopilotStatus State
        {
            get { return this.state; }
            set
            {
                this.status = EnumUtils.GetName(value);
                this.state = value;
            }
        }

        /// <summary>
        /// If this core is currently online
        /// </summary>
        public bool IsOnline => this.state == AutopilotStatus.Online;
        #endregion

        #region Methods
        /// <summary>
        /// Module title
        /// </summary>
        /// <returns>Module title</returns>
        public string GetModuleTitle() => "HydroTech Autopilot";

        /// <summary>
        /// Unused
        /// </summary>
        /// <returns>Null</returns>
        public Callback<Rect> GetDrawModulePanelCallback() => null;

        /// <summary>
        /// Unused
        /// </summary>
        /// <returns>Empty string</returns>
        public string GetPrimaryField() => string.Empty;

        /// <summary>
        /// Used resources
        /// </summary>
        /// <returns>ElectricCharge</returns>
        public List<PartResourceDefinition> GetConsumedResources() => ElectrictyList;
        #endregion

        #region Functions
        /// <summary>
        /// Update function
        /// </summary>
        private void Update()
        {
            if (!HighLogic.LoadedSceneIsFlight || !this.vessel.loaded || !this.vessel.IsControllable) { return; }
            if (this == this.vessel.GetMasterJeb())
            {
                if (this.State == AutopilotStatus.Idle) { this.State = AutopilotStatus.Online; }
            }
            else if (this.IsOnline) { this.State = AutopilotStatus.Idle; }
        }

        /// <summary>
        /// FixedUpdate function
        /// </summary>
        private void FixedUpdate()
        {
            if (!HighLogic.LoadedSceneIsFlight || !this.IsOnline && this != this.vessel.GetMasterJeb() || !this.vessel.loaded || !this.vessel.IsControllable) { return; }

            if ((CheatOptions.InfiniteElectricity ? 1 : this.part.RequestResource(ElectricChargeID, this.electricityConsumption * TimeWarp.fixedDeltaTime)) <= 0)
            {
                //If out of EC
                if (this.IsOnline)
                {
                    this.vessel.FindPartModulesImplementing<HydroJebCore>().ForEach(m => m.State = AutopilotStatus.Offline);
                }
            }
            else
            {
                if (!this.IsOnline)
                {
                    //If returning from offline mode
                    this.State = AutopilotStatus.Online;
                    foreach (HydroJebCore jeb in this.vessel.parts.FindModulesImplementing<HydroJebCore>())
                    {
                        if (jeb == this) { continue; }
                        jeb.State = AutopilotStatus.Idle;
                    }
                }
            }
        }

        /// <summary>
        /// OnDestroy function
        /// </summary>
        private void OnDestroy()
        {
            if (HighLogic.LoadedSceneIsEditor) { HydroToolbarManager.Editor.RemoveEnabler(); }
        }
        #endregion

        #region Overrides
        /// <summary>
        /// Initializes module
        /// </summary>
        /// <param name="state">Current start state</param>
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

        /// <summary>
        /// Module info
        /// </summary>
        /// <returns>Info string</returns>
        public override string GetInfo()
        {
            StringBuilder sb = new StringBuilder("HydroJeb Autopilot Unit");
            sb.AppendLine("\n\n<b><color=#99ff00ff>Input:</color></b>");
            sb.AppendLine("ElectricCharge");
            sb.Append($"Rate: {this.electricityConsumption:0.###}U/s");
            return sb.ToString();
        }
        #endregion
    }
}

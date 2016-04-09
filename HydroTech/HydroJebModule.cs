using System.Linq;
using System.Text;
using HydroTech.Autopilots;
using HydroTech.Managers;
using HydroTech.Panels;
using HydroTech.Utils;
using KSP.UI.Screens;
using UnityEngine;
using AppScenes = KSP.UI.Screens.ApplicationLauncher.AppScenes;

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

        #region Static Properties
        protected static APDockAssist DA
        {
            get { return APDockAssist.TheAutopilot; }
        }
        #endregion

        #region KSPFields
        [KSPField]
        public float electricityRate = 0.1f;

        [KSPField(guiActive = true, guiName = "Status")]
        public string status = "Online";
        #endregion

        #region Fields
        private ApplicationLauncherButton button;
        private GameObject flightPanel;
        private bool added, visible;
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
        private void AddButton()
        {
            if (!this.added)
            {
                this.button = ApplicationLauncher.Instance.AddModApplication(ShowFlightPanel, HideFlightPanel,
                              Empty, Empty, Empty, Empty, AppScenes.FLIGHT, HTUtils.LauncherIcon);
                this.added = true;
            }
        }

        private void RemoveButton()
        {
            if (this.added)
            {
                ApplicationLauncher.Instance.RemoveModApplication(this.button);
                Destroy(this.button);
                this.added = false;
            }
        }

        private void HideButton()
        {
            this.button.SetFalse();
            this.button.VisibleInScenes = AppScenes.NEVER;
        }

        private void SetupModule()
        {
            AddButton();
            if (!this.vessel.isActiveVessel) { this.button.VisibleInScenes = AppScenes.NEVER; }
            GameEvents.onVesselSwitching.Add(SwitchingVessels);
            GameEvents.onGameSceneSwitchRequested.Add(GameSceneChanging);
        }

        private void ShowFlightPanel()
        {
            if (!this.visible)
            {
                this.flightPanel = new GameObject("FlightMainPanel", typeof(FlightMainPanel));
                this.visible = true;
            }
        }

        private void HideFlightPanel()
        {
            if (this.visible)
            {
                Destroy(this.flightPanel);
                this.visible = false;
            }
        }

        private void Empty() { }

        private void SwitchingVessels(Vessel from, Vessel to)
        {
            if (to == this.vessel) { this.button.VisibleInScenes = AppScenes.FLIGHT; }
            else if (from == this.vessel) { HideButton(); }
        }

        private void GameSceneChanging(GameEvents.FromToAction<GameScenes, GameScenes> evnt)
        {
            if (evnt.from == GameScenes.FLIGHT) { RemoveButton(); }
        }

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
            if (!HighLogic.LoadedSceneIsFlight || this.vessel.packed || !this.vessel.loaded || !this.vessel.IsControllable) { return; }
            if (this.state == AutopilotStatus.Idle && this == this.vessel.GetMasterJeb())
            {
                this.State = AutopilotStatus.Online;
            }
        }

        private void FixedUpdate()
        {
            if (!HighLogic.LoadedSceneIsFlight || !this.IsOnline || this.vessel.packed || !this.vessel.loaded || !this.vessel.IsControllable) { return; }

            if (!CheatOptions.InfiniteElectricity)
            {
                double amount = this.part.RequestResource(ecID, this.electricityRate * TimeWarp.fixedDeltaTime);
                if (amount <= 0)
                {
                    if (amount != 0) { this.part.RequestResource(ecID, -amount); }
                    if (this.State != AutopilotStatus.Offline)
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
        }

        private void OnDestroy()
        {
            if (HighLogic.LoadedSceneIsFlight && this.IsOnline)
            {
                RemoveButton();
                GameEvents.onVesselSwitching.Remove(SwitchingVessels);
                GameEvents.onGameSceneSwitchRequested.Remove(GameSceneChanging);
            }
            else if (HighLogic.LoadedSceneIsEditor) { EditorToolbarManager.RemoveEnabler(); }
        }
        #endregion

        #region Overrides
        public override void OnStart(StartState state)
        {
            if (HighLogic.LoadedSceneIsEditor)
            {
                EditorToolbarManager.AddEnabler();
            }
            else if (HighLogic.LoadedSceneIsFlight)
            {
                this.state = EnumUtils.GetValue<AutopilotStatus>(this.status);
                if (this.IsOnline)
                {
                    if (this == this.vessel.GetMasterJeb()) { SetupModule(); }
                    else { this.State = AutopilotStatus.Idle; }
                }
            }
        }

        public override string GetInfo()
        {
            StringBuilder sb = new StringBuilder("HydroJeb Autopilot Unit");
            sb.AppendLine("\n\n<b><color=#99ff00ff>Input:</color></b>");
            sb.AppendLine("ElectricCharge");
            sb.Append(string.Format("Rate: {0:0.#}U/s", this.electricityRate));
            return sb.ToString();
        }
        #endregion
    }
}

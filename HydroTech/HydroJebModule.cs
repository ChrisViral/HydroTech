using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        internal struct InputResource
        {
            #region Fields
            public readonly double rate;
            public readonly PartResourceDefinition resource;
            #endregion

            #region Constructors
            private InputResource(ConfigNode node)
            {
                string name = string.Empty;
                this.rate = 0;
                this.resource = node.TryGetValue("resourceName", ref name) ? PartResourceLibrary.Instance.resourceDefinitions.FirstOrDefault(r => r.name == name) : null;
                node.TryGetValue("rate", ref this.rate);
            }
            #endregion

            #region Static methods
            internal static bool TryGetResource(ConfigNode node, out InputResource input)
            {
                input = new InputResource(node);
                if (input.resource == null || input.rate <= 0)
                {
                    input = new InputResource();
                    Debug.LogError("[HydroJebModule]: Invalid resource name or invalid resource flow rate");
                    return false;
                }
                return true;
            }
            #endregion
        }

        #region KSPFields
        [KSPField(guiActive = true, guiName = "Status")]
        public string status = "Online";
        #endregion

        #region Fields
        private readonly List<InputResource> resources = new List<InputResource>();
        private ApplicationLauncherButton button;
        private GameObject flightPanel;
        private bool added, visible, online;
        private double[] requests;
        #endregion

        #region Methods
        private void SetOnline()
        {
            if (!this.online)
            {
                this.online = true;
                this.status = "Online";
            }
        }

        private void SetOffline(string resourceName)
        {
            if (this.online)
            {
                this.online = false;
                this.status = "Out of " + resourceName;
            }
        }

        private void LoadResources(ConfigNode node)
        {
            if (this.resources.Count == 0 && node.HasNode("INPUT"))
            {
                foreach (ConfigNode n in node.GetNodes("INPUT"))
                {
                    InputResource resource;
                    if (InputResource.TryGetResource(n, out resource)) { this.resources.Add(resource); }
                }
            }
        }

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
            else if (from == this.vessel)
            {
                this.button.SetFalse();
                this.button.VisibleInScenes = AppScenes.NEVER;
            }
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
        private void FixedUpdate()
        {
            if (!HighLogic.LoadedSceneIsFlight) { return; }

            if (!CheatOptions.InfinitePropellant)
            {
                for (int i = 0; i < this.resources.Count; i++)
                {
                    InputResource res = this.resources[i];
                    this.requests[i] = this.part.RequestResource(res.resource.id, res.rate * TimeWarp.fixedDeltaTime);
                    if (this.requests[i] <= 0)
                    {
                        for (int j = 0; j <= i; j++)
                        {
                            this.part.RequestResource(this.resources[j].resource.id, -this.requests[j]);
                        }
                        SetOffline(res.resource.name);
                        break;
                    }
                }
                SetOnline();
            }
        }

        private void OnDestroy()
        {
            if (HighLogic.LoadedSceneIsFlight)
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
                AddButton();
                if (!this.vessel.isActiveVessel) { this.button.VisibleInScenes = AppScenes.NEVER; }
                this.requests = new double[this.resources.Count];
                GameEvents.onVesselSwitching.Add(SwitchingVessels);
                GameEvents.onGameSceneSwitchRequested.Add(GameSceneChanging);
            }
        }

        public override void OnLoad(ConfigNode node)
        {
            if (HighLogic.LoadedScene == GameScenes.LOADING)
            {
                PersistentManager.Instance.AddNode<HydroJebModule>(this.part.name, node);
                LoadResources(node);
            }
            else if(node != null || PersistentManager.Instance.TryGetNode<HydroJebModule>(this.part.name, ref node))
            {
                LoadResources(node);
            }
        }

        public override string GetInfo()
        {
            StringBuilder sb = new StringBuilder("HydroJeb Autopilot Unit");
            if (this.resources.Count > 0)
            {
                foreach (InputResource res in this.resources)
                {
                    sb.Append("\n\n<b><color=#99ff00ff>Input:</color></b>");
                    sb.AppendLine(res.resource.name);
                    sb.Append(string.Format("Rate: {0:0.#}U/s", res.rate));
                }
            }
            return sb.ToString();
        }
        #endregion
    }
}

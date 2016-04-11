using HydroTech.Panels;
using HydroTech.Utils;
using KSP.UI.Screens;
using UnityEngine;
using AppScenes = KSP.UI.Screens.ApplicationLauncher.AppScenes;

namespace HydroTech.Managers
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class HydroToolbarManager : MonoBehaviour
    {
        public class EditorToolbar
        {
            #region Fields
            private const AppScenes editors = AppScenes.VAB | AppScenes.SPH;
            private ApplicationLauncherButton button;
            private GameObject panel;
            private bool added, visible;
            private int enablers;
            #endregion

            #region Constructors
            public EditorToolbar()
            {
                GameEvents.onGUIApplicationLauncherReady.Add(AddButton);
                GameEvents.onGUIApplicationLauncherDestroyed.Add(RemoveButton);
                GameEvents.onGameSceneSwitchRequested.Add(GameSceneChanging);
            }
            #endregion

            #region Methods
            private void AddButton()
            {
                if (!this.added)
                {
                    this.button = ApplicationLauncher.Instance.AddModApplication(ShowPanel, HidePanel,
                             Empty, Empty, Empty, Empty, AppScenes.NEVER, HTUtils.LauncherIcon);
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
                    this.visible = false;
                }
            }

            private void ShowPanel()
            {
                if (!this.visible)
                {
                    this.panel = new GameObject("EditorMainPanel", typeof(EditorMainPanel));
                    this.visible = true;
                }
            }

            private void HidePanel()
            {
                if (this.visible)
                {
                    Destroy(this.panel);
                    this.visible = false;
                }
            }

            private void Empty() { }

            private void GameSceneChanging(GameEvents.FromToAction<GameScenes, GameScenes> evnt)
            {
                if (evnt.from == GameScenes.EDITOR)
                {
                    if (this.visible) { this.button.SetFalse(); }
                    this.button.Disable();
                }
                else if (evnt.to == GameScenes.EDITOR) { this.enablers = 0; }
            }

            public void AddEnabler()
            {
                if (++this.enablers == 1)
                {
                    this.button.VisibleInScenes = editors;
                    HydroEditorManager.Instance.SetActive(true);
                }
            }

            public void RemoveEnabler()
            {
                if (--this.enablers == 0)
                {
                    if (this.visible) { this.button.SetFalse(); }
                    this.button.VisibleInScenes = AppScenes.NEVER;
                    HydroEditorManager.Instance.SetActive(false);
                }
            }
            #endregion
        }

        public class FlightToolbar
        {
            #region Fields
            private ApplicationLauncherButton button;
            private HydroJebModule module;
            private GameObject panel;
            private bool visible, added, enabled;
            #endregion

            #region Constructors
            public FlightToolbar()
            {
                GameEvents.onGUIApplicationLauncherReady.Add(AddButton);
                GameEvents.onGUIApplicationLauncherDestroyed.Add(RemoveButton);
                GameEvents.onVesselSwitching.Add(SwitchingVessels);
                GameEvents.onGameSceneSwitchRequested.Add(GameSceneChanging);
            }
            #endregion

            #region Methods
            private void AddButton()
            {
                if (!this.added)
                {
                    this.button = ApplicationLauncher.Instance.AddModApplication(ShowPanel, HidePanel,
                             Empty, Empty, SetActive, SetInactive, AppScenes.NEVER, HTUtils.LauncherIcon);
                    this.enabled = true;
                    this.added = true;
                }
            }

            private void RemoveButton()
            {
                if (this.added)
                {
                    ApplicationLauncher.Instance.RemoveModApplication(this.button);
                    Destroy(this.button);
                    this.module = null;
                    this.added = false;
                    this.visible = false;
                }
            }

            private void ShowPanel()
            {
                if (this.enabled)
                {
                    if (!this.visible)
                    {
                        this.panel = new GameObject("FlightMainPanel", typeof(FlightMainPanel));
                        this.panel.GetComponent<FlightMainPanel>().SetModule(this.module);
                        this.visible = true;
                    }
                }
                else { this.button.SetFalse(false); }
            }

            private void HidePanel()
            {
                if (this.visible && this.enabled)
                {
                    Destroy(this.panel);
                    this.visible = false;
                }
            }

            private void SetActive()
            {
                if (!this.enabled)
                {
                    this.enabled = true;
                    this.button.SetTexture(HTUtils.LauncherIcon);
                }
            }

            private void SetInactive()
            {
                if (this.enabled)
                {
                    this.button.SetFalse();
                    this.enabled = false;
                    this.button.SetTexture(HTUtils.InactiveIcon);
                }
            }

            private void Empty() { }

            private void SetSubscription(HydroJebModule jeb)
            {
                this.module = jeb;
                if (this.module == null)
                {
                    this.button.VisibleInScenes = AppScenes.NEVER;
                    this.button.Enable();
                    this.button.SetFalse();
                }
                else { this.button.VisibleInScenes = AppScenes.FLIGHT; }
            }

            public bool IsActive(HydroJebModule jeb)
            {
                return this.module == jeb;
            }

            private void SwitchingVessels(Vessel from, Vessel to)
            {
                SetSubscription(to.GetMasterJeb());
            }

            private void GameSceneChanging(GameEvents.FromToAction<GameScenes, GameScenes> evnt)
            {
                if (evnt.from == GameScenes.FLIGHT && this.module != null)
                {
                    this.module = null;
                    this.button.VisibleInScenes = AppScenes.NEVER;
                }
            }    
            #endregion

            #region Functions
            internal void Update()
            {
                HydroJebModule jeb = FlightGlobals.ActiveVessel.GetMasterJeb();
                if (jeb != this.module)
                {
                    SetSubscription(jeb);
                }
                if (this.module != null)
                {
                    if (this.enabled)
                    {
                        if (!this.module.IsOnline)
                        {
                            this.button.SetFalse();
                            this.button.Disable();
                            this.enabled = false;
                            this.button.SetTexture(HTUtils.InactiveIcon);
                        }
                    }
                    else if (this.module.IsOnline)
                    {
                        this.button.Enable();
                    }
                }
            }
            #endregion
        }

        #region Static properties
        public static EditorToolbar Editor { get; private set; }
        public static FlightToolbar Flight { get; private set; }
        #endregion

        #region Functions
        private void Awake()
        {
            Editor = new EditorToolbar();
            Flight = new FlightToolbar();
        }
        #endregion
    }

    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class HydroFlightHandler : MonoBehaviour
    {
        #region Functions
        private void Update()
        {
            HydroToolbarManager.Flight.Update();
        }
        #endregion
    }
}

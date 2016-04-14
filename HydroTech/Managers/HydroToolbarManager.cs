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
            #region Constants
            private const AppScenes editors = AppScenes.VAB | AppScenes.SPH;
            #endregion

            #region Fields
            private ApplicationLauncherButton button;
            private GameObject go;
            private EditorMainPanel panel;
            private bool added;
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
                    this.go = new GameObject("EditorMainPanel", typeof(EditorMainPanel));
                    DontDestroyOnLoad(this.go);
                    this.panel = this.go.GetComponent<EditorMainPanel>();
                    this.button = ApplicationLauncher.Instance.AddModApplication(this.panel.ShowPanel, this.panel.HidePanel,
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
                    Destroy(this.go);
                    this.added = false;
                }
            }

            private void Empty() { }

            private void GameSceneChanging(GameEvents.FromToAction<GameScenes, GameScenes> evnt)
            {
                if (evnt.from == GameScenes.EDITOR)
                {
                    this.button.SetFalse();
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
                    this.button.SetFalse();
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
            private HydroJebCore core;
            private GameObject go;
            private FlightMainPanel panel;
            private bool added, enabled;
            #endregion

            #region Constructors
            public FlightToolbar()
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
                    this.go = new GameObject("FlightMainPanel", typeof(FlightMainPanel));
                    DontDestroyOnLoad(this.go);
                    this.panel = this.go.GetComponent<FlightMainPanel>();
                    this.button = ApplicationLauncher.Instance.AddModApplication(this.panel.ShowPanel, this.panel.HidePanel,
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
                    Destroy(this.go);
                    this.core = null;
                    this.added = false;
                }
            }

            private void Empty() { }

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

            public bool IsActive(HydroJebCore jeb)
            {
                return this.core == jeb;
            }

            private void GameSceneChanging(GameEvents.FromToAction<GameScenes, GameScenes> evnt)
            {
                if (evnt.from == GameScenes.FLIGHT && this.core != null)
                {
                    this.core = null;
                    this.button.VisibleInScenes = AppScenes.NEVER;
                }
            }

            internal void Update(HydroJebCore jeb)
            {
                if (this.core != jeb)
                {
                    this.core = jeb;
                    if (this.core == null)
                    {
                        this.button.VisibleInScenes = AppScenes.NEVER;
                        this.button.Enable();
                        this.button.SetFalse();
                    }
                    else { this.button.VisibleInScenes = AppScenes.FLIGHT; }
                }
                if (this.core != null)
                {
                    if (this.enabled)
                    {
                        if (!this.core.IsOnline)
                        {
                            this.button.SetFalse();
                            this.button.Disable();
                            this.enabled = false;
                            this.button.SetTexture(HTUtils.InactiveIcon);
                        }
                    }
                    else if (this.core.IsOnline)
                    {
                        this.button.Enable();
                    }
                }
            }
            #endregion
        }

        public class SettingsToolbar
        {
            #region Fields
            private ApplicationLauncherButton button;
            private GameObject go;
            private SettingsPanel panel;
            private bool added;
            #endregion

            #region Constructors
            public SettingsToolbar()
            {
                GameEvents.onGUIApplicationLauncherReady.Add(AddButton);
                GameEvents.onGUIApplicationLauncherDestroyed.Add(RemoveButton);
            }
            #endregion

            #region Methods
            private void AddButton()
            {
                if (!this.added)
                {
                    this.go = new GameObject("SettingsPanel", typeof(SettingsPanel));
                    DontDestroyOnLoad(this.go);
                    this.panel = this.go.GetComponent<SettingsPanel>();
                    this.button = ApplicationLauncher.Instance.AddModApplication(this.panel.ShowPanel, this.panel.HidePanel,
                                  Empty, Empty, Empty, Empty, AppScenes.SPACECENTER, HTUtils.LauncherIcon);
                    this.added = true;
                }
            }

            private void RemoveButton()
            {
                if (this.added)
                {
                    ApplicationLauncher.Instance.RemoveModApplication(this.button);
                    Destroy(this.button);
                    Destroy(this.go);
                    this.added = false;
                }
            }

            private void Empty() { }

            public void SetFalse()
            {
                this.button.SetFalse();
            }
            #endregion
        }

        #region Static properties
        public static EditorToolbar Editor { get; private set; }

        public static FlightToolbar Flight { get; private set; }

        public static SettingsToolbar Settings { get; private set; }
        #endregion

        #region Functions
        private void Awake()
        {
            Editor = new EditorToolbar();
            Flight = new FlightToolbar();
            Settings = new SettingsToolbar();
        }
        #endregion
    }
}

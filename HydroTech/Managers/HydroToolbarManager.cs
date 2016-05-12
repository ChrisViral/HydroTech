using HydroTech.Panels;
using HydroTech.Utils;
using KSP.UI.Screens;
using UnityEngine;
using AppScenes = KSP.UI.Screens.ApplicationLauncher.AppScenes;
using FromToAction = GameEvents.FromToAction<GameScenes, GameScenes>;

namespace HydroTech.Managers
{
    /// <summary>
    /// AppLauncher manager for HydroTech buttons
    /// </summary>
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class HydroToolbarManager : MonoBehaviour
    {
        /// <summary>
        /// Editor button manager
        /// </summary>
        public class EditorToolbar
        {
            #region Constants
            //Editor app scenes
            private const AppScenes editors = AppScenes.VAB | AppScenes.SPH;
            #endregion

            #region Fields
            internal ApplicationLauncherButton button;  //Button
            private EditorMainPanel panel;              //Panel
            private bool added;                         //Added flag
            private int enablers;                       //Active flag
            #endregion

            #region Constructors
            /// <summary>
            /// Initiates the toolbar manager
            /// </summary>
            public EditorToolbar()
            {
                GameEvents.onGUIApplicationLauncherReady.Add(AddButton);
                GameEvents.onGUIApplicationLauncherDestroyed.Add(RemoveButton);
                GameEvents.onGameSceneSwitchRequested.Add(GameSceneChanging);
            }
            #endregion

            #region Methods
            /// <summary>
            /// Adds the button to the AppLauncher
            /// </summary>
            private void AddButton()
            {
                if (!this.added)
                {
                    this.panel = go.AddComponent<EditorMainPanel>();
                    this.button = ApplicationLauncher.Instance.AddModApplication(this.panel.ShowPanel, this.panel.HidePanel,
                                  Empty, Empty, Empty, Empty, AppScenes.NEVER, HTUtils.LauncherIcon);
                    this.added = true;
                }
            }
            
            /// <summary>
            /// Removes the button from the AppLauncher
            /// </summary>
            private void RemoveButton()
            {
                if (this.added)
                {
                    ApplicationLauncher.Instance.RemoveModApplication(this.button);
                    Destroy(this.button);
                    Destroy(this.panel);
                    this.added = false;
                }
            }

            /// <summary>
            /// Empty filler method
            /// </summary>
            private void Empty() { }

            /// <summary>
            /// Checkup when changing scenes
            /// </summary>
            /// <param name="evnt">From/to scenes</param>
            private void GameSceneChanging(FromToAction evnt)
            {
                if (evnt.from == GameScenes.EDITOR)
                {
                    this.button.SetFalse();
                    this.button.Disable();
                }
                else if (evnt.to == GameScenes.EDITOR) { this.enablers = 0; }
            }

            /// <summary>
            /// Adds an active core to keep button active
            /// </summary>
            public void AddEnabler()
            {
                if (++this.enablers == 1)
                {
                    this.button.VisibleInScenes = editors;
                    HydroEditorManager.Instance.SetActive(true);
                }
            }

            /// <summary>
            /// Removes an active core to keep button active
            /// </summary>
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

        /// <summary>
        /// Flight button manager
        /// </summary>
        public class FlightToolbar
        {
            #region Fields
            internal ApplicationLauncherButton button;  //Button
            private HydroJebCore core;                  //Active HydroTechCore
            private FlightMainPanel panel;              //Panel
            private bool added, enabled;                //Visible flags
            #endregion

            #region Constructors
            /// <summary>
            /// Initiates the toolbar manager
            /// </summary>
            public FlightToolbar()
            {
                GameEvents.onGUIApplicationLauncherReady.Add(AddButton);
                GameEvents.onGUIApplicationLauncherDestroyed.Add(RemoveButton);
                GameEvents.onGameSceneSwitchRequested.Add(GameSceneChanging);
            }
            #endregion

            #region Methods
            /// <summary>
            /// Adds the button to the AppLauncher
            /// </summary>
            private void AddButton()
            {
                if (!this.added)
                {
                    this.panel = go.AddComponent<FlightMainPanel>();
                    this.button = ApplicationLauncher.Instance.AddModApplication(this.panel.ShowPanel, this.panel.HidePanel,
                                  Empty, Empty, SetActive, SetInactive, AppScenes.NEVER, HTUtils.LauncherIcon);
                    this.enabled = true;
                    this.added = true;
                }
            }

            /// <summary>
            /// Removes the button from the AppLauncher
            /// </summary>
            private void RemoveButton()
            {
                if (this.added)
                {
                    ApplicationLauncher.Instance.RemoveModApplication(this.button);
                    Destroy(this.button);
                    Destroy(this.panel);
                    this.core = null;
                    this.added = false;
                }
            }

            /// <summary>
            /// Empty filler method
            /// </summary>
            private void Empty() { }

            /// <summary>
            /// OnActive event delegate
            /// </summary>
            private void SetActive()
            {
                if (this.button != null)
                {
                    this.enabled = true;
                    this.button.SetTexture(HTUtils.LauncherIcon);
                }
            }

            /// <summary>
            /// OnInactive event delegate
            /// </summary>
            private void SetInactive()
            {
                if (this.button != null)
                {
                    this.button.SetFalse();
                    this.enabled = false;
                    this.button.SetTexture(HTUtils.InactiveIcon);
                }
            }

            /// <summary>
            /// If a given core is the active core
            /// </summary>
            /// <param name="jeb">Core to check</param>
            /// <returns>If the core is the active one</returns>
            public bool IsActive(HydroJebCore jeb) => this.core == jeb;

            /// <summary>
            /// Checkup when changing scenes
            /// </summary>
            /// <param name="evnt">From/to scenes</param>
            private void GameSceneChanging(FromToAction evnt)
            {
                if (evnt.from == GameScenes.FLIGHT && this.core != null)
                {
                    this.core = null;
                    this.button.VisibleInScenes = AppScenes.NEVER;
                }
            }

            /// <summary>
            /// Update function
            /// </summary>
            /// <param name="jeb">Core to update for</param>
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

        /// <summary>
        /// Settings button manager
        /// </summary>
        public class SettingsToolbar
        {
            #region Fields
            internal ApplicationLauncherButton button;  //Button
            private SettingsPanel panel;                //Panel
            private bool added;                         //Added flag
            #endregion

            #region Constructors
            /// <summary>
            /// Initiates the toolbar manager
            /// </summary>
            public SettingsToolbar()
            {
                GameEvents.onGUIApplicationLauncherReady.Add(AddButton);
                GameEvents.onGUIApplicationLauncherDestroyed.Add(RemoveButton);
            }
            #endregion

            #region Methods
            /// <summary>
            /// Adds the button to the AppLauncher
            /// </summary>
            private void AddButton()
            {
                if (!this.added)
                {
                    this.panel = go.AddComponent<SettingsPanel>();
                    this.button = ApplicationLauncher.Instance.AddModApplication(this.panel.ShowPanel, this.panel.HidePanel,
                                  Empty, Empty, Empty, Empty, AppScenes.SPACECENTER, HTUtils.LauncherIcon);
                    this.added = true;
                }
            }

            /// <summary>
            /// Removes the button from the AppLauncher
            /// </summary>
            private void RemoveButton()
            {
                if (this.added)
                {
                    ApplicationLauncher.Instance.RemoveModApplication(this.button);
                    Destroy(this.button);
                    Destroy(this.panel);
                    this.added = false;
                }
            }

            /// <summary>
            /// Empty filler method
            /// </summary>
            private void Empty() { }
            #endregion
        }

        #region Static fields
        private static GameObject go; //GameObject to which the main panels are attached
        #endregion

        #region Static properties
        /// <summary>
        /// Current editor toolbar
        /// </summary>
        public static EditorToolbar Editor { get; private set; }

        /// <summary>
        /// Current flight toolbar
        /// </summary>
        public static FlightToolbar Flight { get; private set; }

        /// <summary>
        /// Current settings toolbar
        /// </summary>
        public static SettingsToolbar Settings { get; private set; }
        #endregion

        #region Static methods
        /// <summary>
        /// Closes the editor button
        /// </summary>
        public static void CloseEditor() => Editor.button.SetFalse();

        /// <summary>
        /// Closes the flight button
        /// </summary>
        public static void CloseFlight() => Flight.button.SetFalse();

        /// <summary>
        /// Closes the settings button
        /// </summary>
        public static void CloseSettings() => Settings.button.SetFalse();
        #endregion

        #region Functions
        /// <summary>
        /// Initialization - Awake function
        /// </summary>
        private void Awake()
        {
            if (go != null) { Destroy(this); return; }

            go = new GameObject("HydroTechPanels");
            DontDestroyOnLoad(go);

            Editor = new EditorToolbar();
            Flight = new FlightToolbar();
            Settings = new SettingsToolbar();
        }
        #endregion
    }
}

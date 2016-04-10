using HydroTech.Panels;
using HydroTech.Utils;
using KSP.UI.Screens;
using UnityEngine;
using AppScenes = KSP.UI.Screens.ApplicationLauncher.AppScenes;

namespace HydroTech.Managers
{
    internal interface IToolbarButton
    {
        void AddButton();

        void RemoveButton();
    }

    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class HydroToolbarManager : MonoBehaviour
    {
        public class EditorToolbar
        {
            #region Static fields
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
                    this.button.enabled = false;
                    this.added = true;
                }
            }

            private void RemoveButton()
            {
                ApplicationLauncher.Instance.RemoveModApplication(this.button);
                Destroy(this.button);
                this.added = false;
                this.visible = false;
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
                }
            }

            public void RemoveEnabler()
            {
                if (--this.enablers == 0)
                {
                    if (this.visible) { this.button.SetFalse(); }
                    this.button.VisibleInScenes = AppScenes.NEVER;
                }
            }
            #endregion
        }

        public class FlightToolbar
        {
            public FlightToolbar()
            {
                
            }
        }

        public static EditorToolbar editor;
        public static FlightToolbar flight;

        private void Awake()
        {
            editor = new EditorToolbar();
            flight = new FlightToolbar();
        }
    }
}

using HydroTech.Panels;
using HydroTech.Utils;
using KSP.UI.Screens;
using UnityEngine;
using AppScenes = KSP.UI.Screens.ApplicationLauncher.AppScenes;

namespace HydroTech.Managers
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class EditorToolbarManager : MonoBehaviour
    {
        #region Static fields
        private const AppScenes editors = AppScenes.VAB | AppScenes.SPH;
        private static ApplicationLauncherButton button;
        private static GameObject panel;
        private static bool added, visible;
        private static int enablers;
        #endregion

        #region Methods
        private void AddButton()
        {
            if (!added)
            {
                button = ApplicationLauncher.Instance.AddModApplication(ShowPanel, HidePanel,
                         Empty, Empty, Empty, Empty, AppScenes.NEVER ,HTUtils.LauncherIcon);
                button.enabled = false;
                added = true;
            }
        }

        private void RemoveButton()
        {
            ApplicationLauncher.Instance.RemoveModApplication(button);
            Destroy(button);
            added = false;
            visible = false;
        }

        private void ShowPanel()
        {
            if (!visible)
            {
                panel = new GameObject("EditorMainPanel", typeof(EditorMainPanel));
                visible = true;
            }
        }

        private void HidePanel()
        {
            if (visible)
            {
                Destroy(panel);
                visible = false;
            }
        }

        private void Empty() { }

        private void GameSceneChanging(GameEvents.FromToAction<GameScenes, GameScenes> evnt)
        {
            if (evnt.from == GameScenes.EDITOR)
            {
                if (visible) { button.SetFalse(); }
                button.Disable();
            }
            else if (evnt.to == GameScenes.EDITOR) { enablers = 0; }
        }
        #endregion

        #region Static methods
        public static void AddEnabler()
        {
            if (++enablers == 1)
            {
                button.VisibleInScenes = editors;
            }
        }

        public static void RemoveEnabler()
        {
            if (--enablers == 0)
            {
                if (visible) { button.SetFalse(); }
                button.VisibleInScenes = AppScenes.NEVER;
            }
        }
        #endregion

        #region Functions
        private void Awake()
        {
            GameEvents.onGUIApplicationLauncherReady.Add(AddButton);
            GameEvents.onGUIApplicationLauncherDestroyed.Add(RemoveButton);
            GameEvents.onGameSceneSwitchRequested.Add(GameSceneChanging);
        }
        #endregion
    }
}

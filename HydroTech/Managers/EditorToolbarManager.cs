using HydroTech.Panels;
using HydroTech.Utils;
using KSP.UI.Screens;
using UnityEngine;

namespace HydroTech.Managers
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class EditorToolbarManager : MonoBehaviour
    {
        #region Static fields
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
                button = ApplicationLauncher.Instance.AddModApplication(ShowPanel, HidePanel, Empty, Empty, Empty, Empty,
                         ApplicationLauncher.AppScenes.VAB,HTUtils.LauncherIcon);
                button.Disable();
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

        private void GameSceneChanged(GameEvents.FromToAction<GameScenes, GameScenes> evnt)
        {
            if (evnt.from == GameScenes.EDITOR)
            {
                if (visible) { button.SetFalse(); }
                button.Disable();
            }
            else if (evnt.to == GameScenes.EDITOR) { enablers = 0;}
        }
        #endregion

        #region Static methods
        public static void AddEnabler()
        {
            if (++enablers == 1)
            {
                button.Enable();
            }
        }

        public static void RemoveEnabler()
        {
            if (--enablers == 0)
            {
                if (visible) { button.SetFalse(); }
                button.Disable();
            }
        }
        #endregion

        #region Functions
        private void Awake()
        {
            GameEvents.onGUIApplicationLauncherReady.Add(AddButton);
            GameEvents.onGUIApplicationLauncherDestroyed.Add(RemoveButton);
            GameEvents.onGameSceneSwitchRequested.Add(GameSceneChanged);
        }
        #endregion
    }
}

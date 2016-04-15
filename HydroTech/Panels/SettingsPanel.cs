using HydroTech.Managers;
using HydroTech.Utils;
using UnityEngine;

namespace HydroTech.Panels
{
    public class SettingsPanel : MonoBehaviour
    {
        #region Instance
        public static SettingsPanel Instance { get; private set; }
        #endregion

        #region Fields
        private Rect pos, drag;
        private bool visible, hid;
        private int id;
        #endregion

        #region Methods
        internal void ShowPanel()
        {
            if (!this.visible && !this.hid) { this.visible = true; }
        }

        internal void HidePanel()
        {
            if (this.visible) { this.visible = false; }
        }

        private void ShowUI()
        {
            this.hid = false;
        }

        private void HideUI()
        {
            this.hid = true;
        }

        private void Window(int id)
        {
            GUI.DragWindow(this.drag);
            
            GUILayout.BeginVertical(GUI.skin.box);
            //Insert settings
            GUILayout.EndVertical();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Save"))
            {
                
            }
            if (GUILayout.Button("Close"))
            {
                HydroToolbarManager.Settings.SetFalse();
            }
            GUILayout.EndHorizontal();
        }
        #endregion

        #region Functions
        private void Awake()
        {
            if (Instance != null) { Destroy(this); return; }

            Instance = this;
            this.pos = new Rect(Screen.width * 0.2f, Screen.height * 0.2f, 250, 50);
            this.drag = new Rect(0, 0, 250, 30);
            this.id = GuidProvider.GetGuid<SettingsPanel>();
            GameEvents.onShowUI.Add(ShowUI);
            GameEvents.onHideUI.Add(HideUI);
            GameEvents.onGUIAstronautComplexSpawn.Add(HideUI);
            GameEvents.onGUIAstronautComplexDespawn.Add(ShowUI);
            GameEvents.onGUIRnDComplexSpawn.Add(HideUI);
            GameEvents.onGUIRnDComplexDespawn.Add(ShowUI);
            GameEvents.onGUIMissionControlSpawn.Add(HideUI);
            GameEvents.onGUIMissionControlDespawn.Add(ShowUI);
            GameEvents.onGUIAdministrationFacilitySpawn.Add(HideUI);
            GameEvents.onGUIAdministrationFacilityDespawn.Add(ShowUI);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
                GameEvents.onShowUI.Remove(ShowUI);
                GameEvents.onHideUI.Remove(HideUI);
                GameEvents.onGUIAstronautComplexSpawn.Remove(HideUI);
                GameEvents.onGUIAstronautComplexDespawn.Remove(ShowUI);
                GameEvents.onGUIRnDComplexSpawn.Remove(HideUI);
                GameEvents.onGUIRnDComplexDespawn.Remove(ShowUI);
                GameEvents.onGUIMissionControlSpawn.Remove(HideUI);
                GameEvents.onGUIMissionControlDespawn.Remove(ShowUI);
                GameEvents.onGUIAdministrationFacilitySpawn.Remove(HideUI);
                GameEvents.onGUIAdministrationFacilityDespawn.Remove(ShowUI);
            }
        }

        private void OnGUI()
        {
            if (this.visible && !this.hid)
            {
                GUI.skin = GUIUtils.Skin;

                this.pos = KSPUtil.ClampRectToScreen(GUILayout.Window(this.id, this.pos, Window, "HydroTech Settings"));
            }
        }
        #endregion
    }
}

using System.Collections.Generic;
using HydroTech.Utils;
using UnityEngine;

namespace HydroTech.Panels
{
    public class EditorMainPanel : MonoBehaviour
    {
        #region Instance
        public static EditorMainPanel Instance { get; private set; }
        #endregion

        #region Fields
        private Rect pos, drag;
        private bool visible;
        private int id;
        #endregion

        #region Properties
        public PanelDockAssistEditor DockAssist { get; private set; }

        public PanelRCSThrustInfo RCSInfo { get; private set; }

        public List<Panel> Panels { get; private set; }
        #endregion

        #region Methods
        internal void ShowPanel()
        {
            if (!this.visible) { this.visible = true; }
        }

        internal void HidePanel()
        {
            if (this.visible) { this.visible = false; }
        }

        private void Window(int id)
        {
            GUI.DragWindow(this.drag);

            GUILayout.BeginVertical();

            this.DockAssist.Active = GUILayout.Toggle(this.DockAssist.Active, "Docking Assist Window", GUI.skin.button);

            this.RCSInfo.Active = GUILayout.Toggle(this.RCSInfo.Active, "RCS Info Window", GUI.skin.button);

            GUILayout.EndVertical();
        }
        #endregion

        #region Functions
        private void Awake()
        {
            if (Instance != null) { Destroy(this); return; }

            Instance = this;
            this.DockAssist = new PanelDockAssistEditor();
            this.RCSInfo = new PanelRCSThrustInfo(true);
            this.Panels = new List<Panel>(2)
            {
                this.DockAssist,
                this.RCSInfo
            };
            this.pos = new Rect(Screen.width * 0.8f, Screen.height * 0.2f, 250, 100);
            this.drag = new Rect(0, 0, 200, 30);
            this.id = GuidProvider.GetGuid<EditorMainPanel>();
        }

        private void OnDestroy()
        {
            if (Instance == this) { Instance = null; }
        }

        private void OnGUI()
        {
            if (this.visible)
            {
                this.pos = GUILayout.Window(this.id, this.pos, Window, "HydroTech Editor Panel");
            }
        }
        #endregion
    }
}

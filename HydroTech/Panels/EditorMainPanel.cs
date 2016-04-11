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
        private Rect pos = new Rect(Screen.width / 2f, Screen.height / 2f, 20, 20);
        private bool visible;
        #endregion

        #region Properties
        public PanelDockAssistEditor EditorDockAssist { get; private set; }

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
        #endregion

        #region Functions
        private void Awake()
        {
            if (Instance != null) { Destroy(this); return; }

            Instance = this;
            this.EditorDockAssist = new PanelDockAssistEditor();
            this.RCSInfo = new PanelRCSThrustInfo();
            this.Panels = new List<Panel>(2)
            {
                this.EditorDockAssist,
                this.RCSInfo
            };
        }

        private void OnDestroy()
        {
            if (Instance == this) { Instance = null; }
        }

        private void OnGUI()
        {
            if (this.visible)
            {
                GUI.Label(this.pos, ":D", GUIUtils.Skin.label);
            }
        }
        #endregion
    }
}

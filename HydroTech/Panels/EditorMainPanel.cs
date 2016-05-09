using HydroTech.Managers;
using HydroTech.Utils;
using UnityEngine;

namespace HydroTech.Panels
{
    /// <summary>
    /// The main editor GUI panel, controlling the visibility of the specific panels
    /// </summary>
    public class EditorMainPanel : MainPanel
    {
        #region Instance
        /// <summary>
        /// The current instance of the main panel
        /// </summary>
        public static EditorMainPanel Instance { get; private set; }
        #endregion

        #region Properties
        /// <summary>
        /// The current instance of the Dock Assist panel
        /// </summary>
        public PanelDockAssistEditor DockAssist { get; private set; }

        /// <summary>
        /// The current instance of the RCS Info panel
        /// </summary>
        public PanelRCSThrustInfo RCSInfo { get; private set; }

        /// <summary>
        /// Title of this MainPanel
        /// </summary>
        protected override string Title => "HydroTech Editor Panel";
        #endregion

        #region Functions
        /// <summary>
        /// Unity Awake function
        /// </summary>
        private void Awake()
        {
            if (Instance != null) { Destroy(this); return; }

            Instance = this;
            this.DockAssist = new PanelDockAssistEditor();
            this.RCSInfo = new PanelRCSThrustInfo(true);

            this.Panels.Add(this.DockAssist);
            this.Panels.Add(this.RCSInfo);

            this.pos = new Rect(Screen.width * 0.8f, Screen.height * 0.2f, 250, 50);
            this.drag = new Rect(0, 0, 250, 30);
            this.id = GUIUtils.GetID<EditorMainPanel>();
            this.close = HydroToolbarManager.CloseEditor;
        }

        /// <summary>
        /// Unity OnDestroy function
        /// </summary>
        private void OnDestroy()
        {
            if (Instance == this) { Instance = null; }
        }
        #endregion
    }
}

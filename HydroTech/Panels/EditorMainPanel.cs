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
        private Rect pos = new Rect(Screen.width / 2, Screen.height / 2, 20, 20);
        #endregion

        #region Properties
        public PanelDockAssistEditor EditorDockAssist { get; private set; }

        public PanelRCSThrustInfo RCSInfo { get; private set; }

        public List<Panel> Panels { get; private set; }
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

        private void OnGUI()
        {
            GUI.Label(this.pos, ":D", GUIUtils.Skin.label);
        }
        #endregion
    }
}

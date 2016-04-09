using HydroTech.Constants;
using HydroTech.Storage;
using UnityEngine;

namespace HydroTech.Panels
{
    public class PanelMain : Panel
    {
        #region Properties
        protected override int PanelID
        {
            get { return CoreConsts.main; }
        }

        public override string PanelTitle
        {
            get { return PanelConsts.mainTitle; }
        }
        #endregion

        #region Constructor
        public PanelMain()
        {
            this.fileName = new FileName("main", "cfg", HydroJebCore.panelSaveFolder);
        }
        #endregion

        #region Overrides
        protected override void LoadDefault()
        {
            base.LoadDefault();
            this.panelShown = true;
        }

        protected override void SetDefaultWindowRect()
        {
            this.windowRect = PanelConsts.main;
        }

        protected override void WindowGUI(int windowId)
        {
            GUILayout.BeginVertical();
            foreach (Panel panel in HydroJebCore.panels.Values)
            {
                if (panel == this) { continue; }
                panel.PanelShown = GUILayout.Toggle(panel.PanelShown, panel.PanelTitle);
            }
            GUILayout.EndVertical();
        }
        #endregion
    }
}
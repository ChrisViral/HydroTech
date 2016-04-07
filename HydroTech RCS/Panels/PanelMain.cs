using HydroTech_FC;
using HydroTech_RCS.Constants;
using UnityEngine;

namespace HydroTech_RCS.Panels
{
    public class PanelMain : Panel
    {
        protected override int PanelID
        {
            get { return CoreConsts.main; }
        }

        public override string PanelTitle
        {
            get { return PanelConsts.mainTitle; }
        }

        public PanelMain()
        {
            this.fileName = new FileName("main", "cfg", HydroJebCore.panelSaveFolder);
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

        protected override void LoadDefault()
        {
            base.LoadDefault();
            this.panelShown = true;
        }
    }
}
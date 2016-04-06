using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HydroTech_RCS.Panels
{
    using UnityEngine;
    using Constants.Core;
    using Constants.Panels;

    public class PanelMain : Panel
    {
        public PanelMain()
        {
            fileName = new HydroTech_FC.FileName("main", "cfg", HydroJebCore.PanelSaveFolder);
        }

        protected override int PanelID { get { return PanelIDs.Main; } }
        public override string PanelTitle { get { return PanelTitles.Main; } }

        protected override void SetDefaultWindowRect() { windowRect = WindowPositions.Main; }

        protected override void WindowGUI(int WindowID)
        {
            GUILayout.BeginVertical();
            foreach (Panel panel in HydroJebCore.panels.Values)
            {
                if (panel == this)
                    continue;
                panel.PanelShown = GUILayout.Toggle(panel.PanelShown, panel.PanelTitle);
            }
            GUILayout.EndVertical();
        }

        protected override void LoadDefault()
        {
            base.LoadDefault();
            _PanelShown = true;
        }
    }
}

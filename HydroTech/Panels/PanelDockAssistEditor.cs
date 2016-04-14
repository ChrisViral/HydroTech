using System.Collections.Generic;
using HydroTech.Utils;
using UnityEngine;

namespace HydroTech.Panels
{
    public class PanelDockAssistEditor : Panel
    {
        #region Fields    
        private bool showCams = true, showTargets = true;
        private Vector2 scroll;
        private readonly List<ModuleDockAssist> assists = new List<ModuleDockAssist>(); 
        #endregion

        #region Constructor
        public PanelDockAssistEditor() : base(new Rect((Screen.width * 0.95f) - 300, 360, 300, 400), GuidProvider.GetGuid<PanelDockAssistEditor>(), "Docking Assistants") { }
        #endregion

        #region Methods
        public void FixedUpdate()
        {
            if (this.assists.Count != 0) { this.assists.Clear(); }
            foreach (Part p in EditorLogic.SortedShipList)
            {
                foreach (PartModule pm in p.Modules)
                {
                    if (pm is ModuleDockAssist)
                    {
                        this.assists.Add((ModuleDockAssist)pm);
                    }
                }
            }
        }
        #endregion

        #region Static methods
        private void DrawAssistUI(ModuleDockAssist assist)
        {
            assist.InfoShown = GUILayout.Toggle(assist.InfoShown, assist.assistName, GUI.skin.button);


            if (assist.InfoShown)
            {
                string type = assist is ModuleDockAssistCam ? "Camera" : "Target";

                GUILayout.Label("Docking " + type);
                assist.AidShown = GUILayout.Toggle(assist.AidShown, "Show aid");
                
                GUILayout.BeginHorizontal();
                GUILayout.Label(string.Format("Rename {0}:", type), GUILayout.MaxWidth(70));
                assist.TempName = GUILayout.TextField(assist.TempName, 20);
                GUILayout.EndHorizontal();

                bool empty = string.IsNullOrEmpty(assist.TempName);
                if (GUILayout.Button("Apply", empty ? GUIUtils.ButtonStyle(XKCDColors.DeepRed) : GUI.skin.button, GUILayout.MaxWidth(70)) && !empty)
                {
                    assist.SetName();
                    ScreenMessages.PostScreenMessage("Rename applied", 3, ScreenMessageStyle.UPPER_LEFT);
                }
            }
        }
        #endregion

        #region Overrides
        protected override void Window(int id)
        {
            GUI.DragWindow(this.drag);

            GUILayout.BeginHorizontal();
            this.showCams = GUILayout.Toggle(this.showCams, "Cameras", GUI.skin.button);
            this.showTargets = GUILayout.Toggle(this.showTargets, "Targets", GUI.skin.button);
            GUILayout.EndHorizontal();

            this.scroll = GUILayout.BeginScrollView(this.scroll, false, true, GUI.skin.horizontalScrollbar, GUI.skin.verticalScrollbar, GUI.skin.box);
            if (this.assists.Count == 0)
            {
                string lbl = "---";
                if (this.showCams)
                {
                    lbl = this.showTargets ? "No docking assists on the vessel" : "No docking cameras on the vessel";
                }
                else if (this.showTargets) { lbl = "No docking targets on the vessel"; }

                GUILayout.Label(lbl);
            }
            else
            {
                foreach (ModuleDockAssist a in this.assists)
                {
                    if (a is ModuleDockAssistCam)
                    {
                        if (this.showCams) { DrawAssistUI(a); }
                    }
                    else if (this.showTargets) { DrawAssistUI(a); }
                }
            }
            GUILayout.EndScrollView();
        }
        #endregion
    }
}
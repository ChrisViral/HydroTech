using System.Collections.Generic;
using HydroTech.Managers;
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
        private static void DrawAssistUI(ModuleDockAssist assist)
        {
            string type = assist is ModuleDockAssistCam ? "Camera" : "Target";
            assist.InfoShown = GUILayout.Toggle(assist.InfoShown, string.Format("{0}: {1}", type, assist.assistName), GUI.skin.button);

            if (!assist.InfoShown)
            {
                if (Event.current.type == EventType.Repaint) { assist.highlight = GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition); } //Hovering button
            }
            else
            {
                assist.highlight = true;              
                assist.AidShown = GUILayout.Toggle(assist.AidShown, "Show visual aid");               
                GUILayout.BeginVertical(GUILayout.MaxHeight(40));
                GUILayout.BeginHorizontal();
                GUILayout.Label(string.Format("Rename {0}:", type), GUILayout.MaxWidth(70));
                GUILayout.BeginVertical();
                GUILayout.FlexibleSpace();
                assist.TempName = GUILayout.TextField(assist.TempName, 20);
                GUILayout.FlexibleSpace();
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();

                GUILayout.BeginHorizontal();
                bool empty = string.IsNullOrEmpty(assist.TempName);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Apply", empty ? GUIUtils.ButtonStyle(XKCDColors.DeepRed) : GUI.skin.button, GUILayout.MaxWidth(80), GUILayout.MaxHeight(25)) && !empty)
                {
                    assist.SetName();
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
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

            this.scroll = GUILayout.BeginScrollView(this.scroll, false, false, GUI.skin.horizontalScrollbar, GUI.skin.verticalScrollbar, GUI.skin.box);
            if (this.assists.Count == 0 || (!this.showCams && !this.showTargets))
            {
                string lbl;
                if (this.showCams)
                {
                    lbl = this.showTargets ? "No docking assists on the vessel" : "No docking cameras on the vessel";
                }
                else if (this.showTargets) { lbl = "No docking targets on the vessel"; }
                else { lbl = "No docking assist type selected"; }

                GUILayout.Label(lbl);
            }
            else
            {
                bool any = false;
                foreach (ModuleDockAssist a in this.assists)
                {
                    if (a is ModuleDockAssistCam)
                    {
                        if (this.showCams) { DrawAssistUI(a); any = true; }
                    }
                    else if (this.showTargets) { DrawAssistUI(a); any = true; }
                }
                if (!any)
                {
                    string lbl;
                    if (this.showCams)
                    {
                        lbl = this.showTargets ? "No docking assists on the vessel" : "No docking cameras on the vessel";
                    }
                    else { lbl = "No docking targets on the vessel"; }
                    GUILayout.Label(lbl);
                }
            }
            GUILayout.EndScrollView();
        }
        #endregion
    }
}
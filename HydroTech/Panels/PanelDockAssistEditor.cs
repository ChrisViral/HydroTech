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

                GUIUtils.CenteredButton("Apply", assist.SetName, string.IsNullOrEmpty(assist.TempName) ? GUIUtils.ButtonStyle(XKCDColors.DeepRed) : GUI.skin.button, GUILayout.MaxWidth(80), GUILayout.MaxHeight(25));
            }
        }
        #endregion

        #region Overrides
        protected override void Window(int id)
        {
            GUI.DragWindow(this.drag);

            GUILayout.BeginHorizontal();
            if (GUILayout.Toggle(this.showCams, "Cameras", GUI.skin.button) && this.showTargets) { this.showCams = !this.showCams; }
            if (GUILayout.Toggle(this.showTargets, "Targets", GUI.skin.button) && this.showCams) { this.showTargets = !this.showTargets; }
            GUILayout.EndHorizontal();

            this.scroll = GUILayout.BeginScrollView(this.scroll, false, false, GUI.skin.horizontalScrollbar, GUI.skin.verticalScrollbar, GUI.skin.box);
            bool any = false;
            foreach (ModuleDockAssist a in this.assists)
            {
                if (a is ModuleDockAssistCam)
                {
                    if (this.showCams) { DrawAssistUI(a); any = true; }
                }
                else if (this.showTargets) { DrawAssistUI(a); any = true; }
            }
            //Double ternaries ftw
            if (!any) { GUILayout.Label(string.Format("No docking {0} on the vessel", this.showCams ? this.showTargets ? "assists" : "cameras" : "targets")); }
            GUILayout.EndScrollView();
        }
        #endregion
    }
}
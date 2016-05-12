using System.Collections.Generic;
using HydroTech.Utils;
using UnityEngine;

namespace HydroTech.Panels
{
    /// <summary>
    /// Editor docking assists panel
    /// </summary>
    public class PanelDockAssistEditor : Panel
    {
        #region Fields
        //GUI stuff
        private bool showCams = true, showTargets = true;
        private Vector2 scroll;

        //Current assists on the editor ship
        private readonly List<ModuleDockAssist> assists = new List<ModuleDockAssist>();
        #endregion

        #region Properties
        /// <summary>
        /// Panel title
        /// </summary>
        public override string Title => "Docking Assists";
        #endregion

        #region Constructor
        /// <summary>
        /// Initiates window size and ID
        /// </summary>
        public PanelDockAssistEditor() : base(new Rect((Screen.width * 0.95f) - 300, 360, 300, 400), GUIUtils.GetID<PanelDockAssistEditor>()) { }
        #endregion

        #region Methods
        /// <summary>
        /// FixedUpdate cycle
        /// </summary>
        public void FixedUpdate()
        {
            if (this.assists.Count != 0) { this.assists.Clear(); }
            this.assists.AddRange(EditorLogic.SortedShipList.FindModulesImplementing<ModuleDockAssist>());
        }
        #endregion

        #region Static methods
        /// <summary>
        /// Draws the UI for the given ModuleDockAssist
        /// </summary>
        /// <param name="assist">ModuleDockAssist to draw for</param>
        private static void DrawAssistUI(ModuleDockAssist assist)
        {
            string type = assist is ModuleDockAssistCam ? "Camera" : "Target";
            assist.InfoShown = GUILayout.Toggle(assist.InfoShown, type + ": " + assist.assistName, GUI.skin.button);

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
                GUILayout.Label($"Rename {type}:", GUILayout.MaxWidth(70));
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
        /// <summary>
        /// Window function
        /// </summary>
        /// <param name="id">Window id</param>
        protected override void Window(int id)
        {
            GUI.DragWindow(this.drag);

            GUILayout.BeginHorizontal();
            //The or makes sure at least one of those is selected
            this.showCams = GUILayout.Toggle(this.showCams, "Cameras", GUI.skin.button) || !this.showTargets;
            this.showTargets = GUILayout.Toggle(this.showTargets, "Targets", GUI.skin.button) || !this.showCams;           
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
            if (!any) { GUILayout.Label($"No docking {(this.showCams ? (this.showTargets ? "assists" : "cameras") : "targets")} on the vessel"); }
            GUILayout.EndScrollView();
        }
        #endregion
    }
}
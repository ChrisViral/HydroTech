﻿using System.Collections.Generic;
using HydroTech.Managers;
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
        private Rect pos, drag;
        private bool visible;
        private int id;
        #endregion

        #region Properties
        public PanelDockAssistEditor DockAssist { get; private set; }

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

        private void Window(int id)
        {
            GUI.DragWindow(this.drag);

            GUILayout.BeginVertical(GUI.skin.box);
            this.DockAssist.Visible = GUILayout.Toggle(this.DockAssist.Visible, "Docking Cameras/Targets");
            this.RCSInfo.Visible = GUILayout.Toggle(this.RCSInfo.Visible, "RCS Info");
            GUILayout.EndVertical();

            GUIUtils.CenteredButton("Close", HydroToolbarManager.CloseEditor, GUILayout.MaxWidth(80), GUILayout.MaxHeight(25));
        }
        #endregion

        #region Functions
        private void Awake()
        {
            if (Instance != null) { Destroy(this); return; }

            Instance = this;
            this.DockAssist = new PanelDockAssistEditor();
            this.RCSInfo = new PanelRCSThrustInfo(true);
            this.Panels = new List<Panel>(2)
            {
                this.DockAssist,
                this.RCSInfo
            };
            this.pos = new Rect(Screen.width * 0.8f, Screen.height * 0.2f, 250, 50);
            this.drag = new Rect(0, 0, 200, 30);
            this.id = IDProvider.GetID<EditorMainPanel>();
        }

        private void OnDestroy()
        {
            if (Instance == this) { Instance = null; }
        }

        private void OnGUI()
        {
            if (this.visible)
            {
                GUI.skin = GUIUtils.Skin;

                this.pos = KSPUtil.ClampRectToScreen(GUILayout.Window(this.id, this.pos, Window, "HydroTech Editor Panel", GUILayout.ExpandHeight(true)));
                
                foreach (Panel p in this.Panels)
                {
                    if (p.Visible) { p.DrawGUI(); }
                }
            }
        }
        #endregion
    }
}

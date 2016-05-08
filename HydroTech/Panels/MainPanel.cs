using System.Collections.Generic;
using HydroTech.Utils;
using UnityEngine;

namespace HydroTech.Panels
{
    /// <summary>
    /// The MainPanel base class
    /// </summary>
    public abstract class MainPanel : MonoBehaviour
    {
        #region Fields
        protected int id;           //Window ID
        protected Rect pos, drag;   //Position rects
        protected Callback close;   //Closing callback
        #endregion

        #region Properties
        /// <summary>
        /// List of all the Panels connected to this MainPanel
        /// </summary>
        public List<Panel> Panels { get; } = new List<Panel>();

        private bool visible;
        /// <summary>
        /// This MainPanel's visibility
        /// </summary>
        protected virtual bool Visible => this.visible;
        #endregion

        #region Abstract properties
        /// <summary>
        /// Title of this MainPanel
        /// </summary>
        protected abstract string Title { get; }
        #endregion

        #region Methods
        /// <summary>
        /// Shows the main editor panel
        /// </summary>
        internal void ShowPanel()
        {
            if (!this.visible) { this.visible = true; }
        }

        /// <summary>
        /// Hides the main editor panel
        /// </summary>
        internal void HidePanel()
        {
            if (this.visible) { this.visible = false; }
        }

        /// <summary>
        /// GUI window function of this MainPanel
        /// </summary>
        /// <param name="id">ID of this window</param>
        private void Window(int id)
        {
            GUI.DragWindow(this.drag);

            GUILayout.BeginVertical(GUI.skin.box);
            foreach (Panel panel in this.Panels)
            {
                panel.Visible = GUILayout.Toggle(panel.Visible, panel.Title);
            }
            GUILayout.EndVertical();

            GUIUtils.CenteredButton("Close", this.close, GUILayout.MaxWidth(80), GUILayout.MaxHeight(25));
        }
        #endregion

        #region Functions
        /// <summary>
        /// Unity OnGUI function
        /// </summary>
        private void OnGUI()
        {
            if (this.Visible)
            {
                GUI.skin = GUIUtils.Skin;

                this.pos = KSPUtil.ClampRectToScreen(GUILayout.Window(this.id, this.pos, Window, this.Title, GUILayout.ExpandHeight(true)));

                foreach (Panel p in this.Panels)
                {
                    if (p.Visible) { p.DrawGUI(); }
                }
            }
        }
        #endregion
    }
}

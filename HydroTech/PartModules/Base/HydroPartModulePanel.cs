using UnityEngine;

namespace HydroTech.PartModules.Base
{
    public abstract class HydroPartModulePanel : HydroPartModule
    {
        #region Fields
        protected Rect windowRect = new Rect(Screen.width * 0.5f, Screen.height * 0.45f, 0, Screen.height * 0.1f);
        #endregion

        #region Properties
        protected bool panelShown;
        protected bool PanelShown
        {
            get { return this.panelShown; }
            set
            {
                if (this.Registered && !this.panelShown) { return; }
                //TODO: More RenderingManager
                if (value && !this.panelShown) { RenderingManager.AddToPostDrawQueue(this.QueueSpot, DrawGUI); }
                else if (!value && this.panelShown) { RenderingManager.RemoveFromPostDrawQueue(this.QueueSpot, DrawGUI); }
                this.panelShown = value;
                this.Registered = value;
            }
        }
        #endregion

        #region Abstract properties
        protected abstract int QueueSpot { get; }
        protected abstract string PanelTitle { get; }
        protected abstract bool Registered { get; set; }
        #endregion

        #region Methods
        protected void DrawGUI()
        {
            GUI.skin = HighLogic.Skin;
            this.windowRect = GUILayout.Window(this.QueueSpot, this.windowRect, WindowGUI, this.PanelTitle);
        }
        #endregion

        #region Abstract methods
        protected abstract void WindowGUI(int id);
        #endregion

        #region Overrides
        public override void OnStart(StartState state)
        {
            if (this.Registered)
            {
                this.panelShown = true;
                this.PanelShown = false;
            }
        }
        #endregion
    }
}
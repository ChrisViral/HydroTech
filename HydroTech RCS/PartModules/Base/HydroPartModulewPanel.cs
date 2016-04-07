using HydroTech_FC;
using UnityEngine;

namespace HydroTech_RCS.PartModules.Base
{
    public abstract class HydroPartModulewPanel : HydroPartModule
    {
        protected bool panelShown;

        protected Rect windowRect = new Rect(Screen.width * 0.5F, Screen.height * 0.45F, 0, Screen.height * 0.1F);

        protected bool PanelShown
        {
            get { return this.panelShown; }
            set
            {
                if (this.Registered && !this.panelShown) // Panel used by another part
                {
                    return;
                }
                if (value && !this.panelShown) { RenderingManager.AddToPostDrawQueue(this.QueueSpot, DrawGui); }
                else if (!value && this.panelShown) { RenderingManager.RemoveFromPostDrawQueue(this.QueueSpot, DrawGui); }
                this.panelShown = value;
                this.Registered = value;
            }
        }

        protected abstract int QueueSpot { get; }
        protected abstract string PanelTitle { get; }

        protected abstract bool Registered { get; set; }

        protected abstract void WindowGui(int id);

        public static GUIStyle BtnStyle()
        {
            return GameGUI.Button.Style();
        }

        public static GUIStyle BtnStyle(Color textColor)
        {
            return GameGUI.Button.Style(textColor);
        }

        public static GUIStyle LabelStyle()
        {
            return GameGUI.Label.Style();
        }

        public static GUIStyle LabelStyle(Color textColor)
        {
            return GameGUI.Label.Style(textColor);
        }

        protected void DrawGui()
        {
            GUI.skin = HighLogic.Skin;
            this.windowRect = GUILayout.Window(this.QueueSpot, this.windowRect, WindowGui, this.PanelTitle);
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            if (this.Registered)
            {
                this.panelShown = true;
                this.PanelShown = false;
            }
        }
    }
}
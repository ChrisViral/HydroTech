using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HydroTech_RCS.PartModules.Base
{
    using UnityEngine;
    using HydroTech_FC;

    abstract public class HydroPartModulewPanel : HydroPartModule
    {
        protected bool _PanelShown = false;
        protected bool PanelShown
        {
            get { return _PanelShown; }
            set
            {
                if (Registered && !_PanelShown) // Panel used by another part
                    return;
                if (value && !_PanelShown)
                    RenderingManager.AddToPostDrawQueue(QueueSpot, drawGUI);
                else if (!value && _PanelShown)
                    RenderingManager.RemoveFromPostDrawQueue(QueueSpot, drawGUI);
                _PanelShown = value;
                Registered = value;
            }
        }

        abstract protected int QueueSpot { get; }
        abstract protected string PanelTitle { get; }

        abstract protected bool Registered { get; set; }

        abstract protected void windowGUI(int ID);

        static public GUIStyle BtnStyle() { return GameGUI.Button.Style(); }
        static public GUIStyle BtnStyle(Color textColor) { return GameGUI.Button.Style(textColor); }

        static public GUIStyle LabelStyle() { return GameGUI.Label.Style(); }
        static public GUIStyle LabelStyle(Color textColor) { return GameGUI.Label.Style(textColor); }

        protected Rect windowRect = new Rect(Screen.width * 0.5F, Screen.height * 0.45F, 0, Screen.height * 0.1F);
        protected void drawGUI()
        {
            GUI.skin = HighLogic.Skin;
            windowRect = GUILayout.Window(QueueSpot, windowRect, windowGUI, PanelTitle);
        }

        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);
            if (Registered)
            {
                _PanelShown = true;
                PanelShown = false;
            }
        }
    }
}
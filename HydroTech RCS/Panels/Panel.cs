#if DEBUG
//#define SHOW_WINDOW_POS
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HydroTech_RCS.Panels
{
    using UnityEngine;
    using HydroTech_FC;
    using Constants.Core;

    abstract public class Panel : LoadSaveFileBasic
    {
        ~Panel()
        {
            PanelShown = false;
        }

        abstract protected int PanelID { get; }
        abstract public String PanelTitle { get; }

        [HydroSLNodeInfo(name = "PANEL")]
        [HydroSLField(saveName = "WindowPos", cmd = CMD.Rect_TopLeft)]
        public Rect windowRect = new Rect();

        abstract protected void SetDefaultWindowRect();

        public void ResetHeight() { windowRect.height = 0; }

        protected int QueueSpot { get { return PanelID + ManagerConsts.RenderMgr_queueSpot; } }
        protected void AddPanel() { HydroRenderingManager.AddToPostDrawQueue(QueueSpot, drawGUI); }
        protected void RemovePanel() { HydroRenderingManager.RemoveFromPostDrawQueue(QueueSpot, drawGUI); }

        protected bool _Active = false;
        virtual public bool Active
        {
            get { return _Active; }
            set
            {
                if (PanelShown && value != _Active)
                {
                    if (value)
                        AddPanel();
                    else
                        RemovePanel();
                }
                _Active = value;
            }
        }

        [HydroSLNodeInfo(name = "PANEL")]
        [HydroSLField(saveName = "PanelShown")]
        public bool _PanelShown = false;
        virtual public bool PanelShown
        {
            get { return _PanelShown; }
            set
            {
                if (!Active)
                    return;
                if (value != _PanelShown)
                {
                    if (value)
                        AddPanel();
                    else
                        RemovePanel();
                    needSave = true;
                }
                _PanelShown = value;
            }
        }

        static public GUIStyle BtnStyle() { return GameGUI.Button.Style(); }
        static public GUIStyle BtnStyle(Color textColor) { return GameGUI.Button.Style(textColor); }
        static public GUIStyle BtnStyle_Wrap() { return GameGUI.Button.Wrap(); }
        static public GUIStyle BtnStyle_Wrap(Color textColor) { return GameGUI.Button.Wrap(textColor); }

        static public GUIStyle LabelStyle() { return GameGUI.Label.Style(); }
        static public GUIStyle LabelStyle(Color textColor) { return GameGUI.Label.Style(textColor); }

        virtual protected bool LayoutEngageBtn(bool _En)
        {
            GUIStyle Engage_Btn_Style = BtnStyle(_En ? Color.red : Color.blue);
            String Engage_Btn_Text = _En ? "DISENGAGE" : "ENGAGE";
            return GUILayout.Button(Engage_Btn_Text, Engage_Btn_Style);
        }

        abstract protected void WindowGUI(int WindowID);
        virtual public void drawGUI()
        {
            GUI.skin = HighLogic.Skin;
            Rect newWindowRect = GUILayout.Window(
                QueueSpot,
                windowRect, WindowGUI, PanelTitle);
            if (newWindowRect.xMin != windowRect.xMin || newWindowRect.yMin != windowRect.yMin)
                needSave = true;
            windowRect = newWindowRect;
        }

        virtual public void onFlightStart()
        {
            Active = true;
            bool tempPanelShown = PanelShown;
            Load();
            if (PanelShown && !tempPanelShown)
                AddPanel();
        }

        virtual public void onGamePause() { Active = false; }
        virtual public void onGameResume() { Active = true; }
        virtual public void OnDeactivate() { Active = false; }
        virtual public void OnActivate() { Active = true; }
        virtual public void OnUpdate()
        {
            Active = HydroJebCore.electricity;
            if (needSave)
                Save();
        }

        protected override void LoadDefault()
        {
            base.LoadDefault();
            _PanelShown = false;
            SetDefaultWindowRect();
        }
    }
}

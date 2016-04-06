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
        public virtual bool Active
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
        public virtual bool PanelShown
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

        public static GUIStyle BtnStyle() { return GameGUI.Button.Style(); }
        public static GUIStyle BtnStyle(Color textColor) { return GameGUI.Button.Style(textColor); }
        public static GUIStyle BtnStyle_Wrap() { return GameGUI.Button.Wrap(); }
        public static GUIStyle BtnStyle_Wrap(Color textColor) { return GameGUI.Button.Wrap(textColor); }

        public static GUIStyle LabelStyle() { return GameGUI.Label.Style(); }
        public static GUIStyle LabelStyle(Color textColor) { return GameGUI.Label.Style(textColor); }

        protected virtual bool LayoutEngageBtn(bool _En)
        {
            GUIStyle Engage_Btn_Style = BtnStyle(_En ? Color.red : Color.blue);
            String Engage_Btn_Text = _En ? "DISENGAGE" : "ENGAGE";
            return GUILayout.Button(Engage_Btn_Text, Engage_Btn_Style);
        }

        abstract protected void WindowGUI(int WindowID);
        public virtual void drawGUI()
        {
            GUI.skin = HighLogic.Skin;
            Rect newWindowRect = GUILayout.Window(
                QueueSpot,
                windowRect, WindowGUI, PanelTitle);
            if (newWindowRect.xMin != windowRect.xMin || newWindowRect.yMin != windowRect.yMin)
                needSave = true;
            windowRect = newWindowRect;
        }

        public virtual void onFlightStart()
        {
            Active = true;
            bool tempPanelShown = PanelShown;
            Load();
            if (PanelShown && !tempPanelShown)
                AddPanel();
        }

        public virtual void onGamePause() { Active = false; }
        public virtual void onGameResume() { Active = true; }
        public virtual void OnDeactivate() { Active = false; }
        public virtual void OnActivate() { Active = true; }
        public virtual void OnUpdate()
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

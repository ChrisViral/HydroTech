#if DEBUG
//#define SHOW_WINDOW_POS
#endif

using System;
using HydroTech_FC;
using HydroTech_RCS.Constants.Core;
using UnityEngine;

namespace HydroTech_RCS.Panels
{
    public abstract class Panel : LoadSaveFileBasic
    {
        protected bool active;

        [HydroSLNodeInfo(name = "PANEL"), HydroSLField(saveName = "PanelShown")]
        public bool panelShown;

        [HydroSLNodeInfo(name = "PANEL"), HydroSLField(saveName = "WindowPos", cmd = CMD.Rect_TopLeft)]
        public Rect windowRect;

        protected abstract int PanelId { get; }
        public abstract string PanelTitle { get; }

        protected int QueueSpot
        {
            get { return this.PanelId + ManagerConsts.renderMgrQueueSpot; }
        }

        public virtual bool Active
        {
            get { return this.active; }
            set
            {
                if (this.PanelShown && (value != this.active))
                {
                    if (value) { AddPanel(); }
                    else
                    { RemovePanel(); }
                }
                this.active = value;
            }
        }

        public virtual bool PanelShown
        {
            get { return this.panelShown; }
            set
            {
                if (!this.Active) { return; }
                if (value != this.panelShown)
                {
                    if (value) { AddPanel(); }
                    else
                    { RemovePanel(); }
                    this.needSave = true;
                }
                this.panelShown = value;
            }
        }

        ~Panel()
        {
            this.PanelShown = false;
        }

        protected abstract void SetDefaultWindowRect();

        public void ResetHeight()
        {
            this.windowRect.height = 0;
        }

        protected void AddPanel()
        {
            HydroRenderingManager.AddToPostDrawQueue(this.QueueSpot, DrawGui);
        }

        protected void RemovePanel()
        {
            HydroRenderingManager.RemoveFromPostDrawQueue(this.QueueSpot, DrawGui);
        }

        public static GUIStyle BtnStyle()
        {
            return GameGUI.Button.Style();
        }

        public static GUIStyle BtnStyle(Color textColor)
        {
            return GameGUI.Button.Style(textColor);
        }

        public static GUIStyle BtnStyle_Wrap()
        {
            return GameGUI.Button.Wrap();
        }

        public static GUIStyle BtnStyle_Wrap(Color textColor)
        {
            return GameGUI.Button.Wrap(textColor);
        }

        public static GUIStyle LabelStyle()
        {
            return GameGUI.Label.Style();
        }

        public static GUIStyle LabelStyle(Color textColor)
        {
            return GameGUI.Label.Style(textColor);
        }

        protected virtual bool LayoutEngageBtn(bool en)
        {
            GUIStyle engageBtnStyle = BtnStyle(en ? Color.red : Color.blue);
            string engageBtnText = en ? "DISENGAGE" : "ENGAGE";
            return GUILayout.Button(engageBtnText, engageBtnStyle);
        }

        protected abstract void WindowGui(int windowId);

        public virtual void DrawGui()
        {
            GUI.skin = HighLogic.Skin;
            Rect newWindowRect = GUILayout.Window(this.QueueSpot, this.windowRect, WindowGui, this.PanelTitle);
            if ((newWindowRect.xMin != this.windowRect.xMin) || (newWindowRect.yMin != this.windowRect.yMin)) { this.needSave = true; }
            this.windowRect = newWindowRect;
        }

        public virtual void OnFlightStart()
        {
            this.Active = true;
            bool tempPanelShown = this.PanelShown;
            Load();
            if (this.PanelShown && !tempPanelShown) { AddPanel(); }
        }

        public virtual void OnGamePause()
        {
            this.Active = false;
        }

        public virtual void OnGameResume()
        {
            this.Active = true;
        }

        public virtual void OnDeactivate()
        {
            this.Active = false;
        }

        public virtual void OnActivate()
        {
            this.Active = true;
        }

        public virtual void OnUpdate()
        {
            this.Active = HydroJebCore.electricity;
            if (this.needSave) { Save(); }
        }

        protected override void LoadDefault()
        {
            base.LoadDefault();
            this.panelShown = false;
            SetDefaultWindowRect();
        }
    }
}
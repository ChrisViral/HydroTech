using HydroTech.Storage;
using HydroTech.Utils;
using UnityEngine;

namespace HydroTech.Panels
{
    public abstract class Panel : LoadSaveFileBasic
    {
        #region Fields
        [HydroSLNodeInfo(name = "PANEL"), HydroSLField(saveName = "WindowPos", cmd = CMD.RECT_TOP_LEFT)]
        public Rect windowRect;
        #endregion

        #region Properties
        [HydroSLNodeInfo(name = "PANEL"), HydroSLField(saveName = "PanelShown")]
        public bool panelShown;
        public virtual bool PanelShown
        {
            get { return this.panelShown; }
            set
            {
                if (!this.Active) { return; }
                if (value != this.panelShown) { this.needSave = true; }
                this.panelShown = value;
            }
        }

        protected bool active;
        public virtual bool Active
        {
            get { return this.active; }
            set { this.active = value; }
        }

        public abstract string PanelTitle { get; }

        protected abstract int ID { get; }
        #endregion

        #region Destructor
        ~Panel()
        {
            this.PanelShown = false;
        }
        #endregion

        #region Abstract Methods
        protected abstract void SetDefaultWindowRect();

        protected abstract void WindowGUI(int windowId);
        #endregion

        #region Methods
        public void ResetHeight()
        {
            this.windowRect.height = 0;
        }
        #endregion

        #region Virtual methods
        protected virtual bool LayoutEngageBtn(bool en)
        {
            GUIStyle engageBtnStyle = GUIUtils.ButtonStyle(en ? Color.red : Color.blue);
            string engageBtnText = en ? "DISENGAGE" : "ENGAGE";
            return GUILayout.Button(engageBtnText, engageBtnStyle);
        }

        public virtual void DrawGUI()
        {
            Rect newWindowRect = KSPUtil.ClampRectToScreen(GUILayout.Window(this.ID, this.windowRect, WindowGUI, this.PanelTitle));
            if (newWindowRect.xMin != this.windowRect.xMin || newWindowRect.yMin != this.windowRect.yMin) { this.needSave = true; }
            this.windowRect = newWindowRect;
        }

        public virtual void OnFlightStart()
        {
            this.Active = true;
            Load();
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
            if (this.needSave) { Save(); }
        }

        protected override void LoadDefault()
        {
            base.LoadDefault();
            this.panelShown = false;
            SetDefaultWindowRect();
        }
        #endregion
    }
}
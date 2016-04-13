using HydroTech.Utils;
using UnityEngine;

namespace HydroTech.Panels
{
    public abstract class Panel
    {
        #region Fields
        protected Rect windowRect;
        #endregion

        #region Properties
        protected bool panelShown;
        public virtual bool PanelShown
        {
            get { return this.panelShown; }
            set
            {
                if (this.Active) { this.panelShown = value; }
            }
        }

        public virtual bool Active { get; set; }
        #endregion

        #region Abstract properties
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
        protected virtual bool LayoutEngageBtn(bool engaged)
        {
            GUIStyle engageBtnStyle = GUIUtils.ButtonStyle(engaged ? Color.red : Color.green);
            string engageBtnText = engaged ? "DISENGAGE" : "ENGAGE";
            return GUILayout.Button(engageBtnText, engageBtnStyle);
        }

        public virtual void DrawGUI()
        {
            this.windowRect = KSPUtil.ClampRectToScreen(GUILayout.Window(this.ID, this.windowRect, WindowGUI, this.PanelTitle));
        }

        public virtual void OnFlightStart()
        {
            this.Active = false;
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

        public virtual void OnUpdate() { }
        #endregion
    }
}
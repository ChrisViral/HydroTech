using HydroTech.Utils;
using UnityEngine;

namespace HydroTech.Panels
{
    public abstract class PanelAP : Panel
    {
        #region Properties
        protected bool settings;
        protected virtual bool Settings
        {
            get { return this.settings; }
            set
            {
                if (value != this.settings) { ResetHeight(); }
                this.settings = value;
            }
        }
        #endregion

        #region Abstract properties
        protected abstract bool Engaged { get; set; }
        #endregion

        #region Constructor
        protected PanelAP(Rect window, int id, string title) : base(window, id, title) { }
        #endregion

        #region Methods
        protected bool LayoutEngageBtn(bool engaged)
        {
            GUIStyle engageBtnStyle = GUIUtils.ButtonStyle(engaged ? Color.red : Color.green);
            string engageBtnText = engaged ? "DISENGAGE" : "ENGAGE";
            return GUILayout.Button(engageBtnText, engageBtnStyle);
        }
        #endregion

        #region Virtual methods
        protected virtual void DrawSettingsUI()
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Ok")) { this.Settings = false; }
            if (this.Engaged && GUILayout.Button("Apply"))
            {
                this.Settings = false;
                this.Settings = true;
            }
            if (GUILayout.Button("Cancel"))
            {
                this.settings = false;
                ResetHeight();
            }
            GUILayout.EndHorizontal();

            if (GUILayout.Button(this.Engaged ? "DISENGAGE\nwithout applying" : "Apply and\nENGAGE", GUIUtils.ButtonStyle(this.Engaged ? Color.red : Color.green)))
            {
                if (!this.Engaged)
                {
                    this.Settings = false;
                    this.Settings = true;
                }
                this.Engaged = !this.Engaged;
            }
        }

        public override void OnFlightStart()
        {
            base.OnFlightStart();
            this.settings = false;
        }
        #endregion
    }
}
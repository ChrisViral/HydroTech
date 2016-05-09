using HydroTech.Utils;
using UnityEngine;

namespace HydroTech.Panels
{
    /// <summary>
    /// Autopilot panel base class
    /// </summary>
    public abstract class PanelAP : Panel
    {
        #region Properties
        protected bool settings;
        /// <summary>
        /// If the panel is currently in settings mode
        /// </summary>
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
        /// <summary>
        /// If this autopilot is currently engaged
        /// </summary>
        protected abstract bool Engaged { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Autopilot panel constructor, relays window information to base constructor
        /// </summary>
        /// <param name="window">Window rect</param>
        /// <param name="id">Window ID</param>
        protected PanelAP(Rect window, int id) : base(window, id) { }
        #endregion

        #region Methods
        /// <summary>
        /// A engaging/disengaging button
        /// </summary>
        /// <param name="engaged">If the autopilot is currently engaged</param>
        /// <returns>The state of the button</returns>
        protected bool EngageButton(bool engaged)
        {
            return GUILayout.Button(engaged ? "DISENGAGE" : "ENGAGE", GUIUtils.ButtonStyle(engaged ? XKCDColors.DeepRed : XKCDColors.Green));
        }
        #endregion

        #region Virtual methods
        /// <summary>
        /// Draws the settings window
        /// </summary>
        protected virtual void DrawSettings()
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
            }
            GUILayout.EndHorizontal();

            if (GUILayout.Button(this.Engaged ? "DISENGAGE\nwithout applying" : "Apply and\nENGAGE", GUIUtils.ButtonStyle(this.Engaged ? XKCDColors.DeepRed : XKCDColors.Green)))
            {
                if (!this.Engaged)
                {
                    this.Settings = false;
                    this.Settings = true;
                }
                this.Engaged = !this.Engaged;
            }
        }
        #endregion
    }
}
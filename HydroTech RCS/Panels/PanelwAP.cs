using UnityEngine;

namespace HydroTech_RCS.Panels
{
    public abstract class PanelwAP : Panel
    {
        protected bool settings;
        protected abstract bool Engaged { get; set; }

        protected virtual bool Settings
        {
            get { return this.settings; }
            set
            {
                if (value != this.settings)
                {
                    if (!value) { MakeAPSave(); }
                    ResetHeight();
                }
                this.settings = value;
            }
        }

        protected abstract void MakeAPSave();

        protected virtual void DrawSettingsUi()
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("OK")) { this.Settings = false; }
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

            if (GUILayout.Button(this.Engaged ? "DISENGAGE\nwithout applying" : "Apply and\nENGAGE", BtnStyle_Wrap(this.Engaged ? Color.red : Color.green)))
            {
                if (!this.Engaged)
                {
                    this.Settings = false;
                    this.Settings = true;
                }
                this.Engaged = !this.Engaged;
            }
        }

        public override void onFlightStart()
        {
            OnFlightStart();
            this.settings = false;
        }
    }
}
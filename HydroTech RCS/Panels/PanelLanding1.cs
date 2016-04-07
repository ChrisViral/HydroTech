using HydroTech_FC;
using HydroTech_RCS.Constants.Autopilots.Landing;
using HydroTech_RCS.Constants.Units;
using UnityEngine;

namespace HydroTech_RCS.Panels
{
    public partial class PanelLanding // User settings
    {
        protected string altKeepText;
        protected string maxThrText;
        protected string stdsText;
        protected float tempAltKeep;
        protected bool tempBurnRetro;
        protected bool tempEngines;
        protected bool tempTouchdown;
        protected bool tempUseTrueAlt;
        protected bool tempVabPod;

        protected bool TempEngines
        {
            get { return this.tempEngines; }
            set
            {
                if (value != this.tempEngines) { ResetHeight(); }
                this.tempEngines = value;
            }
        }

        protected bool TempTouchdown
        {
            get { return this.tempTouchdown; }
            set
            {
                if (value != this.tempTouchdown) { ResetHeight(); }
                this.tempTouchdown = value;
            }
        }

        protected override bool Settings
        {
            set
            {
                if (value != this.settings)
                {
                    if (value) // start settings
                    {
                        this.tempBurnRetro = BurnRetro;
                        this.TempEngines = Engines;
                        this.TempTouchdown = Touchdown;
                        this.tempVabPod = VabPod;
                        this.stdsText = SafeTouchDownSpeed.ToString("#0.0");
                        this.maxThrText = MaxThrottle.ToString("#0.0");
                        this.tempUseTrueAlt = UseTrueAlt;
                        this.tempAltKeep = AltKeep;
                        this.altKeepText = this.tempAltKeep.ToString("#0.0");
                    }
                    else // apply settings
                    {
                        BurnRetro = this.tempBurnRetro;
                        Engines = this.TempEngines;
                        Touchdown = this.TempTouchdown;
                        VabPod = this.tempVabPod;
                        UseTrueAlt = this.tempUseTrueAlt;
                        AltKeep = this.tempAltKeep;
                        float tryParse;
                        if (float.TryParse(this.stdsText, out tryParse)) { SafeTouchDownSpeed = tryParse; }
                        if (float.TryParse(this.maxThrText, out tryParse) && tryParse > 0.0F && tryParse <= 100.0F) { MaxThrottle = tryParse; }
                    }
                }
                base.Settings = value;
            }
        }

        protected override void DrawSettingsUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Pod orientation: ");
            if (GUILayout.Button(this.tempVabPod ? "Up" : "Horizon")) { this.tempVabPod = !this.tempVabPod; }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Touchdown speed");
            this.stdsText = GUILayout.TextField(this.stdsText);
            GUILayout.Label(UnitStrings.speedSimple);
            GUILayout.EndHorizontal();
            this.TempEngines = GUILayout.Toggle(this.TempEngines, "Use engines");
            if (this.TempEngines)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Max throttle");
                this.maxThrText = GUILayout.TextField(this.maxThrText);
                GUILayout.EndHorizontal();
                // tempBurnRetro = GUILayout.Toggle(tempBurnRetro, "Burn retrograde");
            }
            this.TempTouchdown = !GUILayout.Toggle(!this.TempTouchdown, "Hover at");
            if (!this.TempTouchdown)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("True Alt", this.tempUseTrueAlt ? BtnStyle(Color.green) : BtnStyle()))
                {
                    if (!this.tempUseTrueAlt)
                    {
                        this.tempAltKeep -= this.TerrainHeight;
                        if (this.tempAltKeep < Position.finalDescentHeight) { this.tempAltKeep = Position.finalDescentHeight; }
                        this.altKeepText = this.tempAltKeep.ToString("#0.0");
                    }
                    this.tempUseTrueAlt = true;
                }
                if (GUILayout.Button("ASL Alt", this.tempUseTrueAlt ? BtnStyle() : BtnStyle(Color.green)))
                {
                    if (this.tempUseTrueAlt)
                    {
                        this.tempAltKeep += this.TerrainHeight;
                        this.altKeepText = this.tempAltKeep.ToString("#0.0");
                    }
                    this.tempUseTrueAlt = false;
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                this.altKeepText = GUILayout.TextField(this.altKeepText);
                GUILayout.Label(UnitStrings.length);
                GUILayout.EndHorizontal();
                if (this.tempUseTrueAlt)
                {
                    float tryParse;
                    if (float.TryParse(this.altKeepText, out tryParse) && tryParse >= Position.finalDescentHeight)
                    {
                        this.tempAltKeep = tryParse;
                        float tempAltKeepAsl = this.tempAltKeep + this.TerrainHeight;
                        GUILayout.Label("ASL alt: " + tempAltKeepAsl.ToString("#0.0") + " " + UnitStrings.length);
                        GUILayout.Label("Max allowed horizontal speed: " + La.AllowedHoriSpeed(this.tempAltKeep).ToString("#0.0") + UnitStrings.speedSimple);
                    }
                    else
                    {
                        GUILayout.Label("Invalid altitude", LabelStyle(Color.red));
                    }
                }
                else
                {
                    float tryParse;
                    if (float.TryParse(this.altKeepText, out tryParse))
                    {
                        this.tempAltKeep = tryParse;
                        float tempAltKeepTrue = HMaths.Max(this.tempAltKeep - this.TerrainHeight, Position.finalDescentHeight);
                        GUILayout.Label("True alt: " + tempAltKeepTrue.ToString("#0.0") + " " + UnitStrings.length);
                        GUILayout.Label("Max allowed horizontal speed: " + La.AllowedHoriSpeed(tempAltKeepTrue).ToString("#0.0") + UnitStrings.speedSimple);
                    }
                    else
                    {
                        GUILayout.Label("Invalid altitude", LabelStyle(Color.red));
                    }
                }
            }

            DrawSettingsUi();
        }
    }
}
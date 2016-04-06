using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HydroTech_RCS.Panels
{
    using UnityEngine;
    using HydroTech_FC;
    using Constants.Units;
    using Constants.Autopilots.Landing;

    public partial class PanelLanding // User settings
    {
        protected String STDS_Text;
        protected bool tempVABPod;
        protected bool _tempEngines;
        protected bool tempEngines
        {
            get { return _tempEngines; }
            set
            {
                if (value != _tempEngines)
                    ResetHeight();
                _tempEngines = value;
            }
        }
        protected bool tempBurnRetro;
        protected String MaxThr_Text;
        protected bool _tempTouchdown;
        protected bool tempTouchdown
        {
            get { return _tempTouchdown; }
            set
            {
                if (value != _tempTouchdown)
                    ResetHeight();
                _tempTouchdown = value;
            }
        }
        protected bool tempUseTrueAlt;
        protected float tempAltKeep;
        protected String altKeep_Text;

        protected override bool Settings
        {
            set
            {
                if (value != _Settings)
                {
                    if (value) // start settings
                    {
                        tempBurnRetro = burnRetro;
                        tempEngines = Engines;
                        tempTouchdown = touchdown;
                        tempVABPod = VABPod;
                        STDS_Text = safeTouchDownSpeed.ToString("#0.0");
                        MaxThr_Text = MaxThrottle.ToString("#0.0");
                        tempUseTrueAlt = useTrueAlt;
                        tempAltKeep = altKeep;
                        altKeep_Text = tempAltKeep.ToString("#0.0");
                    }
                    else // apply settings
                    {
                        burnRetro = tempBurnRetro;
                        Engines = tempEngines;
                        touchdown = tempTouchdown;
                        VABPod = tempVABPod;
                        useTrueAlt = tempUseTrueAlt;
                        altKeep = tempAltKeep;
                        float tryParse;
                        if (float.TryParse(STDS_Text, out tryParse))
                            safeTouchDownSpeed = tryParse;
                        if (float.TryParse(MaxThr_Text, out tryParse) && tryParse > 0.0F && tryParse <= 100.0F)
                            MaxThrottle = tryParse;
                    }
                }
                base.Settings = value;
            }
        }

        protected override void DrawSettingsUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Pod orientation: ");
            if (GUILayout.Button(tempVABPod ? "Up" : "Horizon"))
                tempVABPod = !tempVABPod;
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Touchdown speed");
            STDS_Text = GUILayout.TextField(STDS_Text);
            GUILayout.Label(UnitStrings.Speed_Simple);
            GUILayout.EndHorizontal();
            tempEngines = GUILayout.Toggle(tempEngines, "Use engines");
            if (tempEngines)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Max throttle");
                MaxThr_Text = GUILayout.TextField(MaxThr_Text);
                GUILayout.EndHorizontal();
                // tempBurnRetro = GUILayout.Toggle(tempBurnRetro, "Burn retrograde");
            }
            tempTouchdown = !GUILayout.Toggle(!tempTouchdown, "Hover at");
            if (!tempTouchdown)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("True Alt", tempUseTrueAlt ? BtnStyle(Color.green) : BtnStyle()))
                {
                    if (!tempUseTrueAlt)
                    {
                        tempAltKeep -= TerrainHeight;
                        if (tempAltKeep < Position.FinalDescentHeight)
                            tempAltKeep = Position.FinalDescentHeight;
                        altKeep_Text = tempAltKeep.ToString("#0.0");
                    }
                    tempUseTrueAlt = true;
                }
                if (GUILayout.Button("ASL Alt", tempUseTrueAlt ? BtnStyle() : BtnStyle(Color.green)))
                {
                    if (tempUseTrueAlt)
                    {
                        tempAltKeep += TerrainHeight;
                        altKeep_Text = tempAltKeep.ToString("#0.0");
                    }
                    tempUseTrueAlt = false;
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                altKeep_Text = GUILayout.TextField(altKeep_Text);
                GUILayout.Label(UnitStrings.Length);
                GUILayout.EndHorizontal();
                if (tempUseTrueAlt)
                {
                    float tryParse;
                    if (float.TryParse(altKeep_Text, out tryParse) && tryParse >= Position.FinalDescentHeight)
                    {
                        tempAltKeep = tryParse;
                        float tempAltKeepASL = tempAltKeep + TerrainHeight;
                        GUILayout.Label("ASL alt: " + tempAltKeepASL.ToString("#0.0") + " " + UnitStrings.Length);
                        GUILayout.Label("Max allowed horizontal speed: " + LA.AllowedHoriSpeed(tempAltKeep).ToString("#0.0") + UnitStrings.Speed_Simple);
                    }
                    else
                        GUILayout.Label("Invalid altitude", LabelStyle(Color.red));
                }
                else
                {
                    float tryParse;
                    if (float.TryParse(altKeep_Text, out tryParse))
                    {
                        tempAltKeep = tryParse;
                        float tempAltKeepTrue = HMaths.Max(tempAltKeep - TerrainHeight, Position.FinalDescentHeight);
                        GUILayout.Label("True alt: " + tempAltKeepTrue.ToString("#0.0") + " " + UnitStrings.Length);
                        GUILayout.Label("Max allowed horizontal speed: " + LA.AllowedHoriSpeed(tempAltKeepTrue).ToString("#0.0") + UnitStrings.Speed_Simple);
                    }
                    else
                        GUILayout.Label("Invalid altitude", LabelStyle(Color.red));
                }
            }

            base.DrawSettingsUI();
        }
    }
}
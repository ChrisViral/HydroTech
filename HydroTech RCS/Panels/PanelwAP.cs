using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HydroTech_RCS.Panels
{
    using UnityEngine;

    abstract public class PanelwAP : Panel
    {
        abstract protected bool Engaged { get; set; }
        abstract protected void MakeAPSave();

        protected bool _Settings = false;
        virtual protected bool Settings
        {
            get { return _Settings; }
            set
            {
                if (value != _Settings)
                {
                    if (!value)
                        MakeAPSave();
                    ResetHeight();
                }
                _Settings = value;
            }
        }

        virtual protected void DrawSettingsUI()
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("OK"))
                Settings = false;
            if (Engaged && GUILayout.Button("Apply"))
            {
                Settings = false;
                Settings = true;
            }
            if (GUILayout.Button("Cancel"))
            {
                _Settings = false;
                ResetHeight();
            }
            GUILayout.EndHorizontal();

            if (GUILayout.Button(
                (Engaged ? "DISENGAGE\nwithout applying" : "Apply and\nENGAGE"),
                BtnStyle_Wrap(Engaged ? Color.red : Color.green)
                ))
            {
                if (!Engaged)
                {
                    Settings = false;
                    Settings = true;
                }
                Engaged = !Engaged;
            }
        }

        public override void onFlightStart()
        {
            base.onFlightStart();
            _Settings = false;
        }
    }
}
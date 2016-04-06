using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HydroTech_RCS.Panels
{
    using UnityEngine;
    using Constants.Units;

    public partial class PanelPreciseControl
    {
        protected String RRate_Text;
        protected String TRate_Text;
        protected String AngA_Text;
        protected String Acc_Text;
        protected bool tempByRate;

        protected override bool Settings
        {
            set
            {
                if (value != _Settings)
                {
                    if (value)
                    {
                        tempByRate = byRate;
                        RRate_Text = RRate.ToString("#0.000");
                        TRate_Text = TRate.ToString("#0.000");
                        AngA_Text = AngA.ToString("#0.000");
                        Acc_Text = Acc.ToString("#0.000");
                    }
                    else
                    {
                        byRate = tempByRate;

                        float tryParse;
                        if (float.TryParse(RRate_Text, out tryParse) && tryParse >= 0.0F && tryParse <= 1.0F)
                            RRate = tryParse;
                        if (float.TryParse(TRate_Text, out tryParse) && tryParse >= 0.0F && tryParse <= 1.0F)
                            TRate = tryParse;
                        if (float.TryParse(AngA_Text, out tryParse) && tryParse >= 0.0F)
                            AngA = tryParse;
                        if (float.TryParse(Acc_Text, out tryParse) && tryParse >= 0.0F)
                            Acc = tryParse;
                    }
                }
                base.Settings = value;
            }
        }

        protected override void DrawSettingsUI()
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("by Rate", tempByRate ? BtnStyle(Color.green) : BtnStyle()))
                tempByRate = true;
            if (GUILayout.Button("by Acceleration", tempByRate ? BtnStyle() : BtnStyle(Color.green)))
                tempByRate = false;
            GUILayout.EndHorizontal();
            if (tempByRate)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Rotation thrust rate");
                RRate_Text = GUILayout.TextField(RRate_Text);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("Translation thrust rate");
                TRate_Text = GUILayout.TextField(TRate_Text);
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Angular Acc");
                AngA_Text = GUILayout.TextField(AngA_Text);
                GUILayout.Label(UnitStrings.AngularAcc);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("Acceleration");
                Acc_Text = GUILayout.TextField(Acc_Text);
                GUILayout.Label(UnitStrings.Acceleration);
                GUILayout.EndHorizontal();
            }

            base.DrawSettingsUI();
        }
    }
}
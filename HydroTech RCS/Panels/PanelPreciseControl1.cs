using HydroTech_RCS.Constants;
using UnityEngine;

namespace HydroTech_RCS.Panels
{
    public partial class PanelPreciseControl
    {
        protected string accText;
        protected string angAText;
        protected string rateText;
        protected string rRateText;
        protected bool tempByRate;

        protected override bool Settings
        {
            set
            {
                if (value != this.settings)
                {
                    if (value)
                    {
                        this.tempByRate = ByRate;
                        this.rRateText = RRate.ToString("#0.000");
                        this.rateText = Rate.ToString("#0.000");
                        this.angAText = AngA.ToString("#0.000");
                        this.accText = Acc.ToString("#0.000");
                    }
                    else
                    {
                        ByRate = this.tempByRate;

                        float tryParse;
                        if (float.TryParse(this.rRateText, out tryParse) && tryParse >= 0.0F && tryParse <= 1.0F) { RRate = tryParse; }
                        if (float.TryParse(this.rateText, out tryParse) && tryParse >= 0.0F && tryParse <= 1.0F) { Rate = tryParse; }
                        if (float.TryParse(this.angAText, out tryParse) && tryParse >= 0.0F) { AngA = tryParse; }
                        if (float.TryParse(this.accText, out tryParse) && tryParse >= 0.0F) { Acc = tryParse; }
                    }
                }
                base.Settings = value;
            }
        }

        protected override void DrawSettingsUI()
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("by Rate", this.tempByRate ? BtnStyle(Color.green) : BtnStyle())) { this.tempByRate = true; }
            if (GUILayout.Button("by Acceleration", this.tempByRate ? BtnStyle() : BtnStyle(Color.green))) { this.tempByRate = false; }
            GUILayout.EndHorizontal();
            if (this.tempByRate)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Rotation thrust rate");
                this.rRateText = GUILayout.TextField(this.rRateText);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("Translation thrust rate");
                this.rateText = GUILayout.TextField(this.rateText);
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Angular Acc");
                this.angAText = GUILayout.TextField(this.angAText);
                GUILayout.Label(GeneralConsts.angularAcc);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("Acceleration");
                this.accText = GUILayout.TextField(this.accText);
                GUILayout.Label(GeneralConsts.acceleration);
                GUILayout.EndHorizontal();
            }

            DrawSettingsUI();
        }
    }
}
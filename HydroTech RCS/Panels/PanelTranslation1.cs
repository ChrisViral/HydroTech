using HydroTech_RCS.Autopilots;
using UnityEngine;

namespace HydroTech_RCS.Panels
{
    public partial class PanelTranslation
    {
        protected string rateText;
        protected bool tempHoldOrient;
        protected bool tempRespond;
        protected APTranslation.TransDir tempTransMode;
        protected string xText;
        protected string yText;
        protected string zText;

        protected override bool Settings
        {
            set
            {
                if (value != this.settings)
                {
                    if (value)
                    {
                        this.tempTransMode = TransMode;
                        this.tempRespond = Respond;
                        this.tempHoldOrient = HoldOrient;

                        this.xText = ThrustVector.x.ToString("#0.00");
                        this.yText = ThrustVector.y.ToString("#0.00");
                        this.zText = ThrustVector.z.ToString("#0.00");
                        this.rateText = ThrustRate.ToString("#0.0");
                    }
                    else
                    {
                        TransMode = this.tempTransMode;
                        Respond = this.tempRespond;
                        HoldOrient = this.tempHoldOrient;

                        if (this.tempTransMode == APTranslation.TransDir.ADVANCED)
                        {
                            float x, y, z;
                            if (float.TryParse(this.xText, out x) && float.TryParse(this.yText, out y) && float.TryParse(this.zText, out z) && (x != 0.0F || y != 0.0F || z != 0.0F)) { ThrustVector = new Vector3(x, y, z).normalized; }
                        }

                        float tryParse;
                        if (float.TryParse(this.rateText, out tryParse) && tryParse >= 0.0F && tryParse <= 1.0F) { ThrustRate = tryParse; }
                    }
                }
                base.Settings = value;
            }
        }

        protected override void DrawSettingsUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            for (int i = 0; i <= 6; i++)
            {
                APTranslation.TransDir x = (APTranslation.TransDir)i;
                if (GUILayout.Button(x.ToString(), this.tempTransMode == x ? BtnStyle(Color.green) : BtnStyle()))
                {
                    this.tempTransMode = x;
                    if (i != 6) { ResetHeight(); }
                }
                if (i % 2 == 1)
                {
                    GUILayout.EndVertical();
                    if (i != 5) { GUILayout.BeginVertical(); }
                    else
                    {
                        GUILayout.EndHorizontal();
                    }
                }
            }
            if (this.tempTransMode == APTranslation.TransDir.ADVANCED)
            {
                GUILayout.BeginVertical();
                GUILayout.Label("X(+RIGHT)=");
                this.xText = GUILayout.TextField(this.xText);
                GUILayout.EndHorizontal();

                GUILayout.BeginVertical();
                GUILayout.Label("Y(+DOWN)=");
                this.yText = GUILayout.TextField(this.yText);
                GUILayout.EndHorizontal();

                GUILayout.BeginVertical();
                GUILayout.Label("Z(+FORWARD)=");
                this.zText = GUILayout.TextField(this.zText);
                GUILayout.EndHorizontal();

                float x, y, z;
                if (float.TryParse(this.xText, out x) && float.TryParse(this.yText, out y) && float.TryParse(this.zText, out z) && (x != 0.0F || y != 0.0F || z != 0.0F))
                {
                    Vector3 tempThrustVector = new Vector3(x, y, z).normalized;
                    GUILayout.Label("Normalized vector:");
                    GUILayout.Label(tempThrustVector.ToString("#0.00"));
                }
                else
                {
                    GUILayout.Label("Invalid input", LabelStyle(Color.red));
                }
            }
            GUILayout.BeginHorizontal();
            GUILayout.Label("Thrust rate (0~1)");
            this.rateText = GUILayout.TextField(this.rateText);
            GUILayout.EndHorizontal();
            this.tempRespond = GUILayout.Toggle(this.tempRespond, "Respond to main throttle");
            this.tempHoldOrient = GUILayout.Toggle(this.tempHoldOrient, "Hold current orientation");

            base.DrawSettingsUI();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HydroTech_RCS.Panels
{
    using UnityEngine;
    using Autopilots;

    public partial class PanelTranslation
    {
        protected APTranslation.TransDir tempTransMode;
        protected String X_Text;
        protected String Y_Text;
        protected String Z_Text;
        protected String Rate_Text;
        protected bool tempRespond;
        protected bool tempHoldOrient;

        protected override bool Settings
        {
            set
            {
                if (value != _Settings)
                {
                    if (value)
                    {
                        tempTransMode = Trans_Mode;
                        tempRespond = respond;
                        tempHoldOrient = HoldOrient;

                        X_Text = thrustVector.x.ToString("#0.00");
                        Y_Text = thrustVector.y.ToString("#0.00");
                        Z_Text = thrustVector.z.ToString("#0.00");
                        Rate_Text = thrustRate.ToString("#0.0");
                    }
                    else
                    {
                        Trans_Mode = tempTransMode;
                        respond = tempRespond;
                        HoldOrient = tempHoldOrient;

                        if (tempTransMode == APTranslation.TransDir.ADVANCED)
                        {
                            float X, Y, Z;
                            if (float.TryParse(X_Text, out X)
                                && float.TryParse(Y_Text, out Y)
                                && float.TryParse(Z_Text, out Z)
                                && (X != 0.0F || Y != 0.0F || Z != 0.0F))
                                thrustVector = new Vector3(X, Y, Z).normalized;
                        }

                        float tryParse;
                        if (float.TryParse(Rate_Text, out tryParse) && tryParse >= 0.0F && tryParse <= 1.0F)
                            thrustRate = tryParse;
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
                if (GUILayout.Button(x.ToString(), tempTransMode == x ? BtnStyle(Color.green) : BtnStyle()))
                {
                    tempTransMode = x;
                    if (i != 6)
                        ResetHeight();
                }
                if (i % 2 == 1)
                {
                    GUILayout.EndVertical();
                    if (i != 5)
                        GUILayout.BeginVertical();
                    else
                        GUILayout.EndHorizontal();
                }
            }
            if (tempTransMode == APTranslation.TransDir.ADVANCED)
            {
                GUILayout.BeginVertical();
                GUILayout.Label("X(+RIGHT)=");
                X_Text = GUILayout.TextField(X_Text);
                GUILayout.EndHorizontal();

                GUILayout.BeginVertical();
                GUILayout.Label("Y(+DOWN)=");
                Y_Text = GUILayout.TextField(Y_Text);
                GUILayout.EndHorizontal();

                GUILayout.BeginVertical();
                GUILayout.Label("Z(+FORWARD)=");
                Z_Text = GUILayout.TextField(Z_Text);
                GUILayout.EndHorizontal();

                float X, Y, Z;
                if (float.TryParse(X_Text, out X)
                    && float.TryParse(Y_Text, out Y)
                    && float.TryParse(Z_Text, out Z)
                    && (X != 0.0F || Y != 0.0F || Z != 0.0F))
                {
                    Vector3 tempThrustVector = new Vector3(X, Y, Z).normalized;
                    GUILayout.Label("Normalized vector:");
                    GUILayout.Label(tempThrustVector.ToString("#0.00"));
                }
                else
                    GUILayout.Label("Invalid input", LabelStyle(Color.red));
            }
            GUILayout.BeginHorizontal();
            GUILayout.Label("Thrust rate (0~1)");
            Rate_Text = GUILayout.TextField(Rate_Text);
            GUILayout.EndHorizontal();
            tempRespond = GUILayout.Toggle(tempRespond, "Respond to main throttle");
            tempHoldOrient = GUILayout.Toggle(tempHoldOrient, "Hold current orientation");

            base.DrawSettingsUI();
        }
    }
}
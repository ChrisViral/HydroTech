using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HydroTech_RCS.Panels
{
    using UnityEngine;
    using HydroTech_FC;
    using Constants.Core;
    using Constants.Panels;

    public class PanelMainThrottle : Panel
    {
        public PanelMainThrottle()
        {
            fileName = new FileName("throttle", "cfg", HydroJebCore.PanelSaveFolder);
        }

        protected override int PanelID { get { return PanelIDs.MainThrottle; } }
        public override string PanelTitle { get { return PanelTitles.MainThrottle; } }

        protected override void SetDefaultWindowRect() { windowRect = WindowPositions.MainThrottle; }

        protected float Throttle
        {
            get { return FlightInputHandler.state.mainThrottle * 100.0F; }
            set { FlightInputHandler.state.mainThrottle = value / 100.0F; }
        }

        protected bool Editing = false;
        protected String EditText;

        protected override void WindowGUI(int WindowID)
        {
            //GUIStyle CurThrStyle = BtnStyle(Color.green);
            GUIStyle CurThrStyle = BtnStyle();
            GUIStyle EditBtnStyle = BtnStyle(Color.yellow);
            GUILayout.BeginVertical();
            for (int i = 7; i > 0; i -= 3)
            {
                GUILayout.BeginHorizontal();
                for (int j = 0; j < 3; j++)
                {
                    float Thr = (i + j) * 10;
                    if (GUILayout.Button(
                        Thr.ToString("#0"),
                        Throttle == Thr ? CurThrStyle : BtnStyle()
                        ))
                        Throttle = Thr;
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("OFF", Throttle == 0 ? CurThrStyle : BtnStyle()))
                Throttle = 0;
            if (GUILayout.Button("100", Throttle == 100 ? CurThrStyle : BtnStyle()))
                Throttle = 100;
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("-1"))
                Throttle -= 1;
            if (GUILayout.Button("+1"))
                Throttle += 1;
            if (GUILayout.Button("-5"))
                Throttle -= 5;
            if (GUILayout.Button("+5"))
                Throttle += 5;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Current throttle:");
            if (Editing)
            {
                EditText = GUILayout.TextField(EditText);
                if (GUILayout.Button("OK", EditBtnStyle))
                {
                    float Thr;
                    if (float.TryParse(EditText, out Thr) && Thr >= 0 && Thr <= 100)
                    {
                        Throttle = Thr;
                        Editing = false;
                    }
                }
            }
            else
            {
                GUILayout.Label(Throttle.ToString("#0.0"));
                if (GUILayout.Button("EDIT", EditBtnStyle))
                {
                    EditText = Throttle.ToString("#0.0");
                    Editing = true;
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUI.DragWindow();
        }

        public override void onFlightStart()
        {
            base.onFlightStart();
            Editing = false;
        }
    }
}

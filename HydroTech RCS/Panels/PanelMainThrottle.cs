using HydroTech_FC;
using HydroTech_RCS.Constants.Core;
using HydroTech_RCS.Constants.Panels;
using UnityEngine;

namespace HydroTech_RCS.Panels
{
    public class PanelMainThrottle : Panel
    {
        protected bool editing;
        protected string editText;

        protected override int PanelID
        {
            get { return PanelIDs.mainThrottle; }
        }

        public override string PanelTitle
        {
            get { return PanelTitles.mainThrottle; }
        }

        protected float Throttle
        {
            get { return FlightInputHandler.state.mainThrottle * 100.0F; }
            set { FlightInputHandler.state.mainThrottle = value / 100.0F; }
        }

        public PanelMainThrottle()
        {
            this.fileName = new FileName("throttle", "cfg", HydroJebCore.panelSaveFolder);
        }

        protected override void SetDefaultWindowRect()
        {
            this.windowRect = WindowPositions.mainThrottle;
        }

        protected override void WindowGUI(int windowId)
        {
            //GUIStyle CurThrStyle = BtnStyle(Color.green);
            GUIStyle curThrStyle = BtnStyle();
            GUIStyle editBtnStyle = BtnStyle(Color.yellow);
            GUILayout.BeginVertical();
            for (int i = 7; i > 0; i -= 3)
            {
                GUILayout.BeginHorizontal();
                for (int j = 0; j < 3; j++)
                {
                    float thr = (i + j) * 10;
                    if (GUILayout.Button(thr.ToString("#0"), this.Throttle == thr ? curThrStyle : BtnStyle())) { this.Throttle = thr; }
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("OFF", this.Throttle == 0 ? curThrStyle : BtnStyle())) { this.Throttle = 0; }
            if (GUILayout.Button("100", this.Throttle == 100 ? curThrStyle : BtnStyle())) { this.Throttle = 100; }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("-1")) { this.Throttle -= 1; }
            if (GUILayout.Button("+1")) { this.Throttle += 1; }
            if (GUILayout.Button("-5")) { this.Throttle -= 5; }
            if (GUILayout.Button("+5")) { this.Throttle += 5; }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Current throttle:");
            if (this.editing)
            {
                this.editText = GUILayout.TextField(this.editText);
                if (GUILayout.Button("OK", editBtnStyle))
                {
                    float thr;
                    if (float.TryParse(this.editText, out thr) && thr >= 0 && thr <= 100)
                    {
                        this.Throttle = thr;
                        this.editing = false;
                    }
                }
            }
            else
            {
                GUILayout.Label(this.Throttle.ToString("#0.0"));
                if (GUILayout.Button("EDIT", editBtnStyle))
                {
                    this.editText = this.Throttle.ToString("#0.0");
                    this.editing = true;
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUI.DragWindow();
        }

        public override void onFlightStart()
        {
            OnFlightStart();
            this.editing = false;
        }
    }
}
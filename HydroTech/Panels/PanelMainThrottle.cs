using HydroTech.Storage;
using HydroTech.Utils;
using UnityEngine;

namespace HydroTech.Panels
{
    public class PanelMainThrottle : Panel
    {
        #region Fields
        protected bool editing;
        protected string editText;
        #endregion

        #region Properties
        protected float Throttle
        {
            get { return FlightInputHandler.state.mainThrottle * 100; }
            set { FlightInputHandler.state.mainThrottle = value / 100f; }
        }

        public override string PanelTitle
        {
            get { return "Main Throttle Control"; }
        }

        private readonly int id;
        protected override int ID
        {
            get { return this.id; }
        }
        #endregion

        #region Constructor
        public PanelMainThrottle()
        {
            this.fileName = new FileName("throttle", "cfg", FileName.panelSaveFolder);
            this.id = GuidProvider.GetGuid<PanelMainThrottle>();
        }
        #endregion

        #region Static methods
        private static float Clamp0(float f)
        {
            return f > 0 ? f : 0;
        }

        private static float Clamp100(float f)
        {
            return f < 100 ? f : 100;
        }
        #endregion

        #region Overrides
        public override void OnFlightStart()
        {
            base.OnFlightStart();
            this.editing = false;
        }

        protected override void SetDefaultWindowRect()
        {
            this.windowRect = new Rect(100, 240, 250, 236);
        }

        protected override void WindowGUI(int windowId)
        {
            GUIStyle curThrStyle = GUIUtils.Skin.button;
            GUIStyle editBtnStyle = GUIUtils.ButtonStyle(Color.yellow);
            GUILayout.BeginVertical();
            for (int i = 7; i > 0; i -= 3)
            {
                GUILayout.BeginHorizontal();
                for (int j = 0; j < 3; j++)
                {
                    float thr = (i + j) * 10;
                    if (GUILayout.Button(thr.ToString("#0"), this.Throttle == thr ? curThrStyle : GUIUtils.Skin.button))
                    {
                        this.Throttle = thr;
                    }
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.BeginHorizontal();
            //The conditionals only change the label colour... not obvious <.<
            if (GUILayout.Button("OFF", this.Throttle == 0 ? curThrStyle : GUIUtils.Skin.button))
            {
                this.Throttle = 0;
            }
            if (GUILayout.Button("100", this.Throttle == 100 ? curThrStyle : GUIUtils.Skin.button))
            {
                this.Throttle = 100;
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("-1")) { this.Throttle = Clamp0(this.Throttle - 1); }
            if (GUILayout.Button("+1")) { this.Throttle = Clamp100(this.Throttle + 1); }
            if (GUILayout.Button("-5")) { this.Throttle = Clamp0(this.Throttle - 5); }
            if (GUILayout.Button("+5")) { this.Throttle = Clamp100(this.Throttle +5); }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Current throttle:");
            if (this.editing)
            {
                this.editText = GUILayout.TextField(this.editText);
                if (GUILayout.Button("OK", editBtnStyle))
                {
                    float temp;
                    if (float.TryParse(this.editText, out temp) && temp >= 0 && temp <= 100)
                    {
                        this.Throttle = temp;
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
        #endregion
    }
}
using HydroTech.Utils;
using UnityEngine;

namespace HydroTech.Panels
{
    public class PanelMainThrottle : Panel
    {
        #region Static properties
        private static float Throttle
        {
            get { return FlightInputHandler.state.mainThrottle * 100; }
            set { FlightInputHandler.state.mainThrottle = value / 100f; }
        }
        #endregion

        #region Fields
        private bool editing;
        private string editText;
        #endregion

        #region Properties
        public override string Title => "Main Throttle Control";
        #endregion

        #region Constructor
        public PanelMainThrottle() : base(new Rect(100, 240, 250, 236), IDProvider.GetID<PanelMainThrottle>(), "Main Throttle Control") { }
        #endregion

        #region Overrides
        public override void OnFlightStart()
        {
            base.OnFlightStart();
            this.editing = false;
        }

        protected override void Window(int id)
        {
            GUI.DragWindow(this.drag);

            GUIStyle curThrStyle = GUIUtils.Skin.button;
            GUIStyle editBtnStyle = GUIUtils.ButtonStyle(Color.yellow);
            GUILayout.BeginVertical();
            for (int i = 7; i > 0; i -= 3)
            {
                GUILayout.BeginHorizontal();
                for (int j = 0; j < 3; j++)
                {
                    float thr = (i + j) * 10;
                    if (GUILayout.Button(thr.ToString("#0"), Throttle == thr ? curThrStyle : GUIUtils.Skin.button))
                    {
                        Throttle = thr;
                    }
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.BeginHorizontal();
            //The conditionals only change the label colour... not obvious <.<
            if (GUILayout.Button("OFF", Throttle == 0 ? curThrStyle : GUIUtils.Skin.button))
            {
                Throttle = 0;
            }
            if (GUILayout.Button("100", Throttle == 100 ? curThrStyle : GUIUtils.Skin.button))
            {
                Throttle = 100;
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("-1")) { Throttle = HTUtils.Clamp0(Throttle - 1); }
            if (GUILayout.Button("+1")) { Throttle = HTUtils.Clamp100(Throttle + 1); }
            if (GUILayout.Button("-5")) { Throttle = HTUtils.Clamp0(Throttle - 5); }
            if (GUILayout.Button("+5")) { Throttle = HTUtils.Clamp100(Throttle +5); }
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
                        Throttle = temp;
                        this.editing = false;
                    }
                }
            }
            else
            {
                GUILayout.Label(Throttle.ToString("#0.0"));
                if (GUILayout.Button("EDIT", editBtnStyle))
                {
                    this.editText = Throttle.ToString("#0.0");
                    this.editing = true;
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }
        #endregion
    }
}
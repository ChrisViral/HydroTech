using HydroTech.Utils;
using UnityEngine;

namespace HydroTech.Panels
{
    public class FlightMainPanel : MonoBehaviour
    {
        private Rect pos = new Rect(Screen.width / 2, Screen.height / 2, 20, 20);
        private HydroJebModule module;

        public void SetModule(HydroJebModule module)
        {
            this.module = module;
        }

        private void OnGUI()
        {
            GUI.Label(this.pos, ":D", GUIUtils.Skin.label);
        }
    }
}

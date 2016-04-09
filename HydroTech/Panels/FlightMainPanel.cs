using HydroTech.Utils;
using UnityEngine;

namespace HydroTech.Panels
{
    public class FlightMainPanel : MonoBehaviour
    {
        private Rect pos = new Rect(Screen.width / 2, Screen.height / 2, 20, 20);

        private void OnGUI()
        {
            GUI.Label(this.pos, ":D", GUIUtils.Skin.label);
        }
    }
}

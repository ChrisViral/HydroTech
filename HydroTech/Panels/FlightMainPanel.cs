using UnityEngine;

namespace HydroTech.Panels
{
    public class FlightMainPanel : MonoBehaviour
    {
        private Rect pos = new Rect(Screen.height / 2, Screen.width / 2, 20, 20);

        private void OnGUI()
        {
            GUI.Label(this.pos, ":D");
        }
    }
}

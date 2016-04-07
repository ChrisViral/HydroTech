using UnityEngine;

namespace HydroTech_RCS.Utils
{
    //TODO: add some caching on coloured GUI styles to prevent creating the same styles over again
    public static class GUIUtils
    {
        #region Buttons
        public static GUIStyle ButtonStyle(bool wrap = false)
        {
            return new GUIStyle(GUI.skin.button) { wordWrap = wrap }; ;
        }

        public static GUIStyle ButtonStyle(Color textColor, bool wrap = false)
        {
            GUIStyle style = ButtonStyle(wrap);
            style.normal.textColor = style.focused.textColor = textColor;
            return style;
        }
        #endregion

        #region Labels
        public static GUIStyle LabelStyle(TextAnchor anchor = TextAnchor.UpperLeft)
        {
            return new GUIStyle(GUI.skin.label) { alignment = anchor };
        }

        public static GUIStyle LabelStyle(Color textColour, TextAnchor anchor = TextAnchor.UpperLeft)
        {
            GUIStyle style = LabelStyle(anchor);
            style.normal.textColor = textColour;
            return style;
        }
        #endregion
    }
}
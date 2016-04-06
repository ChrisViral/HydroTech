using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HydroTech_FC
{
    using UnityEngine;

    public static class GameGUI
    {
        public static class Button
        {
            public enum Options
            {
                NONE = 0,
                Wrap = 1
            }
            public static GUIStyle Style(Options options = Options.NONE)
            {
                GUIStyle style = new GUIStyle(GUI.skin.button);
                if ((options & Options.Wrap) != 0)
                    style.wordWrap = true;
                return style;
            }
            public static GUIStyle Style(Color textColor, Options options = Options.NONE)
            {
                GUIStyle style = Style(options);
                style.normal.textColor = style.focused.textColor = textColor;
                return style;
            }
            public static GUIStyle Style(int options) { return Style((Options)options); }
            public static GUIStyle Style(Color textColor, int options) { return Style(textColor, (Options)options); }

            public static GUIStyle Wrap() { return Style(1); }
            public static GUIStyle Wrap(Color textColor) { return Style(textColor, 1); }
        }

        public static class Label
        {
            public enum Options
            {
                NONE = 0,
                Left = 1, Right = 2, Center = 3,
                Upper = 4, Lower = 8, Middle = 12
            }
            public static GUIStyle Style(Options options = Options.NONE)
            {
                GUIStyle style = new GUIStyle(GUI.skin.label);
                if (((int)options & 15) != 0)
                {
                    int alignment = (int)options & 15;
                    if ((alignment & 3) == 0)
                        alignment |= 1;
                    if ((alignment & 12) == 0)
                        alignment |= 4;
                    switch (alignment)
                    {
                        case 5: style.alignment = TextAnchor.UpperLeft; break;
                        case 6: style.alignment = TextAnchor.UpperRight; break;
                        case 7: style.alignment = TextAnchor.UpperCenter; break;
                        case 9: style.alignment = TextAnchor.LowerLeft; break;
                        case 10: style.alignment = TextAnchor.LowerRight; break;
                        case 11: style.alignment = TextAnchor.LowerCenter; break;
                        case 13: style.alignment = TextAnchor.MiddleLeft; break;
                        case 14: style.alignment = TextAnchor.MiddleRight; break;
                        case 15: style.alignment = TextAnchor.MiddleCenter; break;
                    }
                }
                return style;
            }
            public static GUIStyle Style(Color textColor, Options options = Options.NONE)
            {
                GUIStyle style = Style(options);
                style.normal.textColor = textColor;
                return style;
            }
            public static GUIStyle Style(int options) { return Style((Options)options); }
            public static GUIStyle Style(Color textColor, int options) { return Style(textColor, (Options)options); }

            public static GUIStyle Centered() { return Style(15); }
            public static GUIStyle Centered(Color textColor) { return Style(textColor, 15); }
        }
    }
}
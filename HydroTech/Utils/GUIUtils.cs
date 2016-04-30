using System.Collections.Generic;
using UnityEngine;

namespace HydroTech.Utils
{
    public static class GUIUtils
    {
        #region Fields
        private static readonly Dictionary<Color, GUIStyle> buttonStyles = new Dictionary<Color, GUIStyle>();
        private static readonly Dictionary<Color, GUIStyle> wrapButtonStyles = new Dictionary<Color, GUIStyle>();
        private static readonly Dictionary<Color, GUIStyle> labelStyles = new Dictionary<Color, GUIStyle>();
        #endregion

        #region Properties
        private static bool usingDefaultSkin;
        public static bool UsingDefaultSkin
        {
            get { return usingDefaultSkin; }
            set
            {
                if (value != usingDefaultSkin)
                {
                    Skin = value ? unitySkin : kspSkin;
                    GUI.skin = Skin;

                    WrapButton = new GUIStyle(Skin.button) { wordWrap = true };
                    buttonStyles.Clear();
                    wrapButtonStyles.Clear();
                    labelStyles.Clear();

                    usingDefaultSkin = value;
                }
            }
        }

        private static readonly GUISkin kspSkin;
        private static readonly GUISkin unitySkin;
        public static GUISkin Skin { get; private set; }

        public static GUIStyle WrapButton { get; private set; }
        #endregion

        #region Constructor
        static GUIUtils()
        {
            kspSkin = HighLogic.Skin;
            unitySkin = GUI.skin;

            //TODO: when custom settings are implemented, set the right one to start with
            Skin = kspSkin;

            WrapButton = new GUIStyle(Skin.button) { wordWrap = true };
        }
        #endregion

        #region Methods
        public static GUIStyle ButtonStyle(Color colour, bool wrap = false)
        {
            Dictionary<Color, GUIStyle> styles = wrap ? wrapButtonStyles : buttonStyles;
            GUIStyle style;
            if (!styles.TryGetValue(colour, out style))
            {
                style = new GUIStyle(Skin.button)
                {
                    wordWrap = wrap,
                    normal   = { textColor = colour },
                    focused  = { textColor = colour }
                };
                styles.Add(colour, style);
            }
            return style;
        }
        
        public static GUIStyle ColouredLabel(Color colour)
        {
            GUIStyle style;
            if (!labelStyles.TryGetValue(colour, out style))
            {
                style = new GUIStyle(Skin.label)
                {
                    normal = { textColor = colour }
                };
                labelStyles.Add(colour, style);
            }
            return style;
        }

        public static void CenteredButton(string text, Callback callback, params GUILayoutOption[] options)
        {
            CenteredButton(text, callback, GUI.skin.button, options);
        }

        public static void CenteredButton(string text, Callback callback, GUIStyle style, params GUILayoutOption[] options)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(text, style, options))
            {
                callback();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        public static bool TwinToggle(bool state, string labelA, string labelB, GUIStyle style)
        {
            if (GUILayout.Toggle(state, labelA, style)) { state = true; }
            if (GUILayout.Toggle(!state, labelB, style)) { state = false; }
            return state;
        }

        public static bool CheckRange(float f, float min, float max)
        {
            return f >= min && f <= max;
        }

        public static void CreateEntryArea(string label, ref string value, float min, float max)
        {
            float f;
            GUILayout.Label(label, float.TryParse(value, out f) && CheckRange(f, min, max) ? GUI.skin.label : ColouredLabel(XKCDColors.Red));
            value = GUILayout.TextField(value, 10);
        }
        #endregion
    }
}
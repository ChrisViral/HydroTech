using System.Collections.Generic;
using UnityEngine;

namespace HydroTech.Utils
{
    /// <summary>
    /// Generic GUI utilities properties and methods
    /// </summary>
    public static class GUIUtils
    {
        #region Fields
        private static float f;     //Temporary float parsing field
        private static readonly Dictionary<Color, GUIStyle> buttonStyles = new Dictionary<Color, GUIStyle>();       //Buton storage
        private static readonly Dictionary<Color, GUIStyle> wrapButtonStyles = new Dictionary<Color, GUIStyle>();   //Wrapped button storage
        private static readonly Dictionary<Color, GUIStyle> labelStyles = new Dictionary<Color, GUIStyle>();        //Label storage
        #endregion

        #region Properties
        private static bool usingDefaultSkin;
        /// <summary>
        /// If the UI is using the default Unity skin
        /// </summary>
        public static bool UsingDefaultSkin
        {
            get { return usingDefaultSkin; }
            set
            {
                if (value != usingDefaultSkin)
                {
                    Skin = value ? unitySkin : kspSkin;
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
        /// <summary>
        /// The currently used GUISkin
        /// </summary>
        public static GUISkin Skin { get; private set; }

        /// <summary>
        /// The current wrapped button style
        /// </summary>
        public static GUIStyle WrapButton { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Initiates the current GUI settings
        /// </summary>
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
        /// <summary>
        /// Obtains the button style of the given text colour and wrap style
        /// </summary>
        /// <param name="colour">Text colour of the button</param>
        /// <param name="wrap">Wrapping of the button text, defaults to false</param>
        /// <returns>The specified button GUIStyle</returns>
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
        
        /// <summary>
        /// Obtains a label style of the given text colour
        /// </summary>
        /// <param name="colour">Colour of the label to get</param>
        /// <returns>The specified coloured label GUIStyle</returns>
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

        /// <summary>
        /// Creates a horizontally centered GUILayout button
        /// </summary>
        /// <param name="text">Button text</param>
        /// <param name="callback">Button press callback</param>
        /// <param name="options">GUILayout option parameters</param>
        public static void CenteredButton(string text, Callback callback, params GUILayoutOption[] options) => CenteredButton(text, callback, Skin.button, options);

        /// <summary>
        /// Creates a horizontally cenetered GUILayout button
        /// </summary>
        /// <param name="text">Button text</param>
        /// <param name="callback">Button press callback</param>
        /// <param name="style">Button GUIStyle</param>
        /// <param name="options">GUILayout option parameters</param>
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

        /// <summary>
        /// Creates a pair of linked GUILayout toggles
        /// </summary>
        /// <param name="state">Currently selected toggle (true => first, false => second)</param>
        /// <param name="labelA">Label of the first toggle</param>
        /// <param name="labelB">Label of the second toggle</param>
        /// <param name="style">GUIStyle of the toggles</param>
        /// <returns>The modified state of the toggles</returns>
        public static bool TwinToggle(bool state, string labelA, string labelB, GUIStyle style)
        {
            if (GUILayout.Toggle(state, labelA, style)) { state = true; }
            if (GUILayout.Toggle(!state, labelB, style)) { state = false; }
            return state;
        }

        /// <summary>
        /// Checks if the passed float is within the specified range, inclusively
        /// </summary>
        /// <param name="f">Float to check</param>
        /// <param name="min">Minimum value</param>
        /// <param name="max">Maximum value</param>
        /// <returns>If the float is within the range</returns>
        public static bool CheckRange(float f, float min, float max) => f >= min && f <= max;

        /// <summary>
        /// Creates a numerical text entry field paired with an indicator label that turns red when the entered number is invalid, with a default minimal value of 0
        /// </summary>
        /// <param name="value">Current string value of the entry field</param>
        /// <param name="label">Indicator label of the entry field</param>
        /// <param name="max">Maximum valid numerical value</param>
        /// <returns>The modified string value of the entry field</returns>
        public static string NumericalEntryArea(string value, string label, float max) => NumericalEntryArea(value, label, 0, max);

        /// <summary>
        /// Creates a numerical text entry field paired with an indicator label that turns red when the entered number is invalid
        /// </summary>
        /// <param name="value">Current string value of the entry field</param>
        /// <param name="label">Indicator label of the entry field</param>
        /// <param name="min">Minimum valid numerical value</param>
        /// <param name="max">Maximum valid numerical value</param>
        /// <returns>The modified string value of the entry field</returns>
        public static string NumericalEntryArea(string value, string label,float min, float max)
        {
            GUILayout.Label(label, float.TryParse(value, out f) && CheckRange(f, min, max) ? Skin.label : ColouredLabel(XKCDColors.Red));
            return GUILayout.TextField(value, 20);
        }
        #endregion
    }
}
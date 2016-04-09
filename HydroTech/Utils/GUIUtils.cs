using System.Collections.Generic;
using UnityEngine;

namespace HydroTech.Utils
{
    //TODO: add some caching on coloured GUI styles to prevent creating the same styles over again
    public static class GUIUtils
    {
        #region Skins
        private static bool usingDefaultSkin;
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
        public static GUISkin Skin { get; private set; }
        #endregion

        #region Constructor
        static GUIUtils()
        {
            kspSkin = HighLogic.Skin;
            unitySkin = new GUISkin();

            //TODO: when custom settings are implemented, set the right one to start with
            Skin = kspSkin;
            
            WrapButton = new GUIStyle(Skin.button) { wordWrap = true };
        }
        #endregion

        #region Buttons
        public static GUIStyle WrapButton { get; private set; }

        private static readonly Dictionary<Color, GUIStyle> buttonStyles = new Dictionary<Color, GUIStyle>();
        private static readonly Dictionary<Color, GUIStyle> wrapButtonStyles = new Dictionary<Color, GUIStyle>();
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
        #endregion

        #region Labels
        private static readonly Dictionary<Color, GUIStyle> labelStyles = new Dictionary<Color, GUIStyle>();
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
        #endregion
    }
}
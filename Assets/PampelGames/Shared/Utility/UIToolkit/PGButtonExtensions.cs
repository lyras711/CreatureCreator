// ---------------------------------------------------
// Copyright (c) Pampel Games e.K. All Rights Reserved.
// https://www.pampelgames.com
// ---------------------------------------------------

using UnityEngine;
using UnityEngine.UIElements;

namespace PampelGames.Shared.Utility
{
    public static class PGButtonExtensions
    {

        /// <summary>
        ///     Creates the default dropdown-style button with blue hovering.
        /// </summary>
        public static void PGButtonDropdown(this Button button, float height = 20f, float width = 85f)
        {
            button.PGMargin(0);
            button.PGPadding(6,6,0,1);
            button.style.height = height;
            button.style.width = width;
            button.PGBackgroundColorHover(PGColorsUtility.UnitySceneHeaderBlue());
        }
        
        
        /// <summary>
        ///     Creates a big, dark button with blue hovering.
        /// </summary>
        public static void PGButtonBig(this Button button, float height = 50f, float width = 200f)
        {
            button.style.unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold);
            button.style.height = height;
            button.style.width = width;
            button.style.marginTop = 5;
            button.style.marginBottom = 5;
            button.style.backgroundColor = PGColorsUtility.UnityEventHeader();
            button.PGBackgroundColorHover(PGColorsUtility.UnitySceneHeaderBlue());
            button.PGBorderWidth(5);
            button.PGBorderRadius(3);
        }

        

        
    }
}

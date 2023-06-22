// ---------------------------------------------------
// Copyright (c) Pampel Games e.K. All Rights Reserved.
// https://www.pampelgames.com
// ---------------------------------------------------

using UnityEngine;
using UnityEngine.UIElements;

namespace PampelGames.Shared.Utility
{
    public static class PGListViewUtility
    {
        /// <summary>
        ///     Used for the ListView.makeItem function. Creates a VisualElement "itemWrapper" with two children: Label "itemLabel" and Button "itemRemove".
        ///     If you want to parent an Element to the item, the list should be reorderable and then use "itemWrapper.parent.Add(Visualization);"
        /// </summary>
        public static VisualElement MakeItemWrapperReorderable()
        {
            var itemWrapper = new VisualElement();
            itemWrapper.name = "itemWrapper";
            itemWrapper.style.flexDirection = FlexDirection.Row;

            var itemLabel = new Label();
            itemLabel.name = "itemLabel";
            itemLabel.style.width = 180;
            itemLabel.style.unityTextAlign = TextAnchor.MiddleLeft;
            itemLabel.style.color = PGColorsUtility.InspectorVariableText();
            itemLabel.style.flexGrow = 1f;
            itemLabel.style.unityTextAlign = TextAnchor.MiddleLeft;
            itemLabel.PGBackgroundColorHover(PGColorsUtility.ListViewHover());
            itemLabel.style.unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold);

            var itemRemove = new Button();
            itemRemove.name = "itemRemove";
            itemRemove.text = "-";
            itemRemove.tooltip = "Remove";
            itemRemove.PGBackgroundColorHover(PGColorsUtility.HoverButtonRed());

            itemWrapper.Add(itemLabel);
            itemWrapper.Add(itemRemove);

            return itemWrapper;
        }

        public static VisualElement MakeItemWrapperNotReorderable()
        {
            var itemWrapperParent = new VisualElement();
            itemWrapperParent.style.backgroundColor = PGColorsUtility.ListViewBackground();
            itemWrapperParent.style.marginLeft = 8f;

            var itemWrapper = new VisualElement();
            itemWrapper.name = "itemWrapper";
            itemWrapper.style.flexDirection = FlexDirection.Row;

            var itemLabel = new Label();
            itemLabel.name = "itemLabel";
            itemLabel.style.width = 180;
            itemLabel.style.unityTextAlign = TextAnchor.MiddleLeft;
            itemLabel.style.color = PGColorsUtility.InspectorVariableText();
            itemLabel.style.flexGrow = 1f;
            itemLabel.style.unityTextAlign = TextAnchor.MiddleLeft;
            itemLabel.PGBackgroundColorHover(PGColorsUtility.ListViewHover());
            itemLabel.style.unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold);

            var itemRemove = new Button();
            itemRemove.name = "itemRemove";
            itemRemove.text = "-";
            itemRemove.tooltip = "Remove";
            itemRemove.PGBackgroundColorHover(PGColorsUtility.HoverButtonRed());

            itemWrapper.Add(itemLabel);
            itemWrapper.Add(itemRemove);
            itemWrapperParent.Add(itemWrapper);
            return itemWrapperParent;
        }
    }
}
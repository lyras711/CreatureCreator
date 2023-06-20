using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ColorPicker : MonoBehaviour
{
    public UnityEvent<Color> ColorPickerEvent;
    public Transform colorChartTransform;
    public Texture2D colorChart;
    public GameObject chart;
    public RectTransform cursor;
    public Image button;
    public Image cursorColor;

    public void PickColor(BaseEventData data)
    {
        PointerEventData pointer = data as PointerEventData;

        cursor.position = pointer.position;

        Color pickedColor = colorChart.GetPixel((int)(cursor.localPosition.x * (colorChart.width / colorChartTransform.GetChild(0).GetComponent<RectTransform>().rect.width)), (int)(cursor.localPosition.y * (colorChart.height / colorChartTransform.GetChild(0).GetComponent<RectTransform>().rect.height)));

        //button.color = pickedColor;
        cursorColor.color = pickedColor;
        ColorPickerEvent.Invoke(pickedColor);
    }
}

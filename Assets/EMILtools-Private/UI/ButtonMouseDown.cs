using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonMouseDown : MonoBehaviour, IPointerDownHandler
{
    public UnityEventPlus mouseDownHook;
    public void OnPointerDown(PointerEventData eventData)
    {
        mouseDownHook?.InvokeWithDelay(this);
    }
}

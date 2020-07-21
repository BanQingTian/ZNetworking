using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class ZBtn : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Action ClkEvent;
    public Action EnterEvent;
    public Action ExitEvent;

    public void OnPointerClick(PointerEventData eventData)
    {
        ClkEvent?.Invoke();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        EnterEvent?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ExitEvent?.Invoke();
    }

}

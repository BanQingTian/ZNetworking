using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NRKernal.NRExamples
{
    public class AnchorItem : MonoBehaviour, IPointerClickHandler
    {
        public string key;
        public Action<string, GameObject> OnAnchorItemClick;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (OnAnchorItemClick != null)
            {
                OnAnchorItemClick(key, gameObject);
            }
        }
    }
}

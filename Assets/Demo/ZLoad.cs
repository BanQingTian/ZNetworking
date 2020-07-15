using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using NRKernal;
using UnityEngine.UI;

public class ZLoad : MonoBehaviour,IPointerClickHandler{
    public void OnPointerClick(PointerEventData eventData)
    {
        LoadPngAsyn();
    }


    public void LoadPngAsyn()
    {
        if(doing == 0)
        {
            StartCoroutine(LoadPng());
        }
    }

    public RawImage Image;
    private Texture2D tex2d;
    private int doing = 0;
    public IEnumerator LoadPng()
    {
        doing++;
        string path = NRTools.GetSdcardPath() + "Assets/Demo/1.png";
        Debug.Log(path);
        WWW w = new WWW(path);
        while (!w.isDone)
        {
            Debug.Log(w.bytesDownloaded);
            yield return w;
        }
        if (string.IsNullOrEmpty(w.error))
        {
            tex2d = w.texture;
            Image.texture = tex2d;
            //Image.SetNativeSize();
        }
        else
        {
            Debug.LogError(w.error);
        }
        doing--;
    }
}

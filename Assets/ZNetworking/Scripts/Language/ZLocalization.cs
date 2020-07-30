using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ZLocalization : MonoBehaviour
{
    public Image ShowImage;

    public Sprite Sprite_en;
    public Sprite Sprite_ch;
    public Sprite Sprite_kr;

    private void Awake()
    {
        //ZLocalizationHelper.Instance.AddImageModule(this);
        SetSprite(Global.Languge);
    }

    public void SetSprite(LanguageEnum le)
    {
        Sprite sprite;
        switch (le)
        {
            case LanguageEnum.EN:
                sprite = Sprite_en;
                break;
            case LanguageEnum.CH:
                sprite = Sprite_ch;
                break;
            case LanguageEnum.KR:
                sprite = Sprite_kr;
                break;
            default:
                sprite = Sprite_en;
                break;
        }

        ShowImage.sprite = sprite == null ? Sprite_en : sprite;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZLocalizationHelper {

    private bool m_Initialized = false;

    private static ZLocalizationHelper m_Instance;
    public static ZLocalizationHelper Instance
    {
        get
        {
            if(m_Instance == null)
            {
                m_Instance = new ZLocalizationHelper();
            }
            return m_Instance;
        }
    }


    public List<ZLocalization> ImageMap = new List<ZLocalization>();

    
    public void AddImageModule(ZLocalization zl)
    {
        ImageMap.Add(zl);
    }

    public void Switch(LanguageEnum le)
    {
        for (int i = 0; i < ImageMap.Count; i++)
        {
            ImageMap[i].SetSprite(le);
        }
    }




}

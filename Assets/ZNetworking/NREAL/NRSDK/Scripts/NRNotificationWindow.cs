/****************************************************************************
* Copyright 2019 Nreal Techonology Limited. All rights reserved.
*                                                                                                                                                          
* This file is part of NRSDK.                                                                                                          
*                                                                                                                                                           
* https://www.nreal.ai/        
* 
*****************************************************************************/

using UnityEngine;
using UnityEngine.UI;
using System;

namespace NRKernal
{
    public class NRNotificationWindow : MonoBehaviour
    {
        [Serializable]
        public struct NotificationInfo
        {
            public Sprite sprite;
            public string title;
            public string message;
        }

        public NotificationInfo m_HighLevelInfo;
        public NotificationInfo m_MiddleLevelInfo;

        [SerializeField]
        private Image m_Icon;
        [SerializeField]
        private Text m_Title;
        [SerializeField]
        private Text m_Message;
        [SerializeField]
        private Button m_ConfirmBtn;

        private float m_Duration = 2f;

        public virtual void FillData(NRNotificationListener.Level level, float duration = 2f)
        {
            NotificationInfo info;

            switch (level)
            {
                case NRNotificationListener.Level.High:
                    info = m_HighLevelInfo;
                    break;
                case NRNotificationListener.Level.Middle:
                    info = m_MiddleLevelInfo;
                    m_ConfirmBtn?.gameObject.SetActive(false);
                    break;
                case NRNotificationListener.Level.Low:
                default:
                    GameObject.Destroy(gameObject);
                    return;
            }

            m_Title.text = info.title;
            m_Message.text = info.message;
            m_Duration = duration;
            m_Icon.sprite = info.sprite;

            m_ConfirmBtn?.onClick.AddListener(() =>
            {
                NRDevice.QuitApp();
            });

            if (m_Duration > 0)
            {
                Invoke("AutoDestroy", m_Duration);
            }
        }

        private void AutoDestroy()
        {
            GameObject.Destroy(gameObject);
        }
    }
}

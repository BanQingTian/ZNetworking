/****************************************************************************
* Copyright 2019 Nreal Techonology Limited. All rights reserved.
*                                                                                                                                                          
* This file is part of NRSDK.                                                                                                          
*                                                                                                                                                           
* https://www.nreal.ai/        
* 
*****************************************************************************/

namespace NRKernal.ObserverView
{
    using System.IO;
    using UnityEngine;
    using UnityEngine.UI;

    public class ConfigView : MonoBehaviour
    {
        public delegate void ConfigViewCallBack(ObserverViewConfig config);
        public event ConfigViewCallBack OnConfigrationChanged;
        public InputField m_IPField;
        public Toggle m_UseDebug;

        public GameObject m_StartBtn;
        public GameObject m_StopBtn;

        private ObserverViewConfig currentConfig;

        public static string ConfigPath
        {
            get
            {
                return Path.Combine(Application.persistentDataPath, "ObserverViewConfig.json");
            }
        }

        void Start()
        {
            m_IPField.onValueChanged.AddListener((value) =>
            {
                if (!value.Equals(currentConfig.serverIP))
                {
                    currentConfig.serverIP = value;
                    UpdateConfig();
                }

            });
            m_UseDebug.onValueChanged.AddListener((value) =>
            {
                if (value != currentConfig.useDebugUI)
                {
                    currentConfig.useDebugUI = value;
                    UpdateConfig();
                }
            });

            this.LoadConfig();
        }

        private void LoadConfig()
        {
            if (!File.Exists(ConfigPath))
            {
                Debug.Log("File is not exist :" + ConfigPath);
                currentConfig = new ObserverViewConfig("192.168.0.1");
            }
            else
            {
                Debug.Log("Load config from:" + ConfigPath);
                currentConfig = LitJson.JsonMapper.ToObject<ObserverViewConfig>(File.ReadAllText(ConfigPath));
            }
            m_IPField.text = currentConfig.serverIP;
            m_UseDebug.isOn = currentConfig.useDebugUI;
            OnConfigrationChanged?.Invoke(currentConfig);
        }

        // Config will Works at next run time.
        private void UpdateConfig()
        {
            var json = LitJson.JsonMapper.ToJson(currentConfig);
            File.WriteAllText(ConfigPath, json);
            Debug.Log("Save config :" + json);
            OnConfigrationChanged?.Invoke(currentConfig);
        }

        public void SwitchButton()
        {
            m_StartBtn.SetActive(!m_StartBtn.activeInHierarchy);
            m_StopBtn.SetActive(!m_StartBtn.activeInHierarchy);
        }
    }
}

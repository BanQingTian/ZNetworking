using System;
using UnityEngine;
using UnityEngine.UI;

namespace NRKernal.StreammingCast
{
    public class FPSConfigView : MonoBehaviour
    {
        private const string CacheServerIPKey = "FPSServerIP";
        public Action<string> OnClickStart;
        public Action OnClickStop;
        public InputField m_IPAddress;
        public Button m_StartBtn;
        public Button m_StopBtn;
        public Button m_HideBtn;
        public Transform m_PanelRoot;

        void Start()
        {
            m_IPAddress.text = PlayerPrefs.GetString(CacheServerIPKey);

            m_StartBtn.onClick.AddListener(() =>
            {
                PlayerPrefs.SetString(CacheServerIPKey, m_IPAddress.text);
                OnClickStart?.Invoke(m_IPAddress.text);

                HidePanel();
            });

            m_StopBtn.onClick.AddListener(() =>
            {
                OnClickStop?.Invoke();

                HidePanel();
            });

            m_HideBtn.onClick.AddListener(() =>
            {
                ShowPanel();
            });

            ShowPanel();
        }

        private void ShowPanel()
        {
            m_PanelRoot.gameObject.SetActive(true);
            m_HideBtn.gameObject.SetActive(false);
        }

        private void HidePanel()
        {
            m_PanelRoot.gameObject.SetActive(false);
            m_HideBtn.gameObject.SetActive(true);
        }
    }
}

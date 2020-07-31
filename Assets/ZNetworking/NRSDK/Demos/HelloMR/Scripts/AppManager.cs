using System;
using UnityEngine;

namespace NRKernal.NRExamples
{
    [DisallowMultipleComponent]
    [HelpURL("https://developer.nreal.ai/develop/discover/introduction-nrsdk")]
    public class AppManager : MonoBehaviour
    {
        //If enable this, quick click app button for three times, a profiler bar would show.
        public bool enableTriggerProfiler;

        private float m_LastClickTime = 0f;
        private int m_CumulativeClickNum = 0;
        private bool m_IsProfilerOpened = false;
        private float m_ButtonPressTimer;

        private const int TRIGGER_PROFILER_CLICK_COUNT = 3;
        private const float BUTTON_LONG_PRESS_DURATION = 1.2f;

        private void OnEnable()
        {
            NRInput.AddClickListener(ControllerHandEnum.Right, ControllerButton.HOME, OnHomeButtonClick);
            NRInput.AddClickListener(ControllerHandEnum.Left, ControllerButton.HOME, OnHomeButtonClick);
            NRInput.AddClickListener(ControllerHandEnum.Right, ControllerButton.APP, OnAppButtonClick);
            NRInput.AddClickListener(ControllerHandEnum.Left, ControllerButton.APP, OnAppButtonClick);

            NRDevice.OnAppQuit += OnApplicationQuit;
        }

        private void OnDisable()
        {
            NRInput.RemoveClickListener(ControllerHandEnum.Right, ControllerButton.HOME, OnHomeButtonClick);
            NRInput.RemoveClickListener(ControllerHandEnum.Left, ControllerButton.HOME, OnHomeButtonClick);
            NRInput.RemoveClickListener(ControllerHandEnum.Right, ControllerButton.APP, OnAppButtonClick);
            NRInput.RemoveClickListener(ControllerHandEnum.Left, ControllerButton.APP, OnAppButtonClick);
        }

        private void Update()
        {
#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                QuitApplication();
            }
#endif
            CheckButtonLongPress();
        }

        private void OnHomeButtonClick()
        {
            if (NRHomeMenu.IsShowing)
            {
                NRHomeMenu.Hide();
            }
            else
            {
                NRHomeMenu.Show();
            }
        }

        private void OnAppButtonClick()
        {
            if (enableTriggerProfiler)
            {
                CollectClickEvent();
            }
        }

        private void CheckButtonLongPress()
        {
            if (NRInput.GetButton(ControllerButton.HOME))
            {
                m_ButtonPressTimer += Time.deltaTime;
                if (m_ButtonPressTimer > BUTTON_LONG_PRESS_DURATION)
                {
                    m_ButtonPressTimer = float.MinValue;
                    //NRHomeMenu.Show();
                    NRInput.RecenterController();
                }
            }
            else
            {
                m_ButtonPressTimer = 0f;
            }
        }

        private void CollectClickEvent()
        {
            if (Time.unscaledTime - m_LastClickTime < 0.2f)
            {
                m_CumulativeClickNum++;
                if (m_CumulativeClickNum == (TRIGGER_PROFILER_CLICK_COUNT - 1))
                {
                    // Show the VisualProfiler
                    NRVisualProfiler.Instance.Switch(!m_IsProfilerOpened);
                    m_IsProfilerOpened = !m_IsProfilerOpened;
                    m_CumulativeClickNum = 0;
                }
            }
            else
            {
                m_CumulativeClickNum = 0;
            }
            m_LastClickTime = Time.unscaledTime;
        }

        public static void QuitApplication()
        {
            NRDevice.QuitApp();
        }

        private void OnApplicationQuit()
        {
            Debug.Log("OnApplicationQuit");
        }
    }
}

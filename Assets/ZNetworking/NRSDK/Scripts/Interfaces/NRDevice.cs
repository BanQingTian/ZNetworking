/****************************************************************************
* Copyright 2019 Nreal Techonology Limited. All rights reserved.
*                                                                                                                                                          
* This file is part of NRSDK.                                                                                                          
*                                                                                                                                                           
* https://www.nreal.ai/        
* 
*****************************************************************************/

namespace NRKernal
{
    using AOT;
    using System;
    using System.Runtime.InteropServices;
    using UnityEngine;

    /// <summary>
    /// Manage the HMD device and quit 
    /// </summary>
    public partial class NRDevice : SingleTon<NRDevice>
    {
        public enum GlassesEventType
        {
            PutOn,
            PutOff,
            PlugOut
        }
        public delegate void GlassesEvent(GlassesEventType glssevent);
        public delegate void GlassedTempLevelChanged(GlassesTemperatureLevel level);
        public delegate void AppQuitEvent();
        public static AppQuitEvent OnAppQuit;
        public static event GlassesEvent OnGlassesStateChanged;
        public static event GlassedTempLevelChanged OnGlassesTempLevelChanged;

        private NativeHMD m_NativeHMD;
        public NativeHMD NativeHMD
        {
            get
            {
                if (!m_IsInit)
                {
                    this.Init();
                }
                return m_NativeHMD;
            }
        }

        private readonly object m_Lock = new object();

        private NativeGlassesController m_NativeGlassesController;
        public NativeGlassesController NativeGlassesController
        {
            get
            {
                if (!m_IsInit)
                {
                    this.Init();
                }
                return m_NativeGlassesController;
            }
        }

        private bool m_IsInit = false;
        private static bool isGlassesPlugOut = false;

#if UNITY_ANDROID && !UNITY_EDITOR
        private static AndroidJavaObject m_UnityActivity;
#endif

        /// <summary>
        /// Init HMD device.
        /// </summary>
        public void Init()
        {
            if (m_IsInit)
            {
                return;
            }
            NRTools.Init();
            MainThreadDispather.Initialize();
#if UNITY_ANDROID && !UNITY_EDITOR
            // Init before all actions.
            AndroidJavaClass cls_UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            m_UnityActivity = cls_UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            NativeApi.NRSDKInitSetAndroidActivity(m_UnityActivity.GetRawObject()); 

            CreateGlassesController();
            CreateHMD();
#endif
            m_IsInit = true;
        }

        public void Pause()
        {
            PauseGlassesController();
            PauseHMD();
        }

        public void Resume()
        {
            ResumeGlassesController();
            ResumeHMD();
        }

        #region HMD
        private void CreateHMD()
        {
#if !UNITY_EDITOR
            lock (m_Lock)
            {
                m_NativeHMD = new NativeHMD();
                m_NativeHMD.Create();
            }
#endif
        }

        private void PauseHMD()
        {
#if !UNITY_EDITOR
            lock (m_Lock)
            {
                m_NativeHMD?.Pause();
            }
#endif
        }

        private void ResumeHMD()
        {
#if !UNITY_EDITOR
            lock (m_Lock)
            {
                m_NativeHMD?.Resume();
            }
#endif
        }

        private void DestroyHMD()
        {
#if !UNITY_EDITOR
            lock (m_Lock)
            {
                m_NativeHMD?.Destroy();
                m_NativeHMD = null;
            }
#endif
        }
        #endregion

        #region Glasses Controller
        public GlassesTemperatureLevel TemperatureLevel
        {
            get
            {
                this.Init();
#if !UNITY_EDITOR
                return this.NativeGlassesController.GetTempratureLevel();
#else
                return GlassesTemperatureLevel.TEMPERATURE_LEVEL_NORMAL;
#endif
            }
        }

        private void CreateGlassesController()
        {
#if !UNITY_EDITOR
            try
            {
                lock (m_Lock)
                {
                    m_NativeGlassesController = new NativeGlassesController();
                    m_NativeGlassesController.Create();
                    m_NativeGlassesController.RegisGlassesWearCallBack(OnGlassesWear, 1);
                    m_NativeGlassesController.RegisGlassesPlugOutCallBack(OnGlassesPlugOut, 1);
                    m_NativeGlassesController.Start();
                }
            }
            catch (Exception)
            {
                throw;
            }
#endif
        }

        private void PauseGlassesController()
        {
#if !UNITY_EDITOR
            lock (m_Lock)
            {
                m_NativeGlassesController?.Pause();
            }
#endif
        }

        private void ResumeGlassesController()
        {
#if !UNITY_EDITOR
            lock (m_Lock)
            {
                m_NativeGlassesController?.Resume();
            }
#endif
        }

        private void DestroyGlassesController()
        {
#if !UNITY_EDITOR
            lock (m_Lock)
            {
                m_NativeGlassesController?.Stop();
                m_NativeGlassesController?.Destroy();
                m_NativeGlassesController = null;
            }
#endif
        }

        [MonoPInvokeCallback(typeof(NativeGlassesController.NRGlassesControlWearCallback))]
        private static void OnGlassesWear(UInt64 glasses_control_handle, int wearing_status, UInt64 user_data)
        {
            Debug.Log("[NRDevice] " + (wearing_status == 1 ? "Glasses put on" : "Glasses put off"));
            MainThreadDispather.QueueOnMainThread(() =>
            {
                OnGlassesStateChanged?.Invoke(wearing_status == 1 ? GlassesEventType.PutOn : GlassesEventType.PutOff);
            });
        }

        [MonoPInvokeCallback(typeof(NativeGlassesController.NRGlassesControlPlugOffCallback))]
        private static void OnGlassesPlugOut(UInt64 glasses_control_handle, UInt64 user_data)
        {
            if (isGlassesPlugOut)
            {
                return;
            }
            isGlassesPlugOut = true;

            Debug.Log("[NRDevice] OnGlassesPlugOut");
            CallAndroidkillProcess();
        }

        private static void CallAndroidkillProcess()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            AndroidJNI.AttachCurrentThread();
            AndroidJavaClass processClass = new AndroidJavaClass("android.os.Process");
            int myPid = processClass.CallStatic<int>("myPid");
            processClass.CallStatic("killProcess", myPid);
#endif
        }
        #endregion

        #region Quit
        /// <summary>
        /// Quit the app.
        /// </summary>
        public static void QuitApp()
        {
            Debug.Log("[NRDevice] Start To Quit Application...");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            ForceKill();
#endif
        }

        /// <summary>
        /// Force kill the app.
        /// </summary>
        public static void ForceKill(bool needrelease = true)
        {
            Debug.Log("[NRDevice] Start To kill Application...");
            if (needrelease)
            {
                NRInput.Destroy();
                NRSessionManager.Instance.DestroySession();
            }

            OnAppQuit?.Invoke();
#if UNITY_ANDROID && !UNITY_EDITOR
            if (m_UnityActivity != null)
            {
                m_UnityActivity.Call("finish");
            }
            CallAndroidkillProcess();
#endif
        }

        /// <summary>
        /// Destory HMD resource.
        /// </summary>
        public void Destroy()
        {
            DestroyGlassesController();
            DestroyHMD();
        }
        #endregion

        private struct NativeApi
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRSDKInitSetAndroidActivity(IntPtr android_activity);
#endif
        }
    }
}

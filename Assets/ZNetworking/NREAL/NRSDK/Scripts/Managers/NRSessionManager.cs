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
    using System;
    using System.IO;
    using UnityEngine;
    using System.Collections;
    using NRKernal.Record;
    using System.Threading.Tasks;

    /// <summary>
    ///  Manages AR system state and handles the session lifecycle.
    ///  this class, application can create a session, configure it, start/pause or stop it.
    /// </summary>
    public class NRSessionManager
    {
        private static readonly NRSessionManager m_Instance = new NRSessionManager();

        public static NRSessionManager Instance
        {
            get
            {
                return m_Instance;
            }
        }

        private LostTrackingReason m_LostTrackingReason = LostTrackingReason.INITIALIZING;
        /// <summary>
        /// Current lost tracking reason.
        /// </summary>
        public LostTrackingReason LostTrackingReason
        {
            get
            {
                return m_LostTrackingReason;
            }
        }

        private SessionState m_SessionState = SessionState.UnInitialized;

        public SessionState SessionState
        {
            get
            {
                return m_SessionState;
            }
            private set
            {
                m_SessionState = value;
            }
        }

        public NRSessionBehaviour NRSessionBehaviour { get; private set; }

        public NRHMDPoseTracker NRHMDPoseTracker { get; private set; }

        internal NativeInterface NativeAPI { get; private set; }

        private NRRenderer NRRenderer { get; set; }

        public NRVirtualDisplayer VirtualDisplayer { get; set; }

        public bool IsInitialized
        {
            get
            {
                return (SessionState != SessionState.UnInitialized
                    && SessionState != SessionState.Destroyed);
            }
        }

        public void CreateSession(NRSessionBehaviour session)
        {
            if (SessionState != SessionState.UnInitialized && SessionState != SessionState.Destroyed)
            {
                return;
            }

            if (NRSessionBehaviour != null)
            {
                NRDebugger.LogError("Multiple SessionBehaviour components cannot exist in the scene. " +
                  "Destroying the newest.");
                GameObject.DestroyImmediate(session.gameObject);
                return;
            }
            NRSessionBehaviour = session;
            NRHMDPoseTracker = session.GetComponent<NRHMDPoseTracker>();

            try
            {
                NRDevice.Instance.Init();
            }
            catch (Exception)
            {
                throw;
            }

            NativeAPI = new NativeInterface();
            AsyncTaskExecuter.Instance.RunAction(() =>
            {
                NRDebugger.Log("AsyncTaskExecuter: Create tracking");
                switch (NRHMDPoseTracker.TrackingMode)
                {
                    case NRHMDPoseTracker.TrackingType.Tracking6Dof:
                        NativeAPI.NativeTracking.Create();
                        NativeAPI.NativeTracking.SetTrackingMode(TrackingMode.MODE_6DOF);
                        break;
                    case NRHMDPoseTracker.TrackingType.Tracking3Dof:
                        NativeAPI.NativeTracking.Create();
                        NRSessionBehaviour.SessionConfig.PlaneFindingMode = TrackablePlaneFindingMode.DISABLE;
                        NRSessionBehaviour.SessionConfig.ImageTrackingMode = TrackableImageFindingMode.DISABLE;
                        NativeAPI.NativeTracking.SetTrackingMode(TrackingMode.MODE_3DOF);
                        break;
                    default:
                        break;
                }
            });

            NRKernalUpdater.Instance.OnUpdate -= Update;
            NRKernalUpdater.Instance.OnUpdate += Update;

            SessionState = SessionState.Initialized;

            LoadNotification();
        }

        private void LoadNotification()
        {
            if (this.NRSessionBehaviour.SessionConfig.EnableNotification &&
                GameObject.FindObjectOfType<NRNotificationListener>() == null)
            {
                GameObject.Instantiate(Resources.Load("NRNotificationListener"));
            }
        }

        private bool m_IsSessionError = false;
        public void OprateInitException(Exception e)
        {
            if (m_IsSessionError)
            {
                return;
            }
            m_IsSessionError = true;
            if (e is NRGlassesConnectError)
            {
                ShowErrorTips(NativeConstants.GlassesDisconnectErrorTip);
            }
            else if (e is NRSdkVersionMismatchError)
            {
                ShowErrorTips(NativeConstants.SdkVersionMismatchErrorTip);
            }
            else if (e is NRSdcardPermissionDenyError)
            {
                ShowErrorTips(NativeConstants.SdcardPermissionDenyErrorTip);
            }
            else
            {
                ShowErrorTips(NativeConstants.UnknowErrorTip);
            }
        }

        private void ShowErrorTips(string msg)
        {
            var sessionbehaviour = GameObject.FindObjectOfType<NRSessionBehaviour>();
            if (sessionbehaviour != null)
            {
                GameObject.Destroy(sessionbehaviour.gameObject);
            }
            var input = GameObject.FindObjectOfType<NRInput>();
            if (input != null)
            {
                GameObject.Destroy(input.gameObject);
            }
            var virtualdisplay = GameObject.FindObjectOfType<NRVirtualDisplayer>();
            if (virtualdisplay != null)
            {
                GameObject.Destroy(virtualdisplay.gameObject);
            }

            NRGlassesInitErrorTip errortips;
            if (sessionbehaviour.SessionConfig.ErrorTipsPrefab != null)
            {
                errortips = GameObject.Instantiate<NRGlassesInitErrorTip>(sessionbehaviour.SessionConfig.ErrorTipsPrefab);
            }
            else
            {
                errortips = GameObject.Instantiate<NRGlassesInitErrorTip>(Resources.Load<NRGlassesInitErrorTip>("NRErrorTips"));
            }
            errortips.Init(msg, () =>
            {
                NRDevice.QuitApp();
            });
        }

        private void Update()
        {
            if (SessionState == SessionState.Running)
            {
                m_LostTrackingReason = NativeAPI.NativeHeadTracking.GetTrackingLostReason();
            }

            NRFrame.OnUpdate();
        }

        public void SetConfiguration(NRSessionConfig config)
        {
            if (config == null)
            {
                return;
            }
            AsyncTaskExecuter.Instance.RunAction(() =>
            {
                NRDebugger.Log("AsyncTaskExecuter: UpdateConfig");
                NativeAPI.Configration.UpdateConfig(config);
            });
        }

        public void Recenter()
        {
            if (SessionState != SessionState.Running)
            {
                return;
            }
            AsyncTaskExecuter.Instance.RunAction(() =>
            {
                NativeAPI.NativeTracking.Recenter();
            });
        }

        public static void SetAppSettings(bool useOptimizedRendering)
        {
            Application.targetFrameRate = 60;
            QualitySettings.maxQueuedFrames = -1;
            QualitySettings.vSyncCount = useOptimizedRendering ? 0 : 1;
            Screen.fullScreen = true;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }

        public void StartSession()
        {
            if (SessionState == SessionState.Running
                || SessionState == SessionState.UnInitialized
                || SessionState == SessionState.Destroyed)
            {
                return;
            }
            var config = NRSessionBehaviour.SessionConfig;

            if (config != null)
            {
                SetAppSettings(config.OptimizedRendering);
#if !UNITY_EDITOR
                if (config.OptimizedRendering)
                {
                    if (NRSessionBehaviour.gameObject.GetComponent<NRRenderer>() == null)
                    {
                        NRRenderer = NRSessionBehaviour.gameObject.AddComponent<NRRenderer>();
                        NRRenderer.Initialize(NRHMDPoseTracker.leftCamera, NRHMDPoseTracker.rightCamera);
                    }
                }
#endif
            }
            else
            {
                SetAppSettings(false);
            }

            AsyncTaskExecuter.Instance.RunAction(() =>
            {
                NRDebugger.Log("AsyncTaskExecuter: start tracking");
                NativeAPI.NativeTracking.Start();
                NativeAPI.NativeHeadTracking.Create();
                SessionState = SessionState.Running;
            });

#if UNITY_EDITOR
            InitEmulator();
#endif
        }

        public void DisableSession()
        {
            if (SessionState != SessionState.Running)
            {
                return;
            }

            // Do not put it in other thread...
            if (NRVirtualDisplayer.RunInBackground)
            {
                NRRenderer?.Pause();
                NativeAPI.NativeTracking?.Pause();
                VirtualDisplayer?.Pause();
                NRDevice.Instance.Pause();
                SessionState = SessionState.Paused;
            }
            else
            {
                NRDevice.ForceKill();
            }
        }

        public void SwitchTrackingMode(NRHMDPoseTracker.TrackingType trackingType)
        {
            if (SessionState != SessionState.Running)
            {
                return;
            }

#if !UNITY_EDITOR
            AsyncTaskExecuter.Instance.RunAction(() =>
            {
                NativeAPI.NativeTracking.SwitchTrackingMode((TrackingMode)trackingType);
            });
#endif

            NRFrame.ClearPose();
        }

        public void ResumeSession()
        {
            if (SessionState != SessionState.Paused)
            {
                return;
            }

            // Do not put it in other thread...
            VirtualDisplayer?.Resume();
            NativeAPI.NativeTracking.Resume();
            NRRenderer?.Resume();
            NRDevice.Instance.Resume();
            SessionState = SessionState.Running;
        }

        public void DestroySession()
        {
            if (SessionState == SessionState.Destroyed || SessionState == SessionState.UnInitialized)
            {
                return;
            }

            // Do not put it in other thread...
            SessionState = SessionState.Destroyed;
            NRRenderer?.Destroy();
            NativeAPI.NativeHeadTracking.Destroy();
            NativeAPI.NativeTracking.Destroy();
            VirtualDisplayer?.Destory();
            NRDevice.Instance.Destroy();

            FrameCaptureContextFactory.DisposeAllContext();
        }

        private void InitEmulator()
        {
            if (!NREmulatorManager.Inited && !GameObject.Find("NREmulatorManager"))
            {
                NREmulatorManager.Inited = true;
                GameObject.Instantiate(Resources.Load("Prefabs/NREmulatorManager"));
            }
            if (!GameObject.Find("NREmulatorHeadPos"))
            {
                GameObject.Instantiate(Resources.Load("Prefabs/NREmulatorHeadPose"));
            }
        }
    }
}

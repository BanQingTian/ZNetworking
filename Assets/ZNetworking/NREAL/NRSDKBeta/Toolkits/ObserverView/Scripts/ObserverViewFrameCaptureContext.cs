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
    using NRKernal.Record;
    using System;
    using UnityEngine;

    public class ObserverViewFrameCaptureContext
    {
        private ObserverViewBlender m_Blender;
        private AbstractFrameProvider m_FrameProvider;
        private NRObserverViewBehaviour m_CaptureBehaviour;
        private ObserverViewNetWorker m_NetWorker;
        private IEncoder m_Encoder;

        private CameraParameters m_CameraParameters;
        private bool m_IsInit = false;
        private bool m_IsReleased = false;

        public Texture PreviewTexture
        {
            get
            {
                return m_Blender?.BlendTexture;
            }
        }

        public NRObserverViewBehaviour GetBehaviour()
        {
            return m_CaptureBehaviour;
        }

        public AbstractFrameProvider GetFrameProvider()
        {
            return m_FrameProvider;
        }

        public CameraParameters RequestCameraParam()
        {
            return m_CameraParameters;
        }

        public IEncoder GetEncoder()
        {
            return m_Encoder;
        }

        public ObserverViewFrameCaptureContext()
        {
        }

        public void StartCaptureMode(CameraParameters param)
        {
            this.m_CameraParameters = param;
            this.m_CaptureBehaviour = this.GetObserverViewCaptureBehaviour();
            this.m_Encoder = new VideoEncoder();
            this.m_Encoder.Config(param);
            this.m_Blender = new ObserverViewBlender();
            this.m_Blender.Config(m_CaptureBehaviour.CaptureCamera, m_Encoder, param);
            this.m_NetWorker = new ObserverViewNetWorker(this);

            this.m_FrameProvider = new ObserverViewFrameProvider(m_CaptureBehaviour.CaptureCamera, this.m_CameraParameters.frameRate);
            this.m_FrameProvider.OnUpdate += this.m_Blender.OnFrame;
            this.m_IsInit = true;
        }

        private NRObserverViewBehaviour GetObserverViewCaptureBehaviour()
        {
            NRObserverViewBehaviour capture = GameObject.FindObjectOfType<NRObserverViewBehaviour>();
            Transform headParent = null;
            if (NRSessionManager.Instance.NRSessionBehaviour != null)
            {
                headParent = NRSessionManager.Instance.NRSessionBehaviour.transform.parent;
            }
            if (capture == null)
            {
                capture = GameObject.Instantiate(Resources.Load<NRObserverViewBehaviour>("Record/Prefabs/NRObserverViewBehaviour"), headParent);
            }
            GameObject.DontDestroyOnLoad(capture.gameObject);
            return capture;
        }

        public void StopCaptureMode()
        {
            this.Release();
        }

        public void StartCapture(string ip, Action<bool> callback)
        {
            if (!m_IsInit)
            {
                callback?.Invoke(false);
                return;
            }
            ((VideoEncoder)m_Encoder).EncodeConfig.SetOutPutPath(string.Format("rtp://{0}:5555", ip));
            m_NetWorker.CheckServerAvailable(ip, (result) =>
            {
                Debug.Log("[ObserverView] CheckServerAvailable : " + result);
                if (result)
                {
                    m_Encoder?.Start();
                    m_FrameProvider?.Play();
                }
                else
                {
                    this.Release();
                }
                callback?.Invoke(result);
            });
        }

        public void StopCapture()
        {
            if (!m_IsInit)
            {
                return;
            }
            m_FrameProvider?.Stop();
            m_Encoder?.Stop();
        }

        public void Release()
        {
            if (!m_IsInit || m_IsReleased)
            {
                return;
            }
            m_FrameProvider?.Release();
            m_Blender?.Dispose();
            m_Encoder?.Release();
            m_NetWorker?.Close();

            m_FrameProvider = null;
            m_Blender = null;
            m_Encoder = null;
            m_NetWorker = null;

            GameObject.Destroy(m_CaptureBehaviour.gameObject);
            m_CaptureBehaviour = null;

            m_IsInit = false;
            m_IsReleased = true;
        }
    }
}

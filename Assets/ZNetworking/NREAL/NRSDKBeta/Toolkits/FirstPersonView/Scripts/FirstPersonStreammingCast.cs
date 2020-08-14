using NRKernal.ObserverView;
using NRKernal.Record;
using System.Linq;
using UnityEngine;
using System;

namespace NRKernal.StreammingCast
{
    public class FirstPersonStreammingCast : MonoBehaviour
    {
        [SerializeField]
        private FPSConfigView m_FPSConfigView;
        private ObserverViewNetWorker m_NetWorker;
        private NRVideoCapture m_VideoCapture = null;

        private string serverIP = "192.168.31.6";

        public string RTPPath
        {
            get
            {
                return string.Format(@"rtp://{0}:5555", serverIP);
            }
        }

        private bool m_IsInitialized = false;

        void Start()
        {
            this.Init();
        }

        private void Init()
        {
            if (m_IsInitialized)
            {
                return;
            }
            m_NetWorker = new ObserverViewNetWorker();

            m_FPSConfigView.OnClickStart += (ip) =>
            {
                m_NetWorker.CheckServerAvailable(ip, (result) =>
                {
                    Debug.LogFormat("[FirstPersonStreammingCast] Is the server {0} ok? {1}", ip, result);
                    if (result)
                    {
                        serverIP = ip;
                        CastToServer();
                    }
                });
            };

            m_FPSConfigView.OnClickStop += () =>
            {
                StopVideoCapture();
            };

            m_IsInitialized = true;
        }

        private void CastToServer()
        {
            CreateVideoCapture(delegate ()
            {
                Debug.Log("[FirstPersonStreammingCast] Start video capture.");
                StartVideoCapture();
            });
        }

        #region network

        #endregion

        #region video capture
        private void CreateVideoCapture(Action callback)
        {
            Debug.Log("[FirstPersonStreammingCast] Created VideoCapture Instance!");

            if (m_VideoCapture != null)
            {
                callback?.Invoke();
                return;
            }

            NRVideoCapture.CreateAsync(false, delegate (NRVideoCapture videoCapture)
            {
                if (videoCapture != null)
                {
                    m_VideoCapture = videoCapture;

                    callback?.Invoke();
                }
                else
                {
                    Debug.LogError("Failed to create VideoCapture Instance!");
                }
            });
        }

        public void StartVideoCapture()
        {
            Resolution cameraResolution = NRVideoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();
            Debug.Log(cameraResolution);

            int cameraFramerate = NRVideoCapture.GetSupportedFrameRatesForResolution(cameraResolution).OrderByDescending((fps) => fps).First();
            Debug.Log(cameraFramerate);

            if (m_VideoCapture != null)
            {
                CameraParameters cameraParameters = new CameraParameters();
                cameraParameters.hologramOpacity = 1f;
                cameraParameters.frameRate = cameraFramerate;
                cameraParameters.cameraResolutionWidth = cameraResolution.width;
                cameraParameters.cameraResolutionHeight = cameraResolution.height;
                cameraParameters.pixelFormat = CapturePixelFormat.BGRA32;
                cameraParameters.blendMode = BlendMode.Blend;

                m_VideoCapture.StartVideoModeAsync(cameraParameters, OnStartedVideoCaptureMode);
            }
        }

        public void StopVideoCapture()
        {
            Debug.Log("[FirstPersonStreammingCast] Stop Video Capture!");
            m_VideoCapture.StopRecordingAsync(OnStoppedRecordingVideo);
        }

        void OnStartedVideoCaptureMode(NRVideoCapture.VideoCaptureResult result)
        {
            Debug.Log("[FirstPersonStreammingCast] Started Video Capture Mode!");
            m_VideoCapture.StartRecordingAsync(RTPPath, OnStartedRecordingVideo);
        }

        void OnStoppedVideoCaptureMode(NRVideoCapture.VideoCaptureResult result)
        {
            Debug.Log("[FirstPersonStreammingCast] Stopped Video Capture Mode!");
            m_VideoCapture = null;
        }

        void OnStartedRecordingVideo(NRVideoCapture.VideoCaptureResult result)
        {
            Debug.Log("[FirstPersonStreammingCast] Started Recording Video!");
        }

        void OnStoppedRecordingVideo(NRVideoCapture.VideoCaptureResult result)
        {
            Debug.Log("[FirstPersonStreammingCast] Stopped Recording Video!");
            m_VideoCapture.StopVideoModeAsync(OnStoppedVideoCaptureMode);
        }
        #endregion
    }
}

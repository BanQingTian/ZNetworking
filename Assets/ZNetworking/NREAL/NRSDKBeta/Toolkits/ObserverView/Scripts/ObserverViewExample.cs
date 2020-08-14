/****************************************************************************
* Copyright 2019 Nreal Techonology Limited. All rights reserved.
*                                                                                                                                                          
* This file is part of NRSDK.                                                                                                          
*                                                                                                                                                           
* https://www.nreal.ai/        
* 
*****************************************************************************/

using UnityEngine;
using NRKernal.ObserverView;
using NRKernal.Record;
using System.Linq;
using NRKernal.Persistence;

[RequireComponent(typeof(ImageTrackingAnchorTool))]
public class ObserverViewExample : MonoBehaviour
{
    public ImageTrackingAnchorTool PoseAlignmentTool;
    public NRPreviewer Previewer;
    private string serverIP = "192.168.31.6";

    NRObserverViewCapture m_ObserverCapture = null;
    ConfigView m_ConfigView;
    ObserverViewConfig m_currentConfig;

    void Start()
    {
        PoseAlignmentTool.OnAnchorPoseUpdate += UpdateObserverViewPose;
        CreateVideoCaptureTest();

        m_ConfigView = GameObject.FindObjectOfType<ConfigView>();
        m_ConfigView.OnConfigrationChanged += OnConfigrationChanged;
    }

    private void OnConfigrationChanged(ObserverViewConfig config)
    {
        Debug.Log("OnConfigrationChanged : " + config.ToString());
        this.m_currentConfig = config;
        this.serverIP = m_currentConfig.serverIP;
        if (m_ObserverCapture != null && m_ObserverCapture.GetContext().GetBehaviour() != null)
        {
            m_ObserverCapture.GetContext().GetBehaviour().SwitchDebugPanel(config.useDebugUI);
        }

        this.Previewer.SwitchPerview(config.useDebugUI);
    }

    void CreateVideoCaptureTest()
    {
        NRObserverViewCapture.CreateAsync(false, delegate (NRObserverViewCapture videoCapture)
        {
            if (videoCapture != null)
            {
                m_ObserverCapture = videoCapture;
            }
            else
            {
                Debug.LogError("Failed to create VideoCapture Instance!");
            }
        });
    }

    void UpdateObserverViewPose(Pose pose)
    {
        if (m_ObserverCapture != null)
        {
            var newposition = pose.position - pose.forward * 0.2f;
            var newpose = new Pose(newposition, pose.rotation);
            m_ObserverCapture.GetContext().GetBehaviour().UpdatePose(newpose);
        }
    }

    public void StartVideoCapture()
    {
        Resolution cameraResolution = NRObserverViewCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).Last();
        Debug.Log(cameraResolution);

        int cameraFramerate = NRObserverViewCapture.GetSupportedFrameRatesForResolution(cameraResolution).OrderByDescending((fps) => fps).First();
        Debug.Log(cameraFramerate);

        if (m_ObserverCapture != null)
        {
            Debug.Log("Created VideoCapture Instance!");
            CameraParameters cameraParameters = new CameraParameters();
            cameraParameters.hologramOpacity = 0.0f;
            cameraParameters.frameRate = cameraFramerate;
            cameraParameters.cameraResolutionWidth = cameraResolution.width;
            cameraParameters.cameraResolutionHeight = cameraResolution.height;
            cameraParameters.pixelFormat = CapturePixelFormat.BGRA32;

            m_ObserverCapture.StartObserverViewModeAsync(cameraParameters,
                NRObserverViewCapture.AudioState.ApplicationAndMicAudio,
                OnStartedVideoCaptureMode);

            Previewer.SetData(m_ObserverCapture.PreviewTexture, true);
        }
    }

    public void StopVideoCapture()
    {
        Debug.Log("Stop Video Capture!");
        m_ObserverCapture.StopObserverViewAsync(OnStoppedRecordingVideo);
        Previewer.SetData(m_ObserverCapture.PreviewTexture, false);
    }

    void OnStartedVideoCaptureMode(NRObserverViewCapture.ObserverViewCaptureResult result)
    {
        Debug.Log("Started Video Capture Mode!");
        m_ObserverCapture.StartObserverViewAsync(serverIP, OnStartedRecordingVideo);

        m_ObserverCapture.GetContext().GetBehaviour().SwitchDebugPanel(m_currentConfig.useDebugUI);

        // Set the camera culling musk
        //m_ObserverCapture.GetCaptureContext().GetBehaviour().SetCullingMask(~(1 << 11));
    }

    void OnStoppedVideoCaptureMode(NRObserverViewCapture.ObserverViewCaptureResult result)
    {
        Debug.Log("Stopped Video Capture Mode!");
    }

    void OnStartedRecordingVideo(NRObserverViewCapture.ObserverViewCaptureResult result)
    {
        Debug.Log("Started Recording Video!");
    }

    void OnStoppedRecordingVideo(NRObserverViewCapture.ObserverViewCaptureResult result)
    {
        Debug.Log("Stopped Recording Video!");
        m_ObserverCapture.StopObserverViewModeAsync(OnStoppedVideoCaptureMode);
    }
}

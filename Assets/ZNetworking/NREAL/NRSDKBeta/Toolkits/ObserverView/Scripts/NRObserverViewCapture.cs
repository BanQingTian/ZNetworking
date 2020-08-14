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
    using UnityEngine;
    using NRKernal.Record;
    using System.Collections.Generic;
    using System;

    public class NRObserverViewCapture : IDisposable
    {
        public NRObserverViewCapture()
        {
            IsRecording = false;
        }

        ~NRObserverViewCapture()
        {

        }

        private static IEnumerable<Resolution> m_SupportedResolutions = null;

        /// <summary>
        /// A list of all the supported device resolutions for observer view videos.
        /// </summary>
        public static IEnumerable<Resolution> SupportedResolutions
        {
            get
            {
                if (m_SupportedResolutions == null)
                {
                    var resolutions = new List<Resolution>();
                    var resolution1 = new Resolution();
                    resolution1.width = 1920;
                    resolution1.height = 1080;
                    resolutions.Add(resolution1);

                    var resolution2 = new Resolution();
                    resolution2.width = 1280;
                    resolution2.height = 720;
                    resolutions.Add(resolution2);

                    m_SupportedResolutions = resolutions;
                }

                return m_SupportedResolutions;
            }
        }

        /// <summary>
        /// Indicates whether or not the VideoCapture instance is currently recording video.
        /// </summary>
        public bool IsRecording { get; private set; }

        private ObserverViewFrameCaptureContext m_CaptureContext;

        public ObserverViewFrameCaptureContext GetContext()
        {
            return m_CaptureContext;
        }

        public Texture PreviewTexture
        {
            get
            {
                return m_CaptureContext?.PreviewTexture;
            }
        }

        public static void CreateAsync(bool showHolograms, OnObserverViewResourceCreatedCallback onCreatedCallback)
        {
            NRObserverViewCapture capture = new NRObserverViewCapture();
            capture.m_CaptureContext = new ObserverViewFrameCaptureContext();
            onCreatedCallback?.Invoke(capture);
        }

        /// <summary>
        /// Returns the supported frame rates at which a video can be recorded given a resolution.
        /// </summary>
        /// <param name="resolution">A recording resolution.</param>
        /// <returns>The frame rates at which the video can be recorded.</returns>
        public static IEnumerable<int> GetSupportedFrameRatesForResolution(Resolution resolution)
        {
            List<int> frameRates = new List<int>();
            frameRates.Add(30);
            return frameRates;
        }

        /// <summary>
        /// Dispose must be called to shutdown the PhotoCapture instance.
        /// </summary>
        public void Dispose()
        {
            if (m_CaptureContext != null)
            {
                m_CaptureContext.Release();
                m_CaptureContext = null;
            }
        }

        public void StartObserverViewModeAsync(CameraParameters setupParams, AudioState audioState, OnObserverViewModeStartedCallback onVideoModeStartedCallback)
        {
            setupParams.camMode = CamMode.VideoMode;
            m_CaptureContext.StartCaptureMode(setupParams);
            var result = new ObserverViewCaptureResult();
            result.resultType = CaptureResultType.Success;
            onVideoModeStartedCallback?.Invoke(result);
        }

        public void StartObserverViewAsync(string ip, OnStartedObserverViewCallback onStartedRecordingVideoCallback)
        {
            var captureResult = new ObserverViewCaptureResult();
            if (IsRecording)
            {
                captureResult.resultType = CaptureResultType.UnknownError;
                onStartedRecordingVideoCallback?.Invoke(captureResult);
            }
            else
            {
                m_CaptureContext.StartCapture(ip, (result) =>
                {
                    if (result)
                    {
                        IsRecording = true;
                        captureResult.resultType = CaptureResultType.Success;
                        onStartedRecordingVideoCallback?.Invoke(captureResult);
                    }
                    else
                    {
                        IsRecording = false;
                        captureResult.resultType = CaptureResultType.ServiceIsNotAvailable;
                        onStartedRecordingVideoCallback?.Invoke(captureResult);
                    }
                });
            }
        }

        public void StopObserverViewAsync(OnStoppedObserverViewCallback onStoppedRecordingVideoCallback)
        {
            var result = new ObserverViewCaptureResult();
            if (!IsRecording)
            {
                result.resultType = CaptureResultType.UnknownError;
                onStoppedRecordingVideoCallback?.Invoke(result);
            }
            else
            {
                try
                {
                    m_CaptureContext.StopCapture();
                }
                catch (Exception e)
                {
                    Debug.Log("Stop recording error :" + e.ToString());
                    throw;
                }
                IsRecording = false;
                result.resultType = CaptureResultType.Success;
                onStoppedRecordingVideoCallback?.Invoke(result);
            }
        }

        public void StopObserverViewModeAsync(OnObserverViewModeStoppedCallback onVideoModeStoppedCallback)
        {
            m_CaptureContext.StopCaptureMode();
            var result = new ObserverViewCaptureResult();
            result.resultType = CaptureResultType.Success;
            onVideoModeStoppedCallback?.Invoke(result);
        }

        /// <summary>
        /// Contains the result of the capture request.
        /// </summary>
        public enum CaptureResultType
        {
            /// <summary>
            /// Specifies that the desired operation was successful.
            /// </summary>
            Success,

            /// <summary>
            /// Service is not available.
            /// </summary>
            ServiceIsNotAvailable,

            /// <summary>
            /// Specifies that an unknown error occurred.
            /// </summary>
            UnknownError
        }

        /// <summary>
        /// Specifies what audio sources should be recorded while recording the video.
        /// </summary>
        public enum AudioState
        {
            /// <summary>
            /// Only include the mic audio in the video recording.
            /// </summary>
            MicAudio = 0,

            /// <summary>
            /// Only include the application audio in the video recording.
            /// </summary>
            ApplicationAudio = 1,

            /// <summary>
            /// Include both the application audio as well as the mic audio in the video recording.
            /// </summary>
            ApplicationAndMicAudio = 2,

            /// <summary>
            /// Do not include any audio in the video recording.
            /// </summary>
            None = 3
        }

        /// <summary>
        ///  A data container that contains the result information of a video recording operation.
        /// </summary>
        public struct ObserverViewCaptureResult
        {
            /// <summary>
            /// A generic result that indicates whether or not the VideoCapture operation succeeded.
            /// </summary>
            public CaptureResultType resultType;

            /// <summary>
            /// The specific HResult value.
            /// </summary>
            public long hResult;

            /// <summary>
            /// Indicates whether or not the operation was successful.
            /// </summary>
            public bool success
            {
                get
                {
                    return resultType == CaptureResultType.Success;
                }
            }
        }

        /// <summary>
        /// Called when the web camera begins recording the video.
        /// </summary>
        /// <param name="result">Indicates whether or not video recording started successfully.</param>
        public delegate void OnStartedObserverViewCallback(ObserverViewCaptureResult result);

        /// <summary>
        ///  Called when a VideoCapture resource has been created.
        /// </summary>
        /// <param name="captureObject">The VideoCapture instance.</param>
        public delegate void OnObserverViewResourceCreatedCallback(NRObserverViewCapture captureObject);

        /// <summary>
        /// Called when video mode has been started.
        /// </summary>
        /// <param name="result">Indicates whether or not video mode was successfully activated.</param>
        public delegate void OnObserverViewModeStartedCallback(ObserverViewCaptureResult result);

        /// <summary>
        /// Called when video mode has been stopped.
        /// </summary>
        /// <param name="result">Indicates whether or not video mode was successfully deactivated.</param>
        public delegate void OnObserverViewModeStoppedCallback(ObserverViewCaptureResult result);

        /// <summary>
        ///  Called when the video recording has been saved to the file system.
        /// </summary>
        /// <param name="result">Indicates whether or not video recording was saved successfully to the file system.</param>
        public delegate void OnStoppedObserverViewCallback(ObserverViewCaptureResult result);
    }
}

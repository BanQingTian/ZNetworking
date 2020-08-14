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
    using UnityEngine;
    using System.Collections;

    /// <summary>
    /// HMDPoseTracker update the infomations of pose tracker.
    /// This component is used to initialize the camera parameter, update the device posture, 
    /// In addition, application can change TrackingType through this component.
    /// </summary>
    [HelpURL("https://developer.nreal.ai/develop/discover/introduction-nrsdk")]
    public class NRHMDPoseTracker : MonoBehaviour
    {
        public delegate void HMDPoseTrackerEvent();
        public static event HMDPoseTrackerEvent OnHMDPoseReady;
        public static event HMDPoseTrackerEvent OnHMDLostTracking;

        /// <summary>
        /// HMD tracking type
        /// </summary>
        public enum TrackingType
        {
            /// <summary>
            /// Track the position an rotation.
            /// </summary>
            Tracking6Dof = 0,

            /// <summary>
            /// Track the rotation only.
            /// </summary>
            Tracking3Dof = 1,

            /// <summary>
            /// Track nothing.
            /// </summary>
            Tracking0Dof = 2,
        }

        [SerializeField]
        private TrackingType m_TrackingType = TrackingType.Tracking6Dof;

        public TrackingType TrackingMode
        {
            get
            {
                return m_TrackingType;
            }
        }

        /// <summary>
        /// Use relative coordinates or not.
        /// </summary>
        public bool UseRelative = false;
        private LostTrackingReason m_LastReason = LostTrackingReason.INITIALIZING;

        public Camera leftCamera;
        public Camera centerCamera;
        public Camera rightCamera;

        void Awake()
        {
#if UNITY_EDITOR
            leftCamera.cullingMask = 0;
            rightCamera.cullingMask = 0;
            centerCamera.cullingMask = -1;
            centerCamera.depth = 1;
#else
            centerCamera.cullingMask = 0;
            centerCamera.nearClipPlane = 0.3f;
            centerCamera.farClipPlane = 0.5f;
            centerCamera.clearFlags = CameraClearFlags.Nothing;
#endif
            StartCoroutine(Initialize());
        }

        void LateUpdate()
        {
            CheckHMDPoseState();
            UpdatePoseByTrackingType();
        }

        private void ChangeMode(TrackingType trackingtype)
        {
            NRSessionManager.Instance.SwitchTrackingMode(trackingtype);
            m_TrackingType = trackingtype;
        }

        public void ChangeTo6Dof() { ChangeMode(TrackingType.Tracking6Dof); }
        public void ChangeTo3Dof() { ChangeMode(TrackingType.Tracking3Dof); }
        public void ChangeTo0Dof() { ChangeMode(TrackingType.Tracking0Dof); }

        private IEnumerator Initialize()
        {
            while (NRFrame.SessionStatus != SessionState.Running)
            {
                NRDebugger.Log("[NRHMDPoseTracker] Waitting to initialize.");
                yield return new WaitForEndOfFrame();
            }

#if !UNITY_EDITOR
            bool result;
            var matrix_data = NRFrame.GetEyeProjectMatrix(out result, leftCamera.nearClipPlane, leftCamera.farClipPlane);
            if (result)
            {
                leftCamera.projectionMatrix = matrix_data.LEyeMatrix;
                rightCamera.projectionMatrix = matrix_data.REyeMatrix;

                var eyeposFromHead = NRFrame.EyePosFromHead;
                leftCamera.transform.localPosition = eyeposFromHead.LEyePose.position;
                leftCamera.transform.localRotation = eyeposFromHead.LEyePose.rotation;
                rightCamera.transform.localPosition = eyeposFromHead.REyePose.position;
                rightCamera.transform.localRotation = eyeposFromHead.REyePose.rotation;
                centerCamera.transform.localPosition = (leftCamera.transform.localPosition + rightCamera.transform.localPosition) * 0.5f;
                centerCamera.transform.localRotation = Quaternion.Lerp(leftCamera.transform.localRotation, rightCamera.transform.localRotation, 0.5f);
            }
#endif
            NRDebugger.Log("[NRHMDPoseTracker] Initialized success.");
        }

        private void UpdatePoseByTrackingType()
        {
            Pose pose = NRFrame.HeadPose;
            switch (m_TrackingType)
            {
                case TrackingType.Tracking6Dof:
                    if (UseRelative)
                    {
                        transform.localRotation = pose.rotation;
                        transform.localPosition = pose.position;
                    }
                    else
                    {
                        transform.rotation = pose.rotation;
                        transform.position = pose.position;
                    }
                    break;
                case TrackingType.Tracking3Dof:
                    if (UseRelative)
                    {
                        transform.localRotation = pose.rotation;
                        transform.localPosition = Vector3.zero;
                    }
                    else
                    {
                        transform.rotation = pose.rotation;
                        transform.position = Vector3.zero;
                    }
                    break;
                case TrackingType.Tracking0Dof:

                    break;
                default:
                    break;
            }

            centerCamera.transform.localPosition = (leftCamera.transform.localPosition + rightCamera.transform.localPosition) * 0.5f;
            centerCamera.transform.localRotation = Quaternion.Lerp(leftCamera.transform.localRotation, rightCamera.transform.localRotation, 0.5f);
        }

        private void CheckHMDPoseState()
        {
            if (NRFrame.SessionStatus != SessionState.Running || TrackingMode != TrackingType.Tracking6Dof)
            {
                return;
            }

            var currentReason = NRFrame.LostTrackingReason;
            // When LostTrackingReason changed.
            if (currentReason != m_LastReason)
            {
                if (currentReason != LostTrackingReason.NONE)
                {
                    OnHMDLostTracking?.Invoke();
                }
                else if (currentReason == LostTrackingReason.NONE)
                {
                    OnHMDPoseReady?.Invoke();
                }
                m_LastReason = currentReason;
            }
        }
    }
}

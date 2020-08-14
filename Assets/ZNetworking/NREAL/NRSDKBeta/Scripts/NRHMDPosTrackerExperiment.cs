/****************************************************************************
* Copyright 2019 Nreal Techonology Limited. All rights reserved.
*                                                                                                                                                          
* This file is part of NRSDK.                                                                                                          

* NRSDK is distributed in the hope that it will be usefull                                                              

* https://www.nreal.ai/           
* 
*****************************************************************************/

namespace NRKernal
{
    using UnityEngine;

    /// <summary>
    /// HMDPoseTracker oprate the pose tracking
    /// </summary>
    public class NRHMDPosTrackerExperiment : MonoBehaviour
    {
        public enum PoseSource
        {
            LeftEye = 0,
            RightEye = 1,
            CenterEye = 2,
        }

        public enum TrackingType
        {
            RotationAndPosition = 0,
            RotationOnly = 1,
            PositionOnly = 2
        }

        [SerializeField]
        private PoseSource m_PoseSource;
        [SerializeField]
        private TrackingType m_TrackingType;

        public bool UseRelativeTransform = false;

        private bool isInitialized = false;

        private void Init()
        {
            bool result;
            var leftCamera = transform.Find("LeftCamera").GetComponent<Camera>();
            var rightCamera = transform.Find("RightCamera").GetComponent<Camera>();
            var matrix_data = NRFrame.GetEyeProjectMatrix(out result, leftCamera.nearClipPlane, leftCamera.farClipPlane);
            if (result)
            {
                if (m_PoseSource == PoseSource.CenterEye)
                {
                    leftCamera.projectionMatrix = matrix_data.LEyeMatrix;
                    rightCamera.projectionMatrix = matrix_data.REyeMatrix;

                    var eyeposFromHead = NRFrame.EyePosFromHead;
                    leftCamera.transform.localPosition = eyeposFromHead.LEyePose.position;
                    leftCamera.transform.localRotation = eyeposFromHead.LEyePose.rotation;

                    rightCamera.transform.localPosition = eyeposFromHead.REyePose.position;
                    rightCamera.transform.localRotation = eyeposFromHead.REyePose.rotation;
                }
                else
                {
                    var matrix = m_PoseSource == PoseSource.LeftEye ? matrix_data.LEyeMatrix : matrix_data.REyeMatrix;
                    gameObject.GetComponent<Camera>().projectionMatrix = matrix;
                    NRDebugger.Log("[HMDPoseTracker Init] apply matrix:" + matrix.ToString());
                }

                isInitialized = true;
            }
        }

        public void Update()
        {
            if (!isInitialized)
            {
                this.Init();
            }

            UpdatePos();
        }

        private void UpdatePos()
        {
            switch (m_PoseSource)
            {
                case PoseSource.LeftEye:
                    UpdatePoseByeTrackingType(NRFrame.EyePose.LEyePose);
                    break;
                case PoseSource.RightEye:
                    UpdatePoseByeTrackingType(NRFrame.EyePose.REyePose);
                    break;
                case PoseSource.CenterEye:
                    UpdatePoseByeTrackingType(NRFrame.HeadPose);
                    break;
                default:
                    break;
            }
        }

        private void UpdatePoseByeTrackingType(Pose pose)
        {
            switch (m_TrackingType)
            {
                case TrackingType.RotationAndPosition:
                    if (UseRelativeTransform)
                    {
                        transform.localRotation = pose.rotation;
                        transform.localPosition = pose.position;
                    }
                    else
                    {
                        transform.rotation = pose.rotation;
                        transform.position = pose.position;
                    }
                    NRDebugger.LogError(pose.ToString());
                    break;
                case TrackingType.RotationOnly:
                    if (UseRelativeTransform)
                    {
                        transform.localRotation = pose.rotation;
                    }
                    else
                    {
                        transform.rotation = pose.rotation;
                    }
                    break;
                case TrackingType.PositionOnly:
                    if (UseRelativeTransform)
                    {
                        transform.localPosition = pose.position;
                    }
                    else
                    {
                        transform.position = pose.position;
                    }
                    break;
                default:
                    break;
            }
        }
    }
}

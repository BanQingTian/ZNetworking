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

    public class NRHMDGrayCameraRig : MonoBehaviour
    {
        [SerializeField]
        private Transform m_LeftCameraAnchor;

        [SerializeField]
        private Transform m_RightCameraAnchor;

        private bool m_Inited;

        internal Transform GetCameraAnchor(NativeGrayEye nativeGrayEye)
        {
            return nativeGrayEye == NativeGrayEye.RIGHT ? m_RightCameraAnchor : m_LeftCameraAnchor;
        }

        private void Update()
        {
            if (m_Inited)
            {
                return;
            }
            if(NRFrame.SessionStatus == SessionState.Running)
            {
                Init();
            }
        }

        private void Init()
        {
            SetGrayCameraAnchorsPose(NativeGrayEye.LEFT);
            SetGrayCameraAnchorsPose(NativeGrayEye.RIGHT);
            m_Inited = true;
        }

        private void SetGrayCameraAnchorsPose(NativeGrayEye nativeGrayEye)
        {
            var pose = NRDevice.Instance.NativeHMD.GetEyePoseFromHead(nativeGrayEye);
            var cameraAnchor = GetCameraAnchor(nativeGrayEye);
            cameraAnchor.localRotation = pose.rotation;
            cameraAnchor.localPosition = pose.position;
        }
    }
}

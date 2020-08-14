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

    public class NRObserverViewBehaviour : MonoBehaviour
    {
        [SerializeField]
        private Transform m_CameraRoot;
        [SerializeField]
        private GameObject m_DebugInfo;
        [SerializeField]
        public Camera CaptureCamera;

        NativeMat3f defualtFCC = new NativeMat3f(
           new Vector3(1402.06530528f, 0, 0),
           new Vector3(0, 1401.16300406f, 0),
           new Vector3(939.51336953f, 545.53574753f, 1)
       );

        void Awake()
        {
            m_DebugInfo.SetActive(false);
            //Use default fov.
            UpdateCameraParam(ProjectMatrixUtility.CalculateFOVByFCC(defualtFCC));
        }

        public void SwitchDebugPanel(bool isopen)
        {
            m_DebugInfo.SetActive(isopen);
        }

        public void UpdatePose(Pose pose)
        {
            m_CameraRoot.localPosition = pose.position;
            m_CameraRoot.localRotation = pose.rotation;
        }

        public void SetCullingMask(int i)
        {
            this.CaptureCamera.cullingMask = i;
        }

        public void UpdateCameraParam(Fov4f fov)
        {
            //Update fov.
            CaptureCamera.projectionMatrix = ProjectMatrixUtility.PerspectiveOffCenter(fov.left, fov.right, fov.bottom, fov.top,
                CaptureCamera.nearClipPlane, CaptureCamera.farClipPlane);
            NRDebugger.Log(CaptureCamera.projectionMatrix.ToString());
        }
    }
}

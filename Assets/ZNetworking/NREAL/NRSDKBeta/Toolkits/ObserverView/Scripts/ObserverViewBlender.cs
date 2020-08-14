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
    using UnityEngine;

    public class ObserverViewBlender
    {
        protected Camera m_TargetCamera;
        protected IEncoder m_Encoder;

        public int Width
        {
            get;
            private set;
        }

        public int Height
        {
            get;
            private set;
        }

        public Texture BlendTexture
        {
            get
            {
                return m_TargetCamera?.targetTexture;
            }
        }

        public virtual void Config(Camera camera, IEncoder encoder, CameraParameters param)
        {
            Width = param.cameraResolutionWidth;
            Height = param.cameraResolutionHeight;
            m_TargetCamera = camera;
            m_Encoder = encoder;

            m_TargetCamera.enabled = true;
            m_TargetCamera.backgroundColor = new Color(0, 0, 0, 0);
            m_TargetCamera.targetTexture = new RenderTexture(Width, Height, 24, RenderTextureFormat.ARGB32);
            m_TargetCamera.depthTextureMode = DepthTextureMode.Depth;
        }

        public void OnFrame(RGBTextureFrame frame)
        {
            // Commit frame                
            m_Encoder.Commit((RenderTexture)frame.texture, frame.timeStamp);
        }

        public void Dispose()
        {

        }
    }
}

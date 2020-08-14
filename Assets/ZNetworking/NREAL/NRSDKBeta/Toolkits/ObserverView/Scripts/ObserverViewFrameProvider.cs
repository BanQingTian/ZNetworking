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
    using System.Collections;
    using UnityEngine;

    public class ObserverViewFrameProvider : AbstractFrameProvider
    {
        private Camera m_SourceCamera;
        private RGBTextureFrame m_SourceFrame;
        private bool isPlay = false;
        private int _FPS;

        /// <summary>
        /// Init provider with the camera target texture.
        /// </summary>
        /// <param name="source">camera target texture</param>
        public ObserverViewFrameProvider(Camera camera, int fps = 30)
        {
            this.m_SourceCamera = camera;
            this._FPS = fps;

            NRKernalUpdater.Instance.StartCoroutine(UpdateFrame());
        }

        public IEnumerator UpdateFrame()
        {
            while (true)
            {
                if (isPlay)
                {
                    m_SourceFrame.texture = m_SourceCamera.targetTexture;
                    m_SourceFrame.timeStamp = NRTools.GetTimeStamp();
                    OnUpdate?.Invoke(m_SourceFrame);
                }
                yield return new WaitForSeconds(1 / _FPS);
            }
        }

        public override Resolution GetFrameInfo()
        {
            Resolution resolution = new Resolution();
            resolution.width = m_SourceFrame.texture.width;
            resolution.height = m_SourceFrame.texture.height;
            return resolution;
        }

        public override void Play()
        {
            isPlay = true;
        }

        public override void Stop()
        {
            isPlay = false;
        }

        public override void Release()
        {
            isPlay = false;
            NRKernalUpdater.Instance.StopCoroutine(UpdateFrame());
        }
    }
}

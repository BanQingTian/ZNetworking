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
    using System;

    public class NRGestureRecognizer
    {
        public Action<NRGestureInfo> OnGestureUpdate;
        public Action<NRGestureEventType, NRGestureInfo> OnGestureEventTriggered;

        public NRGestureRecognizer()
        {
            if (!NRGestureManager.Inited)
                NRGestureManager.Init();
            NRGestureManager.RigisterGestureRecognizer(this);
        }

        public void StartRecognize()
        {
            NRGestureManager.StartRecognize();
        }

        public void StopRecognize()
        {
            NRGestureManager.StopRecognize();
        }

        public void Destroy(bool stopReco = true)
        {
            if (stopReco)
                StopRecognize();
            OnGestureUpdate = null;
            OnGestureEventTriggered = null;
            NRGestureManager.UnRigisterGestureRecognizer(this);
        }
    }
}

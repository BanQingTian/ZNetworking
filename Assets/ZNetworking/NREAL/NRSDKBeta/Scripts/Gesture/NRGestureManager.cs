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
    using System.Collections.Generic;
    using UnityEngine;

    public class NRGestureManager : MonoBehaviour
    {
        private static List<NRGestureRecognizer> m_RecognizerList = new List<NRGestureRecognizer>();
        private static NativeGesture m_NativeGesture;

        private static bool m_Recongnizing;
        internal static bool Inited { get; private set; }

        internal static void Init()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (m_NativeGesture == null)
            {
                m_NativeGesture = new NativeGesture();
                m_NativeGesture.SetCallback(OnGestureUpdate);
            }
            Inited = true;
#endif
        }

        internal static void StartRecognize()
        {
            if (m_NativeGesture != null)
                m_NativeGesture.StartRecognize();
        }

        internal static void StopRecognize()
        {
            if (m_NativeGesture != null)
                m_NativeGesture.StopRecognize();
        }

        internal static void RigisterGestureRecognizer(NRGestureRecognizer gestureRecognizer)
        {
            if (Inited)
            {
                if (gestureRecognizer == null || m_RecognizerList.Contains(gestureRecognizer))
                    return;
                m_RecognizerList.Add(gestureRecognizer);
            }
        }

        internal static void UnRigisterGestureRecognizer(NRGestureRecognizer gestureRecognizer)
        {
            if (!Inited)
                Init();
            if (Inited)
            {
                if (gestureRecognizer == null)
                    return;
                if (m_RecognizerList.Contains(gestureRecognizer))
                    m_RecognizerList.Remove(gestureRecognizer);
            }
        }

        internal static void Destroy()
        {
            if (m_NativeGesture != null)
            {
                m_NativeGesture.Destroy();
                m_NativeGesture = null;
            }
        }

        private static void OnGestureUpdate(NRGestureInfo gestureInfo, NRGestureEventType gestureEventType)
        {
            for (int i = 0; i < m_RecognizerList.Count; i++)
            {
                NRGestureRecognizer gestureRecognizer = m_RecognizerList[i];
                if (gestureRecognizer == null)
                {
                    m_RecognizerList.Remove(gestureRecognizer);
                    continue;
                }
                gestureRecognizer.OnGestureUpdate?.Invoke(gestureInfo);
                if (gestureEventType != NRGestureEventType.UNDEFINED)
                    gestureRecognizer.OnGestureEventTriggered?.Invoke(gestureEventType, gestureInfo);
            }
        }
    }
}

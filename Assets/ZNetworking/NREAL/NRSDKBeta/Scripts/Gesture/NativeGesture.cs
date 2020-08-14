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
    using System.Runtime.InteropServices;
    using UnityEngine;

    /// <summary>
    /// Session Native API.
    /// </summary>
    internal partial class NativeGesture
    {
        private static Action<NRGestureInfo, NRGestureEventType> OnGestureUpdate;
        private static NRGestureInfo m_CurrentGestureInfo;

        private bool m_Valid;
        private UInt64 m_GestureHandle;
        private NRGestureEventType m_CurrentEventType = NRGestureEventType.UNDEFINED;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void NRGestureDataCallback(UInt64 gesture_handle, UInt64 gesture_data_handle, UInt64 user_data);

        public NativeGesture()
        {
            var result = NativeApi.NRGestureCreate(ref m_GestureHandle);
            NRDebugger.Log("[NativeGesture] NRGestureCreate: " + result.ToString());
            if (result == NativeResult.Success)
            {
                m_Valid = true;
                SetGestureDataCallback();
            }
        }

        internal void SetCallback(Action<NRGestureInfo, NRGestureEventType> callback)
        {
            if (m_CurrentGestureInfo == null)
                m_CurrentGestureInfo = new NRGestureInfo();
            OnGestureUpdate = callback;
        }

        internal void StartRecognize()
        {
            if (!m_Valid)
                return;
            var result = NativeApi.NRGestureStart(m_GestureHandle);
            NRDebugger.Log("[NativeGesture] NRGestureStart: " + result.ToString());
        }

        internal void StopRecognize()
        {
            if (!m_Valid)
                return;
            var result = NativeApi.NRGestureStop(m_GestureHandle);
            NRDebugger.Log("[NativeGesture] NRGestureStop: " + result.ToString());
        }

        internal void Destroy()
        {
            if (!m_Valid)
                return;
            OnGestureUpdate = null;
            var result = NativeApi.NRGestureDestroy(m_GestureHandle);
            NRDebugger.Log("[NativeGesture] NRGestureDestroy: " + result.ToString());
        }

        private static void OnGestureDataCallback(UInt64 gesture_handle, UInt64 gesture_data_handle, UInt64 userdata)
        {
            int out_gesture_type = 0;
            NativeApi.NRGestureGetGestureType(gesture_handle, gesture_data_handle, ref out_gesture_type);
            int out_event_type = 0;
            NativeApi.NRGestureGetEventType(gesture_handle, gesture_data_handle, ref out_event_type);
            NativeMat4f out_hand_pose = new NativeMat4f(Matrix4x4.identity);
            NativeApi.NRGestureGetHandPose(gesture_handle, gesture_data_handle, ref out_hand_pose);
            Pose unitypose;
            ConversionUtility.ApiPoseToUnityPose(out_hand_pose, out unitypose);

            //Debug.LogError(string.Format("[NativeGesture] gesture info: gesture_type:{0}, gesture_event:{1}", out_gesture_type, out_event_type));

            if(m_CurrentGestureInfo != null)
            {
                m_CurrentGestureInfo.gestureType = (NRGestureBasicType)out_gesture_type;
                m_CurrentGestureInfo.gesturePosition = unitypose.position;
                m_CurrentGestureInfo.gestureRotation = unitypose.rotation;
                OnGestureUpdate?.Invoke(m_CurrentGestureInfo, (NRGestureEventType)out_event_type);
            }
        }

        private bool SetGestureDataCallback()
        {
            var result = NativeApi.NRGestureSetCaptureCallback(m_GestureHandle, OnGestureDataCallback, 0);
            NRDebugger.Log("[NativeGesture] NRGestureSetCaptureCallback: " + result.ToString());
            return result == NativeResult.Success;
        }

        private struct NativeApi
        {
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRGestureCreate(ref UInt64 out_gesture_handle);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRGestureStart(UInt64 gesture_handle);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRGestureStop(UInt64 gesture_handle);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRGestureDestroy(UInt64 gesture_handle);

            [DllImport(NativeConstants.NRNativeLibrary, CallingConvention = CallingConvention.Cdecl)]
            public static extern NativeResult NRGestureSetCaptureCallback(UInt64 gesture_handle, NRGestureDataCallback gesture_callback, UInt64 user_data);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRGestureGetGestureType(UInt64 gesture_handle, UInt64 gesture_data_handle, ref int out_gesture_type);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRGestureGetEventType(UInt64 gesture_handle, UInt64 gesture_data_handle, ref int out_event_type);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRGestureGetHandPose(UInt64 gesture_handle, UInt64 gesture_data_handle, ref NativeMat4f out_hand_pose);
        }
    }
}

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
    using System;
    using UnityEngine;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Only Internal Test Not Release !!!!!
    /// </summary>
    internal partial class NativeHeadTracking
    {
        public NativeResult GetEyePose(ref Pose outLeftEyePose, ref Pose outRightEyePose)
        {
            NativeMat4f lefteyepos = new NativeMat4f(Matrix4x4.identity);
            NativeMat4f righteyepos = new NativeMat4f(Matrix4x4.identity);
            NativeResult result = NativeApi.NRHeadTrackingGetEyePose(m_NativeInterface.TrackingHandle, headTrackingHandle, ref lefteyepos, ref righteyepos);

            if (result == NativeResult.Success)
            {
                ConversionUtility.ApiPoseToUnityPose(lefteyepos, out outLeftEyePose);
                ConversionUtility.ApiPoseToUnityPose(righteyepos, out outRightEyePose);
            }
            NRDebugger.Log("[NativeHeadTracking] GetEyePose :" + result);
            return result;
        }

        public bool GetProjectionMatrix(ref Matrix4x4 outLeftProjectionMatrix, ref Matrix4x4 outRightProjectionMatrix)
        {
            NativeMat4f projectmatrix = new NativeMat4f(Matrix4x4.identity);
            NativeResult result_left = NativeApi.NRInternalGetProjectionMatrix((int)NativeEye.LEFT, ref projectmatrix);
            outLeftProjectionMatrix = projectmatrix.ToUnityMat4f();
            NativeResult result_right = NativeApi.NRInternalGetProjectionMatrix((int)NativeEye.RIGHT, ref projectmatrix);
            outRightProjectionMatrix = projectmatrix.ToUnityMat4f();
            return (result_left == NativeResult.Success && result_right == NativeResult.Success);
        }

        private partial struct NativeApi
        {
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRHeadTrackingGetEyePose(UInt64 session_handle, UInt64 head_tracking_handle, ref NativeMat4f left_eye, ref NativeMat4f right_eye);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRInternalGetProjectionMatrix(int index, ref NativeMat4f eye);
        };
    }
}

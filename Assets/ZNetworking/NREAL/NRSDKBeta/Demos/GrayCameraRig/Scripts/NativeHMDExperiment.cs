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

    public enum NativeGrayEye
    {
        LEFT = 3,
        RIGHT = 4
    }

    /// <summary>
    /// HMD Gray Eye offset Native API .
    /// </summary>
    public partial class NativeHMD
    {
        public Pose GetEyePoseFromHead(NativeGrayEye eye)
        {
            Pose outEyePoseFromHead = Pose.identity;
            NativeMat4f mat4f = new NativeMat4f(Matrix4x4.identity);
            NativeResult result = NativeApi.NRHMDGetEyePoseFromHead(m_HmdHandle, (int)eye, ref mat4f);
            if (result == NativeResult.Success)
            {
                ConversionUtility.ApiPoseToUnityPose(mat4f, out outEyePoseFromHead);
            }
            return outEyePoseFromHead;
        }
    }
}

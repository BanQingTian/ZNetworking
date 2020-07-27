/****************************************************************************
* Copyright 2019 Nreal Techonology Limited. All rights reserved.
*                                                                                                                                                          
* This file is part of NRSDK.                                                                                                          
*                                                                                                                                                           
* https://www.nreal.ai/                  
* 
*****************************************************************************/

using System.Runtime.InteropServices;

namespace NRKernal
{
    public enum NRCameraModel
    {
        NR_CAMERA_MODEL_RADIAL = 1,
        NR_CAMERA_MODEL_FISHEYE = 2,
    }

    /// <summary>
    /// if camera_model == NR_CAMERA_MODEL_RADIAL,the first 4 value of distortParams is:
    // radial_k1、radial_k2、radial_r1、radial_r2.
    // else if camera_model == NR_CAMERA_MODEL_FISHEYE:
    // fisheye_k1、fisheye_k2、fisheye_k3、fisheye_k4.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct NRDistortionParams
    {
        [MarshalAs(UnmanagedType.I4)]
        public NRCameraModel cameraModel;
        [MarshalAs(UnmanagedType.R4)]
        public float distortParams1;
        [MarshalAs(UnmanagedType.R4)]
        public float distortParams2;
        [MarshalAs(UnmanagedType.R4)]
        public float distortParams3;
        [MarshalAs(UnmanagedType.R4)]
        public float distortParams4;
        [MarshalAs(UnmanagedType.R4)]
        public float distortParams5;
        [MarshalAs(UnmanagedType.R4)]
        public float distortParams6;
        [MarshalAs(UnmanagedType.R4)]
        public float distortParams7;

        public override string ToString()
        {
            return string.Format("cameraModel:{0} distortParams1:{1} distortParams2:{2} distortParams3:{3} distortParams4:{4}",
                cameraModel, distortParams1, distortParams2, distortParams3, distortParams4);
        }
    }
}

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

    [StructLayout(LayoutKind.Sequential)]
    public struct PointCloudPoint
    {
        [MarshalAs(UnmanagedType.I8)]
        public Int64 Id;
        [MarshalAs(UnmanagedType.Struct)]
        public NativeVector3f Position;
        [MarshalAs(UnmanagedType.R4)]
        public float Confidence;

        public override string ToString()
        {
            return string.Format("id:{0} pos:{1} confidence:{2}", Id, Position, Confidence);
        }
    }
}

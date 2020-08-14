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
    using UnityEngine;
    using System.Runtime.InteropServices;

    /// <summary>
    /// HMD Eye offset Native API .
    /// </summary>
    internal partial class NativeRenderring
    {
        //public bool SetRenderMode(NativeRenderMode mode)
        //{
        //    Debug.Log("[NativeRenderring] SetRenderMode:" + mode.ToString());
        //    var result = NativeApi.NRRenderingSetRenderMode(m_RenderingHandle, mode);
        //    NativeErrorListener.Check(result, this, "SetRenderMode");
        //    return result == NativeResult.Success;
        //}

        //private partial struct NativeApi
        //{
        //    [DllImport(NativeConstants.NRNativeLibrary)]
        //    public static extern NativeResult NRRenderingSetRenderMode(UInt64 rendering_handle, NativeRenderMode render_mode);
        //}
    }
}

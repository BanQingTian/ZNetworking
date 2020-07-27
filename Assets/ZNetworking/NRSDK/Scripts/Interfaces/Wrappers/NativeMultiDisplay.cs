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
    internal partial class NativeMultiDisplay
    {
        public delegate void NRDisplayResolutionCallback(int width, int height);
        private UInt64 m_MultiDisplayHandle;

        public bool Create()
        {
            NativeResult result = NativeApi.NRDisplayCreate(ref m_MultiDisplayHandle);
            NativeErrorListener.Check(result, this, "Create", true);
            return result == NativeResult.Success;
        }

        public void InitColorSpace()
        {
            NativeColorSpace colorspace = QualitySettings.activeColorSpace == ColorSpace.Gamma ?
                NativeColorSpace.COLOR_SPACE_GAMMA : NativeColorSpace.COLOR_SPACE_LINEAR;
            NativeResult result = NativeApi.NRDisplayInitSetTextureColorSpace(m_MultiDisplayHandle, colorspace);
            NativeErrorListener.Check(result, this, "InitColorSpace");
        }

        public void Start()
        {
            NativeResult result = NativeApi.NRDisplayStart(m_MultiDisplayHandle);
            NativeErrorListener.Check(result, this, "Start", true);
        }

        public void ListenMainScrResolutionChanged(NRDisplayResolutionCallback callback)
        {
            NativeResult result = NativeApi.NRDisplaySetMainDisplayResolutionCallback(m_MultiDisplayHandle, callback);
            NativeErrorListener.Check(result, this, "ListenMainScrResolutionChanged");
        }

        public void Stop()
        {
            NativeResult result = NativeApi.NRDisplayStop(m_MultiDisplayHandle);
            NativeErrorListener.Check(result, this, "Stop");
        }

        public bool UpdateHomeScreenTexture(IntPtr rendertexture)
        {
            NativeResult result = NativeApi.NRDisplaySetMainDisplayTexture(m_MultiDisplayHandle, rendertexture);
            NativeErrorListener.Check(result, this, "UpdateHomeScreenTexture");
            return result == NativeResult.Success;
        }

        public bool Pause()
        {
            NativeResult result = NativeApi.NRDisplayPause(m_MultiDisplayHandle);
            NativeErrorListener.Check(result, this, "Pause");
            return result == NativeResult.Success;
        }

        public bool Resume()
        {
            NativeResult result = NativeApi.NRDisplayResume(m_MultiDisplayHandle);
            NativeErrorListener.Check(result, this, "Resume");
            return result == NativeResult.Success;
        }

        public bool Destroy()
        {
            NativeResult result = NativeApi.NRDisplayDestroy(m_MultiDisplayHandle);
            NativeErrorListener.Check(result, this, "Destroy");
            return result == NativeResult.Success;
        }

        private struct NativeApi
        {
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRDisplayCreate(ref UInt64 out_display_handle);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRDisplaySetMainDisplayResolutionCallback(UInt64 display_handle,
                NRDisplayResolutionCallback resolution_callback);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRDisplayInitSetTextureColorSpace(UInt64 display_handle,
                NativeColorSpace color_space);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRDisplayStart(UInt64 display_handle);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRDisplayStop(UInt64 display_handle);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRDisplayPause(UInt64 display_handle);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRDisplayResume(UInt64 display_handle);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRDisplaySetMainDisplayTexture(UInt64 display_handle,
                IntPtr controller_texture);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRDisplayDestroy(UInt64 display_handle);
        };
    }
}

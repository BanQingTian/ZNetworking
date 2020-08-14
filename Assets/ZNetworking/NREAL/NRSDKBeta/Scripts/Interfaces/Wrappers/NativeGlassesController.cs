/****************************************************************************
* Copyright 2019 Nreal Techonology Limited. All rights reserved.
*                                                                                                                                                          
* This file is part of NRSDK.                                                                                                          
*                                                                                                                                                           
* https://www.nreal.ai/        
* 
*****************************************************************************/

using System;
using System.Runtime.InteropServices;

namespace NRKernal
{
    public partial class NativeGlassesController
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void NRGlassesControlBrightnessKeyCallback(UInt64 glasses_control_handle, int key_event, UInt64 user_data);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void NRGlassesControlTemperatureCallback(UInt64 glasses_control_handle, int temperature, UInt64 user_data);

        public int GetDuty()
        {
            if (m_GlassesControllerHandle == 0)
            {
                return -1;
            }
            int duty = -1;
            var result = NativeApi.NRGlassesControlGetDuty(m_GlassesControllerHandle, ref duty);
            NativeErrorListener.Check(result, this, "GetDuty");
            return duty;
        }

        public void SetDuty(int duty)
        {
            if (m_GlassesControllerHandle == 0)
            {
                return;
            }
            var result = NativeApi.NRGlassesControlSetDuty(m_GlassesControllerHandle, duty);
            NativeErrorListener.Check(result, this, "SetDuty");
        }

        public int GetBrightness()
        {
            if (m_GlassesControllerHandle == 0)
            {
                return -1;
            }
            int brightness = -1;
            var result = NativeApi.NRGlassesControlGetBrightness(m_GlassesControllerHandle, ref brightness);
            NativeErrorListener.Check(result, this, "GetBrightness");
            return brightness;
        }

        public void SetBrightness(int brightness)
        {
            if (m_GlassesControllerHandle == 0)
            {
                return;
            }
            var result = NativeApi.NRGlassesControlSetBrightness(m_GlassesControllerHandle, brightness);
            NativeErrorListener.Check(result, this, "SetBrightness");
        }

        public bool RegisGlassesControlBrightnessKeyCallBack(NRGlassesControlBrightnessKeyCallback callback, ulong userdata)
        {
            if (m_GlassesControllerHandle == 0)
            {
                return false;
            }
            var result = NativeApi.NRGlassesControlSetBrightnessKeyCallback(m_GlassesControllerHandle, callback, userdata);
            NativeErrorListener.Check(result, this, "SetBrightnessKeyCallback");
            return result == NativeResult.Success;
        }

        public int GetTemprature(NativeGlassesTemperaturePosition temperatureType)
        {
            if (m_GlassesControllerHandle == 0)
            {
                return 0;
            }
            int temp = 0;
            var result = NativeApi.NRGlassesControlGetTemperatureData(m_GlassesControllerHandle, temperatureType, ref temp);
            NativeErrorListener.Check(result, this, "GetTemprature");
            return temp;
        }

        public string GetVersion()
        {
            if (m_GlassesControllerHandle == 0)
            {
                return string.Empty;
            }
            string version = "";
            var result = NativeApi.NRGlassesControlGetVersion(m_GlassesControllerHandle, ref version);
            NativeErrorListener.Check(result, this, "GetVersion");
            return version;
        }

        public NativeGlassesMode GetMode()
        {
            if (m_GlassesControllerHandle == 0)
            {
                return NativeGlassesMode.ThreeD_1080;
            }
            NativeGlassesMode mode = NativeGlassesMode.TwoD_1080;
            var result = NativeApi.NRGlassesControlGet2D3DMode(m_GlassesControllerHandle, ref mode);
            NativeErrorListener.Check(result, this, "GetMode");
            return mode;
        }

        public void SetMode(NativeGlassesMode mode)
        {
            if (m_GlassesControllerHandle == 0)
            {
                return;
            }
            var result = NativeApi.NRGlassesControlSet2D3DMode(m_GlassesControllerHandle, mode);
            NativeErrorListener.Check(result, this, "SetMode");
        }

        private partial struct NativeApi
        {
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRGlassesControlGetDuty(UInt64 glasses_control_handle, ref int out_dute);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRGlassesControlSetDuty(UInt64 glasses_control_handle, int dute);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRGlassesControlGetBrightness(UInt64 glasses_control_handle, ref int out_brightness);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRGlassesControlSetBrightness(UInt64 glasses_control_handle, int brightness);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRGlassesControlGetTemperatureData(UInt64 glasses_control_handle, NativeGlassesTemperaturePosition position, ref int out_temperature);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRGlassesControlGetVersion(UInt64 glasses_control_handle, ref string out_version);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRGlassesControlGet2D3DMode(UInt64 glasses_control_handle, ref NativeGlassesMode out_mode);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRGlassesControlSet2D3DMode(UInt64 glasses_control_handle, NativeGlassesMode mode);

            [DllImport(NativeConstants.NRNativeLibrary, CallingConvention = CallingConvention.Cdecl)]
            public static extern NativeResult NRGlassesControlSetBrightnessKeyCallback(UInt64 glasses_control_handle, NRGlassesControlBrightnessKeyCallback callback, UInt64 user_data);
        }
    }
}

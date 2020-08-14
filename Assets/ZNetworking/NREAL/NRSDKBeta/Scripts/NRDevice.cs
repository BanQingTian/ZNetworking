/****************************************************************************
* Copyright 2019 Nreal Techonology Limited. All rights reserved.
*                                                                                                                                                          
* This file is part of NRSDK.                                                                                                          
*                                                                                                                                                           
* https://www.nreal.ai/        
* 
*****************************************************************************/

using AOT;
using System;

namespace NRKernal
{
    partial class NRDevice : SingleTon<NRDevice>
    {
        public enum NRBrightnessKEYEvent
        {
            NR_BRIGHTNESS_KEY_DOWN = 0,
            NR_BRIGHTNESS_KEY_UP = 1,
        }

        public const int BRIGHTNESS_MIN = 0;
        public const int BRIGHTNESS_MAX = 7;

        public int Brightness
        {
            get
            {
                return this.NativeGlassesController.GetBrightness();
            }
            set
            {
                this.NativeGlassesController.SetBrightness(value);
            }
        }

        public NativeGlassesMode Mode
        {
            get
            {
                return this.NativeGlassesController.GetMode();
            }
            set
            {
                this.NativeGlassesController.SetMode(value);
            }
        }

        public string GlassesVersion
        {
            get
            {
                return this.NativeGlassesController.GetVersion();
            }
        }

        public static Action<NRBrightnessKEYEvent> OnBrightnessKeyCallback;

        [MonoPInvokeCallback(typeof(NativeGlassesController.NRGlassesControlBrightnessKeyCallback))]
        private static void OnBrightnessKeyCallbackInternal(UInt64 glasses_control_handle, int key_event, UInt64 user_data)
        {
            OnBrightnessKeyCallback?.Invoke((NRBrightnessKEYEvent)key_event);
        }

        public void RegisGlassesControllerExtraCallbacks()
        {
            NativeGlassesController.RegisGlassesControlBrightnessKeyCallBack(OnBrightnessKeyCallbackInternal, 0);
        }
    }
}

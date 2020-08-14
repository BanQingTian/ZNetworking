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
    /// <summary>
    /// NRSession holds information about NR Device's pose in the world coordinate, trackables, etc..
    /// </summary>
    public partial class NRFrame
    {
        private static EyePoseData m_EyePose;
        
        /// <summary>
        /// Get the pose of device in unity world coordinate.
        /// </summary>
        public static EyePoseData EyePose
        {
            get
            {
                if (SessionStatus == SessionState.Running)
                {
                    NRSessionManager.Instance.NativeAPI.NativeHeadTracking.GetEyePose(ref m_EyePose.LEyePose, ref m_EyePose.REyePose);
                }
                return m_EyePose;
            }
        }
    }
}

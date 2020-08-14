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

    public enum NRGestureBasicType
    {
        UNDEFINED = -1,
        FIST = 0,
        ONE = 1,
        ERROR = 2, //position error
        PALM = 3,
    }

    public enum NRGestureEventType
    {
        UNDEFINED = -1
    }

    public class NRGestureInfo
    {
        public NRGestureBasicType gestureType;
        public Vector3 gesturePosition;
        public Quaternion gestureRotation;

        public NRGestureInfo()
        {
            Clear();
        }

        public void Clear()
        {
            gestureType = NRGestureBasicType.UNDEFINED;
            gesturePosition = Vector3.zero;
            gestureRotation = Quaternion.identity;
        }

        public override string ToString()
        {
            return string.Format("[NRGestureInfo] gestureType: {0}, position: {1}, rotation: {2}", gestureType, gesturePosition.ToString("f3"), gestureRotation.ToString("f3"));
        }
    }
}

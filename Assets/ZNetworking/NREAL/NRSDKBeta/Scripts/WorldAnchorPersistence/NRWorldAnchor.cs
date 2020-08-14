﻿/****************************************************************************
* Copyright 2019 Nreal Techonology Limited. All rights reserved.
*                                                                                                                                                          
* This file is part of NRSDK.                                                                                                          
*                                                                                                                                                           
* https://www.nreal.ai/        
* 
*****************************************************************************/

namespace NRKernal.Persistence
{
    using UnityEngine;
    using System;

    /// <summary>
    /// The WorldAnchor component allows a GameObject's position to be locked in physical space.
    /// For example, a cube arranged on top of a physical desk with a WorldAnchor applied will remain fixed even as an observer walks around the room. This overrides all
    /// manipulation of the GameObject's position and orientation. To move the GameObject, first remove the WorldAnchor and manipulate the Transform normally.
    /// While it is generally recommended to use Destroy instead of DestroyImmediate, it's best to call DestroyImmediate on WorldAnchor objects. Doing so will let you 
    /// manipulate the Transform of the GameObject including adding a new WorldAnchor.
    /// </summary>
    internal class NRWorldAnchor : MonoBehaviour
    {
        public bool isLocated { get; }

        public event OnTrackingChangedDelegate OnTrackingChanged;

        public UInt64 AnchorNativeHandle
        {
            get;
            private set;
        }

#if !UNITY_EDITOR
        private NativeMapping m_NativeInterface;
#endif

        private TrackingState m_TrackingState = TrackingState.Tracking;
        /**
         * Get the tracking state of current trackable.
         */
        public TrackingState GetTrackingState()
        {
            TrackingState currentState;
#if !UNITY_EDITOR
            currentState = m_NativeInterface.GetTrackingState(AnchorNativeHandle);
#else
            currentState = TrackingState.Tracking;
#endif
            if (currentState != m_TrackingState)
            {
                OnTrackingChanged?.Invoke(this, currentState == TrackingState.Tracking);
            }

            m_TrackingState = currentState;

            return m_TrackingState;
        }

#if UNITY_EDITOR
        Pose anchorPos;

        private void Start()
        {
            anchorPos = new Pose(UnityEngine.Random.insideUnitSphere, Quaternion.identity);
        }
#endif
        /**
         * Get the center pose of current trackable.
         */
        public virtual Pose GetCenterPose()
        {
#if UNITY_EDITOR
            return anchorPos;
#else
            return m_NativeInterface.GetAnchorPose(AnchorNativeHandle);
#endif
        }

        /// <summary>
        /// Retrieve a native pointer to the Windows.Perception.Spatial.SpatialAnchor COM object. 
        /// This function calls IUnknown::AddRef on the pointer before returning it. 
        /// The pointer must be released by calling IUnknown::Release.
        /// </summary>
        /// <returns></returns>
        public int GetNativeSpatialAnchorPtr()
        {
#if UNITY_EDITOR
            return (int)AnchorNativeHandle;
#else
            return NativeMapping.GetAnchorNativeID(AnchorNativeHandle);
#endif
        }

        /// <summary>
        /// Assigns the Windows.Perception.Spatial.SpatialAnchor COM pointer maintained by this WorldAnchor.
        /// </summary>
        /// <param name="spatialAnchorPtr"></param>
        /// <param name="nativeinterface"></param>
        public void SetNativeSpatialAnchorPtr(UInt64 spatialAnchorPtr, NativeMapping nativeinterface)
        {
            AnchorNativeHandle = spatialAnchorPtr;
#if !UNITY_EDITOR
            m_NativeInterface = nativeinterface;
#endif
        }

        /// <summary>
        /// Update anchor's position while the state is Tracking.
        /// If not, anchor will be set to the position Vector3.one * 10000.
        /// </summary>
        internal void Update()
        {
            if (GetTrackingState() == TrackingState.Tracking)
            {
                var pose = GetCenterPose();
                transform.position = pose.position;
                transform.rotation = pose.rotation;
            }
        }

        /// <summary>
        /// Event that is fired when this object's tracking state changes.
        /// </summary>
        /// <param name="worldAnchor"></param>
        /// <param name="located"></param>
        public delegate void OnTrackingChangedDelegate(NRWorldAnchor worldAnchor, bool located);
    }
}
